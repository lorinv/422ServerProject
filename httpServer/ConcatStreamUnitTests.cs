using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

/*
 *OK: Found type CS422.ConcatStreamOK: Found type CS422.NoSeekMemoryStreamSuccess: Stream length is positive and seek to beginning okFailure: Failed to read exactly 653985 bytes from stream of length 653985 bytesSuccess: Verifed that stream passed to seek/read/write test function returns true for CanSeek, CanRead, and CanWrite.Failure: Stream length went from 653985 to 653985 after writing 10000000 bytes 
 
 
     */

namespace CS422
{
    class ConcatStreamUnitTests
    {        
        //Constructor 1 Tests
        static public bool Test()
        {
            Console.WriteLine("ConcatStream: ");            
            canWriteTest(); //Review this
            canReadTest(); //Review this
            LengthTest(); //Review this, test
            ReadStreamTests();
            PositionTests(); //NEED TO TEST IF underlying streams position changes.
            WriteTests();
            SeekTesting();
            //Close Testing        
            //Can Seek
            

            return true;
        }

        static bool SeekTesting()
        {
            Console.WriteLine("\nTesting Seeking Functionality: ");

            Console.Write("\tTesting 100 random seeks from beginning: ");
            FileStream fs1 = new FileStream("testFile1.txt", FileMode.Open, FileAccess.Read);
            FileStream fs2 = new FileStream("testFile1.txt", FileMode.Open, FileAccess.Read);
            ConcatStream cs1 = new ConcatStream(fs1, fs2);
            Random rn = new Random();
            bool success = true;
            int offset = 0;
            for (int i = 0; i < 100; i++)
            {
                offset = rn.Next(0, (int)cs1.Length);
                cs1.Seek(offset, SeekOrigin.Begin);
                if (offset != cs1.Position) { Console.WriteLine("FAILED"); break; success = false; }
            }
            if (success == true) Console.WriteLine("SUCCESS");

            /***********************/
            Console.Write("\tTesting random seeks from current: ");
            success = true;
            offset = -100;// rn.Next((int)cs1.Position, (int)cs1.Length);
            long oldPosition = cs1.Position;
            cs1.Seek(offset, SeekOrigin.Current);
            if (offset + oldPosition != cs1.Position) { Console.WriteLine("FAILED"); success = false; }            
            if (success == true) Console.WriteLine("SUCCESS");


            return true;
        }

        static bool WriteTests()
        {
            Console.WriteLine("\nTesting Concat Stream Write Functionality: ");

            //Can we write all the way through both buffers
            Console.Write("\tTesting write all the way through both buffers: ");
            FileStream fs1 = new FileStream("testFile11.txt", FileMode.Create, FileAccess.ReadWrite);
            string testString = "Checking 123";
            byte[] toBytes = Encoding.ASCII.GetBytes(testString);
            fs1.Write(toBytes, 0, toBytes.Length);
            FileStream fs2 = new FileStream("testFile12.txt", FileMode.Create, FileAccess.ReadWrite);
            testString = "IT's working!!!!";
            toBytes = Encoding.ASCII.GetBytes(testString);
            fs2.Write(toBytes, 0, toBytes.Length);
            ConcatStream cs1 = new ConcatStream(fs1, fs2);
            string msg = "";
            for(int i = 0; i < fs1.Length + fs2.Length + 20; i++)
            {
                msg += "a";
            }
            toBytes = Encoding.ASCII.GetBytes(msg);
            cs1.Write(toBytes, 0, toBytes.Length);
            cs1.Position = 0;
            toBytes = new byte[cs1.Length];
            int bytesRead = cs1.Read(toBytes, 0, toBytes.Length);
            string msg2 = System.Text.Encoding.ASCII.GetString(toBytes, 0, bytesRead);
            if (msg == msg2) Console.WriteLine("SUCCESS");
            else Console.WriteLine("FAILED");

            /********************************/
                        
            Console.Write("\tTesting writing NULL buffer, excetion expected: ");            
            try
            {
                cs1.Write(null, 0, 10);
                Console.WriteLine("FAILED");
            }
            catch
            {
                Console.WriteLine("SUCCESS");
            }

            cs1.Close();

            /**************************/

            //Can we write all the way through both buffers
            Console.Write("\tTesting writing passed a fixed length, expception expected: ");
            fs1 = new FileStream("testFile11.txt", FileMode.Open, FileAccess.ReadWrite);                        
            fs2 = new FileStream("testFile12.txt", FileMode.Open, FileAccess.ReadWrite);            
            cs1 = new ConcatStream(fs1, fs2, 10);            
            for (int i = 0; i < fs1.Length + fs2.Length + 200; i++)
            {
                msg += "a";
            }
            toBytes = Encoding.ASCII.GetBytes(msg);
            try
            {
                cs1.Write(toBytes, 0, toBytes.Length);
                Console.WriteLine("FAILED");
            }
            catch
            {
                Console.WriteLine("SUCCESS");
            }

            cs1.Close();         
            return true;
        }

