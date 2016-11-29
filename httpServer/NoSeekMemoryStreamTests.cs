using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS422
{
    class NoSeekMemoryStreamTests
    {
        static public bool Test()
        {
            Console.WriteLine("\nNoSeekMemoryStream: ");            

            SeekTests();            
            return true;
        }

        static public bool SeekTests()
        {
            Console.WriteLine("\nTesting Seek Functionality: ");
            byte[] b = new byte[80000];
            NoSeekMemoryStream ns = new NoSeekMemoryStream(b);

            Console.Write("\tTesting CanSeek, false expected: ");
            if (ns.CanSeek) Console.WriteLine("FAILED");
            else Console.WriteLine("SUCCESS");

            try
            {
                Console.Write("\tTesting Seeking function, exception expected: ");
                ns.Seek(10, System.IO.SeekOrigin.Begin);
                Console.WriteLine("FAILED");
            }
            catch
            {                
                Console.WriteLine("SUCCESS");
            }
            /*
            try
            {
                Console.Write("\tTesting setting position, exception expected: ");
                ns.Position = 12;
                Console.WriteLine("FAILED");
            }
            catch
            {
                Console.WriteLine("SUCCESS");
            }
            */
            return true;
        }

    }
}
