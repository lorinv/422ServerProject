using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CS422
{
    class Program
    {        
        static void Main(string[] args)
        {
            /*
            byte[] b = new byte[]{ 0, 1, 2, 3, 4, 5 };
            NoSeekMemoryStream n = new NoSeekMemoryStream(b);

            // n.Seek(1,System.IO.SeekOrigin.Begin);
            byte[] b2 = new byte[1000];
            n.Read(b2, 0, 1000);
            foreach(var i in b2) { Console.Out.Write(i); }
            
            UnitTests u = new UnitTests();
            u.runTests();
            */
            //StandardFileSystem fs = StandardFileSystem.Create("C:\\Users\\lt_va\\Documents");     

            //WebServer.AddService(new FilesWebService(fs));
            //WebServer.Start(9000, 10);
            //Thread.Sleep(10);
            //WebServer.Stop();                 

            TestBigNum test = new TestBigNum();

            //ConcatStreamUnitTests.Test();
            //NoSeekMemoryStreamTests.Test();
        }     
        
        class TestBigNum
        {
            public TestBigNum()
            {
                //TODO: Check if there are non digit values in the middle of a string

                //In ToString -- Add 'e' like 2e10
                //0.01010101010 -> 10101010101x2^-52
                //Different if 
                //Pow2 func - for loop, multply by itself n number of times
                //doubleToStringTest(double.MaxValue.ToString(), "-.01999");
                
                toStringTest("-10000000000000000000000000", "-10000000000000000000000000");
                toStringTest("10000000000000000000000000", "10000000000000000000000000");
                toStringTest("100000000.00000000000000001", "100000000.00000000000000001");
                toStringTest("-100000000.00000000000000001", "-100000000.00000000000000001");
                toStringTest("-100000000.000000000000000000000000000000000000001", "-100000000.000000000000000000000000000000000000001");
                toStringTest("100000000.000000000000000000000000000000000000001", "100000000.000000000000000000000000000000000000001");
                toStringTest("-10000000000000000000000000000000.000000000000000000000000000000000000001", "-10000000000000000000000000000000.000000000000000000000000000000000000001");
                toStringTest("-10", "-10");
                toStringTest("-1.01", "-1.01");
                toStringTest("-10.01", "-10.01");
                toStringTest("-100.001", "-100.001");
                toStringTest("100E1", "1000");
                toStringTest("100E2", "10000");
                toStringTest("100E-2", "1");



                /*
                doubleTest(1010.12345678, "1010.12345678");
                //toStringTest(double.MaxValue.ToString(), "1.79769313486232E+308");
                doubleToStringTest(0.01, ".01");
                doubleToStringTest(-0.01, "-.01");
                doubleToStringTest(-.01, "-.01");
                doubleToStringTest(99.99, "99.99");
                multiplicationTest("-1", "10000000", "-10000000");
                divideTest("2", "4", ".5");
                
                
                divideTest("9", "3", "3");
                divideTest("1", "-1", "-1");
                */
            }           

            private bool divideTest(string val1, string val2, string expectedOutput)
            {
                Console.WriteLine("Divide Test: " + val1 + " / " + val2);
                BigNum bn1 = new BigNum(val1);
                BigNum bn2 = new BigNum(val2);
                BigNum bn3 = bn1 / bn2;
                if (bn3.ToString() == expectedOutput)
                {
                    Console.WriteLine("True: Expected Value: " + expectedOutput + " | Resulting Value: " + bn3.ToString());                    
                    return true;
                }
                else
                {
                    Console.WriteLine("False: Expected Value: " + expectedOutput + " | Resulting Value: " + bn3.ToString());
                    return false;
                }
            }

            private bool multiplicationTest(string val1, string val2, string expectedOutput)
            {
                Console.WriteLine("Multiplication Test: " + val1 + " * " + val2);
                BigNum bn1 = new BigNum(val1);
                BigNum bn2 = new BigNum(val2);
                BigNum bn3 = bn1 * bn2;
                if (bn3.ToString() == expectedOutput)
                {
                    Console.WriteLine("True: Expected Value: " + expectedOutput + " | Resulting Value: " + bn3.ToString());
                    return true;
                }
                else
                {
                    Console.WriteLine("False: Expected Value: " + expectedOutput + " | Resulting Value: " + bn3.ToString());
                    return false;
                }
            }

            private bool additionTest(string val1, string val2, string expectedOutput)
            {
                Console.WriteLine("Addition Test: " + val1 + " + " + val2);
                BigNum bn1 = new BigNum(val1);
                BigNum bn2 = new BigNum(val2);
                BigNum bn3 = bn1 + bn2;
                if (bn3.ToString() == expectedOutput)
                {
                    Console.WriteLine("True: Expected Value: " + expectedOutput + " | Resulting Value: " + bn3.ToString());
                    return true;
                }
                else
                {
                    Console.WriteLine("False: Expected Value: " + expectedOutput + " | Resulting Value: " + bn3.ToString());
                    return false;
                }
            }

            private bool subtractionTest(string val1, string val2, string expectedOutput)
            {
                Console.WriteLine("Subtraction Test: " + val1 + " - " + val2);
                BigNum bn1 = new BigNum(val1);
                BigNum bn2 = new BigNum(val2);
                BigNum bn3 = bn1 - bn2;
                if (bn3.ToString() == expectedOutput)
                {
                    Console.WriteLine("True: Expected Value: " + expectedOutput + " | Resulting Value: " + bn3.ToString());
                    return true;
                }
                else
                {
                    Console.WriteLine("False: Expected Value: " + expectedOutput + " | Resulting Value: " + bn3.ToString());
                    return false;
                }
            }

            private bool toStringTest(string val1, string expectedOutput)
            {
                Console.WriteLine("ToString Test: " + val1);
                BigNum bn1 = new BigNum(val1);
                if (bn1.ToString() == expectedOutput)
                {
                    Console.WriteLine("True: Expected Value: " + expectedOutput + " | Resulting Value: " + bn1.ToString());
                    return true;
                }
                else
                {
                    Console.WriteLine("False: Expected Value: " + expectedOutput + " | Resulting Value: " + bn1.ToString());
                    return false;
                }
            }           

            private bool doubleToStringTest(double val1, string expectedOutput)
            {
                Console.WriteLine("doubleToString Test: " + expectedOutput);
                BigNum bn1 = new BigNum(val1, true);
                if (bn1.ToString() == expectedOutput)
                {
                    Console.WriteLine("True: Expected Value: " + expectedOutput + " | Resulting Value: " + bn1.ToString());
                    return true;
                }
                else
                {
                    Console.WriteLine("False: Expected Value: " + expectedOutput + " | Resulting Value: " + bn1.ToString());
                    return false;
                }
            }
            private bool doubleTest(double val1, string expectedOutput)
            {
                Console.WriteLine("double Test: " + expectedOutput);
                BigNum bn1 = new BigNum(val1, false);
                if (bn1.ToString() == expectedOutput)
                {
                    Console.WriteLine("True: Expected Value: " + expectedOutput + " | Resulting Value: " + bn1.ToString());
                    return true;
                }
                else
                {
                    Console.WriteLine("False: Expected Value: " + expectedOutput + " | Resulting Value: " + bn1.ToString());
                    return false;
                }
            }
        }   
    }
}
