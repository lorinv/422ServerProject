using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS422
{

    // Represents a memory stream that does not support seeking, but otherwise has
    // functionality identical to the MemoryStream class.
    public class NoSeekMemoryStream : MemoryStream
    {
        private long _position = 0;
        //Throw NotSupportedException if someone tries to seek
        public NoSeekMemoryStream(byte[] buffer) : base(buffer) { }
        public NoSeekMemoryStream(byte[] buffer, int index, int count) : base(buffer, index, count) { }


        public override bool CanSeek
        {
            get
            {
                return false;                 
            }
        }

        public override long Position
        {
            get
            {
                return base.Position;
            }
            set
            {                
                _position = value;
            }
        }            

        public override long Seek(long offset, SeekOrigin loc)
        {
            throw new NotSupportedException();
        }
    }
}
