using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS422
{
    class DemoService : WebService
    {
        public override string ServiceURI
        {
            get
            {
                return "/";
            }
        }

        public override void Handler(WebRequest req)
        {
            string c_template =
                "<html>This is the response to the request:<br>" +
                "Method: {0}<br>Request-Target/URI: {1}<br>" +
                "Request body size, in bytes: {2}<br><br>" +
                "Student ID: {3}</html>";

            try
            {
                req.WriteHTMLResponse(String.Format(c_template, req.Method, req.URI, req.Length, 11354621));
            }
            catch
            {
                req.WriteHTMLResponse(String.Format(c_template, req.Method, req.URI, -1, 11354621));
            }
        }
    }
}
