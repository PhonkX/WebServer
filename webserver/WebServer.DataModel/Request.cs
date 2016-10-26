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
        public string RequestedUri;
        public string Version; // TODO: разобраться, что лучше сюда сделать
        public NameValueCollection QueryParameters; 
        public NameValueCollection Headers;
        public string Body;
        // TODO: добавить параметры и сделать их парсинг с помощью HttpUtility
        public Request()
        {
            Headers = new NameValueCollection();
            Method = HttpMethod.GET;
            RequestedUri = null;
            Query = "";
            Body = "";
            QueryParameters	= new NameValueCollection();
        }
    }
}
