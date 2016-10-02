using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.DataModel
{
    public class Request
    {
        public string Query; // TODO: разобраться, что лучше сюда сделать
        public HttpMethod Method;
        public string RequestedResource;
        public string Version; // TODO: разобраться, что лучше сюда сделать
        public NameValueCollection Headers;
        public string Body;

        public Request()
        {
            Headers = new NameValueCollection();
            Method = HttpMethod.GET;
            RequestedResource = "";
            Query = "";
            Body = "";
        }
    }
}
