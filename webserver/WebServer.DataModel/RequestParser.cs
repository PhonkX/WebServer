using System;
using System.Collections.Generic;
using System.Globalization;
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
            var query = tokens[0].Trim();
            parsedRequest.Query = query;
            var queryElements = query.Split(' ');
            if (queryElements.Length != 3)
            {
                throw new IncorrectRequestException();
            }
            HttpMethod method;
            parsedRequest.Method = Enum.TryParse(queryElements[0], out method) ? method : HttpMethod.GET; // TODO: подумать, нужно ли здесь значение по умолчанию
            parsedRequest.RequestedResource = queryElements[1];
            parsedRequest.Version = queryElements[2].Substring(queryElements[2].IndexOf("/") + 1);
            
            for (int i = 1; i < tokens.Length; ++i) // TODO: отрефакторить
            {
                switch (emptyStringCount)
                {
                    case 0:
                        if (!String.IsNullOrWhiteSpace(tokens[i]))
                        {
                            var colonPosition = tokens[i].IndexOf(":");
                            if (colonPosition > 0)
                            {
                                var header = tokens[i].Substring(0, colonPosition).Trim();
                                var headerValue = tokens[i].Substring(colonPosition + 2).Trim(); // TODO: подумать про 1
                                parsedRequest.Headers.Add(header, headerValue);
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
