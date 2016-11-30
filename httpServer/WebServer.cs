using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CS422
{       
    /*
     * Manages the server functionality like adding services,
     * listenning and handling connection, etc. 
    */
    static class WebServer
    {                
        public static BlockingCollection<TcpClient> _clientQueue;
        public static Thread[] pool;
        public static bool stopWork = false;
        public static List<WebService> serviceList;
        public static int _port;
        private static System.Timers.Timer _timer;

        public static bool Start(int port, int threadCount)
        {
            if (serviceList == null)
            {                
                Console.WriteLine("Must add service(s) before running the server.");
                throw new NotImplementedException();
            }

            _port = port;

            if (threadCount <= 0)
                threadCount = 64;

            _clientQueue = new BlockingCollection<TcpClient>();
            pool = new Thread[threadCount];
           
            for (int i = 0; i < threadCount - 1; i++)
            {
                pool[i] = new Thread(() => ThreadWork());
                pool[i].Start();
            }

            pool[threadCount - 1] = new Thread(() => ListenForClients());
            pool[threadCount - 1].Start();

            return true;            
        }


        public static void ThreadWork()
        {
            while (!stopWork)
            {
                //**** Wait for new client to be added to the queue ****////
                TcpClient client = _clientQueue.Take();
                WebRequest request = BuildRequest(client);
                if (request != null)
                {
                    //Call corresponding service from list
                    foreach (var item in serviceList)
                    {
                        if (request.URI.StartsWith(item.ServiceURI))
                        {
                            item.Handler(request);
                            client.Close();
                            break;
                        }
                    }
                }
            }
        }
        
        // A thread to listen and delagte clients         
        private static void ListenForClients()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, _port);
            listener.Start();
            while (!stopWork) // TODO: Change this to actual
            {
                var client = listener.AcceptTcpClient();
                if (!client.Connected)
                    return;
                _clientQueue.Add(client);
            }
        }

        //Build the web request using all the incoming info
        private static WebRequest BuildRequest(TcpClient client)
        {
            // Constansts for building a request
            const int REQUEST_TIMEOUT = 1000000;
            const int INITIAL_LINE_BREAK_THRESHOLD = 2048;
            const int PRE_BODY_LENGTH_THESHOLD = 100 * 1024;
            const int BUFFER_SIZE = 2048;

            // Setup the stream and set individual read timeouts
            Stream stream = client.GetStream();            
            stream.ReadTimeout = 15000000;
            
            // Setup a request timeout and start that timer
            _timer = new System.Timers.Timer(REQUEST_TIMEOUT);
            _timer.AutoReset = false;
            _timer.Elapsed += async (sender, e) => await RequestTimeoutResponse(client);
            _timer.Start();
            
            byte[] bytes = new byte[BUFFER_SIZE];
            int bytesRead;
            try { bytesRead = stream.Read(bytes, 0, bytes.Length); }
            catch { client.Close(); return null; }
            string data = "";
            int total_read = 0;

            // If no bytes are read, return a null request
            if (bytesRead == 0)
            {
                client.Close();
                return null;
            }

            // Receive all the data sent by the client. 
            while (bytesRead != 0)
            {
                // Translate data bytes to a ASCII string.
                data += System.Text.Encoding.ASCII.GetString(bytes, 0, bytesRead);

                // Return False if you have read in more than three bytes but not seen "GET"
                if (total_read >= 3 && !(data.Contains("GET") || data.Contains("PUT")))
                {
                    Console.WriteLine("Invalid Request.");
                    client.Close();
                    return null;
                }
                else
                {

                }

                // Return False if you have not seen "HTTP/1.1"
                string[] parts = data.Split(' ');
                if (parts.Length >= 3 && parts[2].Length >= 8 && !parts[2].Contains("HTTP/1.1"))
                {
                    Console.WriteLine("Invalid Request.");
                    client.Close();
                    return null;
                }

                // Test if the initial line break was within the size threshold
                if (total_read >= INITIAL_LINE_BREAK_THRESHOLD)
                {
                    if (!data.Contains("\r\n"))
                    {
                        Console.WriteLine("Initial line of request is too large...");
                        client.Close();
                        return null;
                    }
                }

                // Test is the double line break was within the size threshold
                if (total_read >= PRE_BODY_LENGTH_THESHOLD)
                {
                    if (!data.Contains("\r\n\r\n"))
                    {
                        Console.WriteLine("Pre-body request is too large...");
                        client.Close();
                        return null;
                    }
                }

                // If you see the double line break, stop reading
                // You have hit the body of the request.
                if (data.Contains("\r\n\r\n"))
                {
                    _timer.Stop();
                    break;
                }

                try { bytesRead = stream.Read(bytes, 0, bytes.Length); }
                catch { client.Close(); return null; }
                total_read += bytesRead;
            }

            Console.WriteLine("Final: " + data + "\n");
            WebRequest request = new WebRequest(data, stream);
            if (request.URI == null || !request.isValid)
                return null;
            else        
                return request;                        
        }

        private static Task RequestTimeoutResponse(TcpClient client)
        {
            if (client.Connected)
            {
                client.Close();
                Console.WriteLine("Request Timeout Expired...");
            }
            return new Task(ThreadWork);            
        }        

        public static void AddService(WebService service)
        {
            
            // Adds the given service to a list of services that the web server holds.                                       
            if (serviceList == null)
                serviceList = new List<WebService>();
            serviceList.Add(service);
        }

        public static void Stop()
        {
            stopWork = true;           
            int i = pool.Count<Thread>();
            while (i > 1)
                i = pool.Count<Thread>();            
        }
    }    
}
