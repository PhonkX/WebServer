using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.DataModel
{
    public class ResponseSerializer
    {
        public byte[] Serialize(Response response)
        {
            var answer = Encoding.UTF8.GetBytes(
                String.Format("HTTP/1.1 {0} {1}", (int) response.ResponseCode, response.ResponseCode.ToString()));
            var headers = Encoding.UTF8.GetBytes(String.Format("{0} \r\n\r\n",
                String.Join("\r\n",response.Headers.AllKeys.Select(header =>
                            String.Format("{0} {1}", header, response.Headers[header]))
            )));
            var buffer = new MemoryStream();
            buffer.Write(answer, 0, answer.Length);
            buffer.Write(headers, answer.Length, headers.Length);
            buffer.Write(response.Body, headers.Length, response.Body.Length);

            return buffer.ToArray();
        }
    }
}
