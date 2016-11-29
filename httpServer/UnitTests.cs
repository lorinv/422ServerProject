using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS422
{
    /*
     * Unit Tests 
    */
    class UnitTests
    {
        public UnitTests()
        {

        }

        public bool runTests()
        {
            // Read through all of the stream contents
            // Combines 2 mem streams, reads back all the data in random chumck sizes, verifies it against original data
            // Combines memory streams and noSeekMemStream and verfies that all data can be read
            // ConcatStream - query length property in both circumstances where it can and canot be computed without exception
            // NoSeekMemoryStream test - attempts to seek using all relevant properties/methods that provide seeking capabilities in a stream -
            // - Make sure each throws the NotSupportedException
            byte[] buffer = new byte[]{ 0,1,2,3,4,5,6,7,8};
            byte[] buffer2 = new byte[]{ 9,10,11,12,13,14,15,16};
            byte[] buffer3 = new byte[]{ 17,18,19,20,21,22,23,24,25};
            MemoryStream mem1 = new MemoryStream(buffer);
            MemoryStream mem2 = new MemoryStream(buffer2);

            NoSeekMemoryStream NoSeekMem1 = new NoSeekMemoryStream(buffer3);
            ConcatStream concatStream1 = new ConcatStream(mem1, mem2);
            ConcatStream concatStream2 = new ConcatStream(mem2, NoSeekMem1, 1);
            try
            {
                ConcatStream concatStream3 = new ConcatStream(NoSeekMem1, mem2);
                Console.WriteLine("Failed to Throw Length Error.");
            }
            catch (System.Exception e)
            {
                Console.WriteLine("Threw Proper Exception");
                //Console.WriteLine(e);
            }

            Console.WriteLine(noSeekTest(concatStream2));
            //readAllDataSequenciallyTest(concatStream1);
            //Console.WriteLine(lengthQueryTest(concatStream1, true));
            //Console.WriteLine(lengthQueryTest(concatStream2, true));
            //Console.WriteLine(lengthQueryTest(concatStream3, false));
            //readAllDataSequenciallyTest(concatStream3);

            return true;
        }

        public bool randomReadTest(ConcatStream c)
        {
            byte[] buffer = new byte[100];
            for (int i = 0; i < 20; i++)
                c.Read(buffer, 0, 1);

            return true;
        }

        public bool readAllDataSequenciallyTest(ConcatStream c1)
        {
            int OUTPUT_ARRAY_LENGTH = 100;
            byte[] output = new byte[OUTPUT_ARRAY_LENGTH];
            c1.Read(output, 0, OUTPUT_ARRAY_LENGTH);
            foreach (var i in output) { Console.Out.WriteLine(i); }
            return true;
        }

        public bool lengthQueryTest(ConcatStream c, bool shouldThrowException)
        {
            try { long length = c.Length; return (shouldThrowException == true); }
            catch { return (shouldThrowException == false); }                
        }

        public bool noSeekTest(Stream s)
        {
            try
            {
                if (!s.CanSeek)
                {
                    s.Seek(10, SeekOrigin.Begin);
                    s.Seek(10, SeekOrigin.End);                    
                }
                else
                {
                    return true;
                }
            }
            catch
            {
                return true;
            }
            return false;
            
        }
    }
}
