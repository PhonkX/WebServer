using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.DataModel
{
    public class RequestParser
    {
        public Request ParseRequest(byte[] request)
        {
            return ParseRequest(Encoding.UTF8.GetString(request));
        }

        public Request ParseRequest(string request)
        {
            var parsedRequest = new Request();
            var tokens = request.Split('\n'); // TODO: подумать над проверками и как можно разбить по пустой строке
            int emptyStringCount = 0;
            parsedRequest.Query = tokens[0].Trim();
            for (int i = 1; i < tokens.Length; ++i)
            {
                switch (emptyStringCount)
                {
                    case 0:
                        if (!String.IsNullOrWhiteSpace(tokens[i]))
                        {
                            var headerElements = tokens[i].Split(':');
                            if (headerElements.Length >= 2)
                            {
                                var sBuilder = new StringBuilder();
                                for (int j = 1; j < headerElements.Length - 1; j++)
                                {
                                    sBuilder.Append(headerElements[j]);
                                    sBuilder.Append(":");
                                }
                                sBuilder.Append(headerElements[headerElements.Length - 1]); // TODO: подумать, как сделать аккуратнее
                                parsedRequest.Headers.Add(headerElements[0].Trim(), sBuilder.ToString().Trim());
                            }
                            else
                            {
                                parsedRequest.Body += tokens[i];
                            }
                        }
                        else
                        {
                            emptyStringCount++;
                        }
                        break;
                    case 1:
                        if (!String.IsNullOrWhiteSpace(tokens[i]))
                        {
                            parsedRequest.Body += tokens[i];
                        }
                        break;
                }
            }

            return parsedRequest;
        }
    }
}