        static bool PositionTests()
        {
            Console.WriteLine("\nTesting Concat Stream Position Property: ");
           

            Console.Write("\tTesting Position after Read (FileStream, FileStream): ");
            FileStream fs1 = new FileStream("testFile1.txt", FileMode.Open, FileAccess.Read);
            FileStream fs2 = new FileStream("testFile1.txt", FileMode.Open, FileAccess.Read);
            ConcatStream cs1 = new ConcatStream(fs1, fs2);

            long initialPosition = cs1.Position;
            byte[] buffer1 = new byte[10];
            int bytesRead = cs1.Read(buffer1, 0, buffer1.Length);
            long currentPosition = cs1.Position;

            if (currentPosition - bytesRead != initialPosition || currentPosition == 0) Console.WriteLine("FAILED");
            else Console.WriteLine("SUCCESS");

            fs1.Close();
            fs2.Close();
            cs1.Close();

            /******************************************/

            Console.Write("\tTesting Position after Write (FileStream, FileStream): ");
            FileStream fs3 = new FileStream("testFile3.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            string testString = "Checking";
            byte[] toBytes = Encoding.ASCII.GetBytes(testString);
            fs3.Write(toBytes, 0, toBytes.Length);
            FileStream fs4 = new FileStream("testFile4.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            ConcatStream cs2 = new ConcatStream(fs3, fs4);

            initialPosition = cs2.Position;
            testString = "Testing, 1 2 3, Testing...";
            toBytes = Encoding.ASCII.GetBytes(testString);
            cs2.Write(toBytes, 0, toBytes.Length);
            currentPosition = cs2.Position;

            if (currentPosition - toBytes.Length != initialPosition || currentPosition == 0) Console.WriteLine("FAILED");
            else Console.WriteLine("SUCCESS");

            fs3.Close();
            fs4.Close();
            cs2.Close();

            /*********************************************/

            Console.Write("\tTesting Position after Seek (FileStream, FileStream): ");
            fs1 = new FileStream("testFile1.txt", FileMode.Open, FileAccess.Read);
            fs2 = new FileStream("testFile1.txt", FileMode.Open, FileAccess.Read);
            cs1 = new ConcatStream(fs1, fs2);
                        
            int offset = 20;
            cs1.Seek(offset, SeekOrigin.Begin);
            if (cs1.Position == offset) Console.WriteLine("SUCCESS");
            else Console.WriteLine("FAILED");            

            fs1.Close();
            fs2.Close();
            cs1.Close();

            /******************************************/

            Console.Write("\tTesting set Position after length max (FileStream, FileStream, Length): ");            
            fs1 = new FileStream("testFile1.txt", FileMode.Open, FileAccess.Read);
            fs2 = new FileStream("testFile1.txt", FileMode.Open, FileAccess.Read);
            cs1 = new ConcatStream(fs1, fs2, 10);
            try
            {
                cs1.Position = 12;
                Console.WriteLine("FAILED");
            }
            catch
            {                
                Console.WriteLine("SUCCESS");
            }

            fs1.Close();
            fs2.Close();
            cs1.Close();

            /******************************************/

            Console.Write("\tTesting exception for passing the same instance for both streams (FileStream, FileStream): ");
            fs3 = new FileStream("testFile10.txt", FileMode.Create, FileAccess.ReadWrite);
            try
            {
                cs2 = new ConcatStream(fs3, fs3);
                Console.WriteLine("FAILED");
            }
            catch
            {
                Console.WriteLine("SUCCESS");
            }

            cs1.Close();            
            return true;
        }
        
        static bool ReadStreamTests()
        {
            Console.WriteLine("\nTesting Read Functionality:");                  

            Console.Write("\tTesting reading all content from both streams (FileStream, FileStream): ");
            FileStream fs1 = new FileStream("testFile1.txt", FileMode.Open, FileAccess.Read);
            byte[] buffer1 = new byte[fs1.Length];
            int bytesRead = fs1.Read(buffer1, 0, buffer1.Length);
            string msg1 = System.Text.Encoding.ASCII.GetString(buffer1, 0, bytesRead);            

            FileStream fs2 = new FileStream("testFile1.txt", FileMode.Open, FileAccess.Read);
            byte[] buffer2 = new byte[fs2.Length];
            bytesRead = fs2.Read(buffer2, 0, buffer2.Length);
            string msg2 = System.Text.Encoding.ASCII.GetString(buffer2, 0, bytesRead);

            ConcatStream cs1 = new ConcatStream(fs1, fs2);
            byte[] buffer3 = new byte[cs1.Length];
            bytesRead = cs1.Read(buffer3, 0, buffer3.Length);
            string msg3 = System.Text.Encoding.ASCII.GetString(buffer3, 0, bytesRead);
            if (msg3 != msg1 + msg2 || msg3.Length <= 0 || msg2.Length <= 0 || msg1.Length <= 0) Console.WriteLine("FAILED");
            else Console.WriteLine("SUCCESS");            

            cs1.Close();

            /***************************************/

            Console.Write("\tTesting reading all content from both streams, random amounts (MemoryStream, MemoryStream): ");
            MemoryStream ms1 = new MemoryStream(buffer1);
            MemoryStream ms2 = new MemoryStream(buffer2);
            cs1 = new ConcatStream(ms1, ms2);
            byte[] buffer4 = new byte[cs1.Length];
            Random rnd = new Random();
            msg3 = "";
            bytesRead = 0;
            int totalRead = 0;
            bytesRead = cs1.Read(buffer4, 0, rnd.Next(1, buffer4.Length - totalRead));
            msg3 += System.Text.Encoding.ASCII.GetString(buffer4, 0, bytesRead);
            totalRead += bytesRead;

            while (bytesRead != 0)
            {
                int max = Math.Max(buffer4.Length - totalRead, 1);
                bytesRead = cs1.Read(buffer4, 0, rnd.Next(1, max));
                msg3 += System.Text.Encoding.ASCII.GetString(buffer4, 0, bytesRead);
                buffer4 = new byte[cs1.Length];
                totalRead += bytesRead;
            }
            
            if (msg3 == msg1 + msg2 && msg3.Length > 0 && msg2.Length > 0 && msg1.Length > 0) Console.WriteLine("SUCCESS");
            else Console.WriteLine("FAILED");

            ms1.Close();
            ms2.Close();
            cs1.Close();

            /*****************************/

            Console.Write("\tTesting read all from (memoryStream,NoSeekMemoryStream): ");
            ms1 = new MemoryStream(buffer1);
            ms2 = new NoSeekMemoryStream(buffer2);
            cs1 = new ConcatStream(ms1, ms2);

            msg1 = System.Text.Encoding.ASCII.GetString(buffer1, 0, buffer1.Length);
            msg2 = System.Text.Encoding.ASCII.GetString(buffer2, 0, buffer2.Length);

            buffer3 = new byte[cs1.Length];
            bytesRead = cs1.Read(buffer3, 0, buffer3.Length);
            msg3 = System.Text.Encoding.ASCII.GetString(buffer3, 0, buffer3.Length);

            if (msg3 == msg1 + msg2 && msg3.Length > 0 && msg2.Length > 0 && msg1.Length > 0) Console.WriteLine("SUCCESS");
            else Console.WriteLine("FAILED");

            /*********************************/

            Console.Write("\tTesting if position resets when stream is passed to CS: ");
            fs1 = new FileStream("testFile1.txt", FileMode.Open, FileAccess.Read);
            int amountToRead = 10;
            buffer1 = new byte[amountToRead];
            bytesRead = fs1.Read(buffer1, 0, buffer1.Length);
            msg1 = System.Text.Encoding.ASCII.GetString(buffer1, 0, bytesRead);

            fs2 = new FileStream("testFile1.txt", FileMode.Open, FileAccess.Read);
            buffer2 = new byte[amountToRead];
            bytesRead = fs2.Read(buffer2, 0, buffer2.Length);
            msg2 = System.Text.Encoding.ASCII.GetString(buffer2, 0, bytesRead);

            cs1 = new ConcatStream(fs1, fs2);
            buffer3 = new byte[amountToRead];
            bytesRead = cs1.Read(buffer3, 0, buffer3.Length);
            msg3 = System.Text.Encoding.ASCII.GetString(buffer3, 0, bytesRead);            
            if (msg3 != msg1 || msg3 != msg2 || msg3.Length <= 0) Console.WriteLine("FAILED");
            else Console.WriteLine("SUCCESS");

            fs1.Close();
            fs2.Close();
            cs1.Close();

            /*********************************************************************/

            Console.Write("\tTesting reading from stream no length property (FileStream, NetworkStream): ");
            string urlAddress = "https://www.facebook.com";
            HttpWebRequest request = (HttpWebRequest)System.Net.WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();            
            fs2 = new FileStream("testFile2.txt", FileMode.Open, FileAccess.Read);            
            byte[] buffer = new byte[fs2.Length + 2];
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                cs1 = new ConcatStream(fs2, receiveStream);
                bytesRead = cs1.Read(buffer, 0, buffer.Length);
                msg1 = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);
                if (msg1.Length > fs2.Length + 1) Console.WriteLine("SUCCESS");
                else { Console.WriteLine("FAILED"); }                
            }
            else
            {
                Console.Write("!!!!!!!!!!!!!!!!!!!SOCKET CONNNECTION FAILED, cannot run test.");
                throw new ArgumentException();
            }

            fs2.Close();
            cs1.Close();            
            return true;
            
        }

