using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.DataModel
{
    public class Response // TODO: подумать об использовании стандартных классов
    {
        public HttpStatusCode ResponseCode;
        public NameValueCollection Headers;
        public byte[] Body; // TODO: Подумать, как объединять
    }
}
