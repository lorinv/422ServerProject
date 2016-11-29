using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS422
{
    // Class to represent a web request
    // Must have the ability to respond to request
    // body will -often- be a ConcatStream
    // Request object is created after double line berak

    class WebRequest
    {
        private string _method;
        private string _uri;
        private string _version;
        private Dictionary<String, String> _headers;
        private Stream _bodyStream;
        private Stream _networkStream;
        private bool _queryLength;
        public bool isValid = true;

        Stream bodyStream;

        public string Method
        {
            get { return _method; }
            set { _method = value; }
        }

        public string URI
        {
            get { return _uri; }
            set { _uri = value; }        
        }

        public string Version
        {
            get { return _version; }
            set { _version = value; }
        }        

        public Dictionary<string,string> Headers
        {
            get { return _headers; }
        }

        public WebRequest(string request, Stream stream)
        {
            _networkStream = stream;
            _headers = new Dictionary<string, string>();
            parseRequest(request);
        }

        private void parseRequest(string request)
        {
            string[] requestSplit = request.Split('\n', '\r');
            string[] split = requestSplit[0].Split(' ');
            if (split.Length >= 3)
            {
                Method = split[0];
                URI = split[1];
                Version = split[2];
            }
            else
            {
                isValid = false;
                return;
            }

            for (int i = 1; i < requestSplit.Length - 1; i += 1)
                _headers[requestSplit[i].Split(' ')[0]] = String.Join(" ", requestSplit[i].Split(' ').Skip(1));
        }

        public WebRequest(string method, string uri, string version, Dictionary<String, String> headers, Stream bodyStream)
        {
            _headers = new Dictionary<string, string>();
            //TODO: set all the variables and add the public get, set   
        }        

        public void WriteNotFoundResponse(string pageHTML)
        {            
            string response = 
                "HTTP/1.1 404 Not Found\r\n" +
                "Content-Type: text/html\r\n" +
                "Content-Length: " + System.Text.Encoding.ASCII.GetBytes(pageHTML) + 
                "\r\n\r\n" + pageHTML;
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(response);

            _networkStream.Write(msg, 0, msg.Length);
        }

        public void WriteNotFoundResponse()
        {
            string response =
                "HTTP/1.1 404 Not Found\r\n" +
                "Content-Type: text/html\r\n" +
                "Content-Length: " + "0" +
                "\r\n\r\n" + "";
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(response);

            _networkStream.Write(msg, 0, msg.Length);
        }

        public bool WriteHTMLResponse(string htmlString)
        {            
            string response =
                "HTTP/1.1 200 OK\r\n" +
                "Content-Type: text/html\r\n" +
                "Content-Length: " + htmlString.Length +
                "\r\n\r\n" + htmlString;
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(response);

            _networkStream.Write(msg, 0, msg.Length);

            return true;
        }

        public bool WriteHTMLResponse(string htmlString, string content_type)
        {
            string response =
                "HTTP/1.1 200 OK\r\n" +
                "Content-Type: + " + content_type + "\r\n" +
                "Content-Length: " + htmlString.Length +
                "\r\n\r\n" + htmlString;
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(response);

            _networkStream.Write(msg, 0, msg.Length);

            return true;
        }

        public Stream Response
        {
            get { return _networkStream; }
        }

        public long Length
        {
            get
            {
                if (_headers.Keys.Contains("Content-Length"))
                    //TODO: Double check this please
                    return (long)Convert.ToDouble(_headers["Content-Length"]);
                else
                    throw new NotSupportedException();
            }
        }
    }
}
