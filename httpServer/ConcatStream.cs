using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/*
 * Concatinate two streams together.
 */
namespace CS422
{
    class ConcatStream : Stream
    {
        private Stream streamA;
        private Stream streamB;
        private long length;
        private long position;
        private bool streamBHasLength;
        private bool streamBHasSetLength;
        private int constructorUsed = 0;

        // Constructor 1
        public ConcatStream(Stream first, Stream second)
        {
            constructorUsed = 1;
            if (first == null || second == null) throw new ArgumentException("At least one stream parameter is NULL");
            if (first == second) throw new ArgumentException("Parameters must be two distict stream instances.");            
            streamA = first;
            if (streamA.CanSeek)
                streamA.Seek(0, SeekOrigin.Begin);
            streamB = second;
            if (streamB.CanSeek)
                streamB.Seek(0, SeekOrigin.Begin);            
            try { length = streamA.Length + streamB.Length; streamBHasLength = true; }
            catch (Exception e)
            {
                length = -1;
                long test_property = streamA.Length;
                streamBHasLength = false;
            }
            position = 0;
        }

        // Constructor 2
        public ConcatStream(Stream first, Stream second, long fixedLength)
        {
            constructorUsed = 2;
            if (first == null || second == null) throw new ArgumentException("At least one stream parameters is NULL");
            if (fixedLength < 0) throw new ArgumentException("Length parameter must be non-negative.");
            if (first == second) throw new ArgumentException("Parameters must be two distict stream instances.");            
            streamA = first;
            streamB = second;
            if (streamA.CanSeek)
                streamA.Seek(0, SeekOrigin.Begin);
            streamB = second;
            if (streamB.CanSeek)
                streamB.Seek(0, SeekOrigin.Begin);            
            try { length = streamA.Length + streamB.Length; streamBHasLength = true; length = fixedLength; }
            catch (Exception e)
            {
                length = streamA.Length;
                streamBHasLength = false;
                length = fixedLength;
            }
            position = 0;
        }

        // *******************************************************
        // Properties

        public override bool CanRead
        {
            get
            {
                if (streamA.CanRead && streamB.CanRead)
                    return true;
                else
                    return false;                
            }
        }

        public override bool CanSeek
        {
            get
            {
                if (streamA.CanSeek && streamB.CanSeek)
                    return true;
                else
                    return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                if (streamA.CanWrite && streamB.CanWrite)
                    return true;
                else
                    return false;
            }
        }

        public override long Length
        {
            get
            {
                if (length == -1)
                    throw new NotImplementedException();
                else
                    return length;
            }
        }

        public override void Close()
        {
            streamA.Close();
            streamB.Close();
        }

        public override long Position
        {
            get
            {
                return position;
            }

            set
            {
                if (streamBHasLength)
                {
                    // (value == 0 && length == 0)
                    if (value < 0 || (value > length && constructorUsed == 2)) throw new IndexOutOfRangeException();
                    position = value;
                    if (position < streamA.Length || (position == streamA.Length && streamA.Length == 0))
                    {
                        streamA.Position = position;
                        streamB.Position = 0;
                    }
                    else
                    {
                        streamA.Seek(0, SeekOrigin.End);
                        streamB.Position = position - streamA.Length;
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        //******************************************
        // Methods

        public override void Flush()
        {
            streamA.Flush();
            streamB.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (CanSeek)
            {
                if (origin == SeekOrigin.Begin)
                {
                    streamA.Seek(0, SeekOrigin.Begin);
                    streamB.Seek(0, SeekOrigin.Begin);
                    Position = 0;
                }
                if (origin == SeekOrigin.End)
                {                    
                    streamA.Seek(0, SeekOrigin.End);
                    streamB.Seek(0, SeekOrigin.End);
                    Position = streamB.Length + streamA.Length; //// - 2??
                }

                if (offset + Position > length && constructorUsed == 2) throw new IndexOutOfRangeException();

                if (Position < streamA.Length)
                {
                    if (Position + offset >= streamA.Length)
                    {
                        streamA.Seek(0, SeekOrigin.End);
                        offset -= streamA.Length - Position;
                        streamB.Position = offset;
                        position = streamA.Position + streamB.Position;
                    }
                    else
                    {
                        position += offset;
                        streamA.Position += offset;
                    }
                }
                else 
                {
                    if (Position + offset < streamA.Length)
                    {
                        offset += Position - streamA.Length;
                        streamB.Position = 0;
                        streamA.Position += offset;
                    }
                    else
                    {
                        position += offset;
                        streamB.Position += offset;
                    }
                }

                if (position > length) length = position;
                return Position;
                //TODO: WORKING ON SEEKING
                //Once we are at either the current, beginning, or end
                //How can we currently place the offset?

            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override void SetLength(long value)
        {
            length = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentException("Buffer cannot be null");
            if (count < 0) throw new ArgumentException("Count cannot be negative.");
            int totalRead = 0;
            if (CanRead)
            {
                int bytesRead = 0;
                if (CanRead)
                {
                    // Reading from only streamA
                    if (Position + count < streamA.Length)
                    {
                        bytesRead = streamA.Read(buffer, offset, count);
                        Position += bytesRead;
                        totalRead += bytesRead;
                    }
                    // Read from both A and B streams            
                    else if (Position < streamA.Length)
                    {

                        bytesRead = streamA.Read(buffer, offset, (int)(streamA.Length - Position));
                        totalRead += bytesRead;
                        count -= (int)(streamA.Length - Position);
                        Position += bytesRead;
                        bytesRead = streamB.Read(buffer, offset + bytesRead, count);
                        if (streamBHasLength)
                            Position += bytesRead;
                        else
                            position += bytesRead;
                        totalRead += bytesRead;
                    }
                    // Read from stream Bddd
                    else
                    {
                        bytesRead = streamB.Read(buffer, offset, count);
                        if (streamBHasLength)
                            Position += bytesRead;
                        else
                            position += bytesRead;
                        totalRead += bytesRead;
                    }
                }
                return totalRead;
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentException("Buffer cannot be null");
            if (count < 0) throw new ArgumentException("Count cannot be negative.");
            if (CanWrite)
            {
                // Write to streamA only
                Position += offset;
                if (Position + count < streamA.Length)
                {
                    streamA.Write(buffer, offset, count);
                }
                // Read from both A and B streams            
                else if (Position < streamA.Length)
                {
                    int bytesToWrite = Math.Min((int)streamA.Length - offset, count);
                    streamA.Write(buffer, offset, bytesToWrite);
                    Position += bytesToWrite;
                    count -= bytesToWrite;
                    if (streamBHasSetLength && count + Position > Length)
                        throw new OutOfMemoryException();
                    else
                    {
                        streamB.Write(buffer, offset + bytesToWrite, count);
                        Position += count;
                        if (position > length) length = position;
                    }
                }
                // Read from stream B
                else
                {
                    if (streamBHasSetLength && count + Position > Length)
                        throw new OutOfMemoryException();
                    else
                    {
                        streamB.Write(buffer, offset, count);
                        Position += count;
                        if (position > length) length = position;
                    }
                }
            }
            else
            {
                throw new ArgumentException();
            }
        }
    }
}
    