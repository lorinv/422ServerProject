using System;
using System.IO;

namespace CS422
{
    internal class FilesWebService : WebService
    {
        private readonly FileSys422 _fs;

        public FilesWebService(FileSys422 fs)
        {
            _fs = fs;
        }
    
        public override void Handler(WebRequest req)
        {
            //Every request should start with /files/
            if (!req.URI.StartsWith(this.ServiceURI))
            {
                throw new InvalidOperationException();
            }

            //percent-decode URI
            string uri = req.URI;                  

            string[] pieces = req.URI.Substring(ServiceURI.Length).Split('/','\\');
            Dir422 dir = _fs.GetRoot();        
            //Grabs all the parts but the last part, which could be a file or a dir
            string piece = "";
            for (int i = 0; i < pieces.Length - 1; i++)
            {
                piece = pieces[i];
                if (piece.Contains("%20")) piece = piece.Replace("%20", " ");
                dir = dir.GetDir(piece);

                //Check if directory exists
                if (dir == null)
                {
                    req.WriteNotFoundResponse();
                    return;
                }
            }
                        

            //Check if the last part is a file or a directory
            piece = pieces[pieces.Length - 1];
            if (piece.Contains("%20")) piece = piece.Replace("%20", " ");
            File422 file = dir.GetFile(piece); //TODO: This is returning Null and is not supposed to.
            if (file != null)
            {
                //If it's a file, then return the file.
                Console.WriteLine("It's a file!");
                RespondWithFile(file, req);
            }
            else
            {
                piece = pieces[pieces.Length - 1];
                if (piece.Contains("%20")) piece = piece.Replace("%20", " ");
                dir = dir.GetDir(piece);

                if (dir != null)
                {
                    //If it's a dir, return the dir                    
                    RespondWithList(dir, req);
                }
                else
                {
                    //Else return that nothing was found.                    
                    req.WriteNotFoundResponse();
                }
            }
        }

        string BuildDirHTML(Dir422 dir)
        {
            var html = new System.Text.StringBuilder("<html><h1>Folders</h1>");

            foreach (Dir422 directory in dir.GetDirs())
            {       
                html.AppendFormat("<a href=\"{0}\">{1}</a><br>", directory.Name, directory.Name);                
            }
            html.Append("<br><br><h1>Files</h1>");
            foreach (File422 file in dir.GetFiles())
            {
                html.AppendFormat("<a href=\"{0}\">{1}</a><br>", file.Name, file.Name);                
            }

            return html.ToString();
        }

        private void RespondWithFile(File422 file,WebRequest req)
        {
            int range_start = -1;
            int range_stop = -1;
            int total_read = 0;
            if (req.Headers.ContainsKey("Content-Range") || req.Headers.ContainsKey("content-range"))
            {
                //Get content Range.
                try
                {
                    range_start = Convert.ToInt32(req.Headers["Content-Range"].Split('/', '-')[0]);
                    range_stop = Convert.ToInt32(req.Headers["Content-Range"].Split('/', '-')[1]);
                }
                catch { };
            }           

            string content_type = "";
            if (file.Name.ToLower().EndsWith(".jpg") || file.Name.EndsWith(".jpeg")) content_type = "image/jpeg";
            if (file.Name.ToLower().EndsWith(".png")) content_type = "image/png";
            if (file.Name.ToLower().EndsWith(".pdf")) content_type = "application/pdf";
            if (file.Name.ToLower().EndsWith(".mp4")) content_type = "video/mp4";
            if (file.Name.ToLower().EndsWith(".txt")) content_type = "text/html";
            if (file.Name.ToLower().EndsWith(".html")) content_type = "text/html";
            if (file.Name.ToLower().EndsWith(".xml")) content_type = "text/xml";

            Stream output = file.OpenReadOnly();
            string res = "HTTP/1.1 200 OK\r\n" +
                "Content-Length: " + output.Length + "\r\n" +
                "Content-Type: " + content_type +
                "\r\n\r\n";

            byte[] msg = System.Text.Encoding.ASCII.GetBytes(res);
            req.Response.Write(msg, 0, msg.Length);

            int bytes_read = 0;
            byte[] input = new byte[8000];

            while (total_read < range_start)
            {                
                if (range_start - total_read > input.Length)
                    bytes_read = output.Read(input, 0, input.Length);
                else
                    bytes_read = output.Read(input, 0, range_start - total_read);
                total_read += bytes_read;
            }

            string html = "";

            if (range_stop != -1)
            {
                while (total_read < range_stop)
                {
                    if (range_stop - total_read > input.Length)
                        bytes_read = output.Read(input, 0, input.Length);
                    else
                        bytes_read = output.Read(input, 0, range_stop - total_read);
                    total_read += bytes_read;
                    req.Response.Write(input, 0, input.Length);
                }
            }
            else
            {
                bytes_read = output.Read(input, 0, input.Length);
                total_read += bytes_read;                
                req.Response.Write(input, 0, input.Length);

                while (bytes_read != 0)
                {
                    bytes_read = output.Read(input, 0, input.Length);
                    total_read += bytes_read;                    
                    req.Response.Write(input, 0, input.Length);
                }
            }                                  
                       
            output.Close();
        }

        private void RespondWithList(Dir422 dir, WebRequest req)
        {
            string html = BuildDirHTML(dir);
            html += "</html>";            
            req.WriteHTMLResponse(html);
        }

        public override string ServiceURI
        {
            get
            {
                return "/files/";
            }
        }
    }
}