        static public bool canWriteTest()
        {
            bool val = true;

            FileStream fs1 = new FileStream("testFile1.txt", FileMode.Open, FileAccess.Read);
            FileStream fs2 = new FileStream("testFile1.txt", FileMode.Open, FileAccess.Read);
            FileStream fs3 = new FileStream("testFile2.txt", FileMode.Create, FileAccess.ReadWrite);
            FileStream fs4 = new FileStream("testFile3.txt", FileMode.Create, FileAccess.ReadWrite);

            ConcatStream cs1 = new ConcatStream(fs1, fs2);
            ConcatStream cs2 = new ConcatStream(fs1, fs3);
            ConcatStream cs3 = new ConcatStream(fs3, fs4);            
                        
            if (cs1.CanWrite) { Console.Out.WriteLine("CanWrite failed - 1 readonly, 1 read/write. Expected Output: False | Output: True "); val = false; }
            if (cs2.CanWrite) { Console.Out.WriteLine("CanWrite failed - 1 readonly, 1 read/write. Expected Output: False | Output: True "); val = false; }
            if (!cs3.CanWrite) { Console.Out.WriteLine("CanWrite failed - 2 read/write. Expected Output: True | Output: False "); val = false; }

            try
            {
                string testString = "New String";
                byte[] toBytes = Encoding.ASCII.GetBytes(testString);
                cs1.Write(toBytes, 0, toBytes.Length);
                Console.WriteLine("2 readonly streams : Write to a readonly Concatstream succeeded | Expected Output: Fail");
                val = false;
            }
            catch { };
            try
            {
                string testString = "New String";
                byte[] toBytes = Encoding.ASCII.GetBytes(testString);
                cs2.Write(toBytes, 0, toBytes.Length);                
                Console.WriteLine("1 readonly, 1 readWrite stream : Write to Concatstream succeeded | Expected Output: Fail");
                val = false;
            }
            catch { };
            try
            {
                string testString = "New String";
                byte[] toBytes = Encoding.ASCII.GetBytes(testString);
                cs3.Write(toBytes, 0, toBytes.Length);                
            }
            catch
            {
                Console.WriteLine("2 readWrite stream : Write to Concatstream failed | Expected Output: Success");
                val = false;
            };

            fs1.Close();
            fs2.Close();
            fs3.Close();
            fs4.Close();
            cs1.Close();
            cs2.Close();

            return val;
        }

