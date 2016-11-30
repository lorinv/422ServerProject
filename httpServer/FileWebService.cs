using System;
using System.IO;
using System.Linq;

namespace CS422
{
    internal class FilesWebService : WebService
    {
        private readonly FileSys422 _fs;
        private bool m_allowUploads;

        public FilesWebService(FileSys422 fs)
        {
            _fs = fs;
            m_allowUploads = true;
        }

        public void HandlePut(WebRequest req, Dir422 dir)
        {
            File422 file = dir.CreateFile(req.URI.Split('/', '\\').Last());
            Stream outputStream = file.OpenReadWrite();
            Stream inputStream = req._networkStream;
            
            int BUFFER_SIZE = 8000;
            int bytesRead = 0;
            byte[] bytes = new byte[BUFFER_SIZE];
            bytesRead = inputStream.Read(bytes, 0, bytes.Length);            

            while (bytesRead != 0)
            {
                // Translate data bytes to a ASCII string.
                outputStream.Write(bytes, 0, bytesRead);
                bytes = new byte[BUFFER_SIZE];
                bytesRead = inputStream.Read(bytes, 0, bytes.Length);                                
            }

            outputStream.Close();
            inputStream.Close();
            req.WriteHTMLResponse("");
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
            for (int i = 1; i < pieces.Length - 1; i++)
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
            if (file == null && req.Method == "PUT")
            {
                HandlePut(req, dir);
                return;
            }
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
                if (piece != "")
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

            // Build an HTML file listing            
            // We'll need a bit of script if uploading is allowed
            if (m_allowUploads)
            {
                html.AppendLine(
               @"<script>
function selectedFileChanged(fileInput, urlPrefix)
{
 document.getElementById('uploadHdr').innerText = 'Uploading ' + fileInput.files[0].name + '...';
 // Need XMLHttpRequest to do the upload
 if (!window.XMLHttpRequest)
 {
 alert('Your browser does not support XMLHttpRequest. Please update your browser.');
 return;
 }
 // Hide the file selection controls while we upload
 var uploadControl = document.getElementById('uploader');
 if (uploadControl)
 {
 uploadControl.style.visibility = 'hidden';
 }
 // Build a URL for the request
 if (urlPrefix.lastIndexOf('/') != urlPrefix.length - 1)
 {
 urlPrefix += '/';
 }

 var uploadURL = urlPrefix + fileInput.files[0].name;
 // Create the service request object
 var req = new XMLHttpRequest();
 req.open('PUT', uploadURL);
 req.onreadystatechange = function()
 {
 document.getElementById('uploadHdr').innerText = 'Upload (request status == ' + req.status + ')';
 };
 req.send(fileInput.files[0]);
}
</script>
");
            }

            foreach (Dir422 directory in dir.GetDirs())
            {       
                html.AppendFormat("<a href=\'{0}\'>{1}</a><br>", directory.Name, directory.Name);                
            }
            html.Append("<br><br><h1>Files</h1>");
            foreach (File422 file in dir.GetFiles())
            {
                html.AppendFormat("<a href=\"{0}\">{1}</a><br>", file.Name, file.Name);                
            }
            // If uploading is allowed, put the uploader at the bottom
            if (m_allowUploads)
            {
                html.Append(GetHREF(dir, true)); 
                html.AppendFormat(
                "<hr><h3 id='uploadHdr'>Upload</h3><br>" +
                "<input id=\"uploader\" type='file' " +
                "onchange='selectedFileChanged(this,\"{0}\")' /><hr>",
                GetHREF(dir, true));
            }

            return html.ToString();
        }



        private string GetHREF(Dir422 dir, bool something)
        {
            return "/files" + GetHREFHelper(dir);
        }


        private string GetHREFHelper(Dir422 dir)
        {
            if (dir.Parent != null)
                return GetHREFHelper(dir.Parent) + "/" + dir.Name;
            return "/" + dir.Name;
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