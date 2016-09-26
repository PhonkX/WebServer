using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace webserver
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var server = Server.StartNew(IPAddress.Any, 48954);
            while (true)
            {
                server.HandleRequest();
            }
            server.Stop();
        }
    }
}
