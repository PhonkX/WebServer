using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.DataModel
{
    public class ResponseBuilder
    {
        private Response response;
        private ResponseBuilder(HttpStatusCode code)
        {
            response = new Response();
            response.ResponseCode = code;
        }

        public static ResponseBuilder StartNew(HttpStatusCode code)
        {
            return new ResponseBuilder(code);
        }

        public void AddHeader(string header, string value) // TODO: подумать про объект Header
        {
            response.Headers.Add(header, value);
        }

        public void SetBody(byte[] body)
        {
            response.Body = body;
        }

        public Response GetResponse()
        {
            return response;
        }
    }
}