        static public bool canReadTest()
        {
            bool val = true;
            FileStream fs1 = new FileStream("testFile1.txt", FileMode.Open, FileAccess.Read);
            FileStream fs2 = new FileStream("testFile1.txt", FileMode.Open, FileAccess.Read);
            FileStream fs3 = new FileStream("testFile2.txt", FileMode.Open, FileAccess.ReadWrite);
            ConcatStream cs1 = new ConcatStream(fs1, fs2);
            ConcatStream cs2 = new ConcatStream(fs2, fs3);
            if (!cs1.CanRead) { Console.Out.WriteLine("CanRead failed - 2 readonly. Expected Output: True | Output: False "); val = false; }
            if (!cs2.CanRead) { Console.Out.WriteLine("CanRead failed - 1 readonly, 1 write only. Expected Output: true | Output: false "); val = false; }
            
            try
            {
                byte[] toBytes = new byte[8000];
                cs1.Read(toBytes, 0, toBytes.Length);
            }
            catch
            {
                Console.WriteLine("2 readonly stream : Read from Concatstream failed | Expected Output: Success");
                val = false;
            };
            try
            {
                byte[] toBytes = new byte[8000];
                cs2.Read(toBytes, 0, toBytes.Length);
                Console.WriteLine("1 readonly, 1 Write stream : Read from Concatstream succeeded | Expected Output: Fail");
                val = false;
            }
            catch { };

            fs1.Close();
            fs2.Close();
            fs3.Close();
            cs1.Close();
            cs2.Close();            

            return val;
        }
        
        static public bool LengthTest()
        {
            Console.WriteLine("\nChecking Length Property: ");
            string urlAddress = "https://www.facebook.com";
            HttpWebRequest request = (HttpWebRequest)System.Net.WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            byte[] buffer = new byte[8000];
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                Stream receiveStream2 = response.GetResponseStream();
                FileStream fs3 = new FileStream("testFile3.txt", FileMode.Open, FileAccess.ReadWrite);
                try
                {
                    ConcatStream cs = new ConcatStream(receiveStream, receiveStream2);
                    Console.WriteLine("Stream1 with no Length instantiated. Argument Expection Expected");
                    return false;
                }
                catch { }                
                try
                {
                    Console.Write("\tValid Instantiation of ConcatStream: (filestream, networkStream): ");                    
                    ConcatStream cs = new ConcatStream(fs3, receiveStream2);
                    Console.WriteLine("SUCCESS");
                }
                catch
                {
                    Console.WriteLine("FAILED");
                }
                try
                {
                    Console.Write("\tAccessing the Length property when not available. (filestream, networkStream): ");
                    ConcatStream cs = new ConcatStream(fs3, receiveStream2);
                    long length = cs.Length;
                    Console.WriteLine("FAILED");
                    return false;
                }
                catch { Console.WriteLine("SUCCESS"); }
                try
                {
                    Console.Write("\tAccessing and checking length of valid fixed length constructor: ");
                    ConcatStream cs = new ConcatStream(fs3, receiveStream2, fs3.Length + 20);
                    long length = cs.Length;
                    if (length != fs3.Length + 20)
                    {
                        Console.WriteLine("FAILED");
                        return false;
                    }
                    Console.WriteLine("SUCCESS");
                }
                catch
                {
                    Console.WriteLine("FAILED");
                    return false;
                }

                try
                {
                    Console.Write("\tFixed Length Constructor, stream1 with no length: ");
                    ConcatStream cs = new ConcatStream(receiveStream, receiveStream2, fs3.Length + 20);
                    Console.Write("FAILED");                    
                    return false;
                }
                catch
                {
                    Console.WriteLine("SUCCESS");
                }
                try
                {
                    Console.Write("\tTesting Negative Length Exception for Fixed Length Construct (FileStream, NetworkStream): ");
                    ConcatStream cs = new ConcatStream(fs3, receiveStream2, -10);
                    Console.WriteLine("FAILED");
                }
                catch
                {
                    Console.WriteLine("SUCCESS");
                }
                try
                {
                    Console.Write("\tTesting Exception for null second stream: ");
                    ConcatStream cs = new ConcatStream(fs3, null);
                    Console.WriteLine("FAILED");
                }
                catch
                {
                    Console.WriteLine("SUCCESS");
                }
                try
                {
                    Console.Write("\tTesting Exception for null first stream: ");
                    ConcatStream cs = new ConcatStream(fs3, null);
                    Console.WriteLine("FAILED");
                }
                catch
                {
                    Console.WriteLine("SUCCESS");
                }

                fs3.Close();                                
                // int bytesRead = receiveStream.Read(buffer, 0, buffer.Length);
                // string msg = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);                
            }            
            return true;
        }



        /*Where I LEFT OFF:
         * Change some source code to make sure tests are working
         * Finish changing all the tests into the new FAILED/SUCCESS FORMAT
         * CONTINUE WRITING LENGTH TESTS
         * Write other tests
        */


    }
}
