using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebServer.DataModel;

namespace webserver
{
    public class Server
    {
        private TcpListener listener;
        private RequestParser requestParser;
        private RequestReader requestReader;
        private FileReader fileReader;
        private IAuthenticator authenticator;

        private Server(TcpListener listener)
        {
            this.listener = listener;
            requestParser = new RequestParser();
            requestReader = new RequestReader();
            fileReader = new FileReader();
            authenticator = new Authenticator();
        }

        public static Server StartNew(IPAddress startAddress, int port)
        {
            var listener = new TcpListener(startAddress, port);
            listener.Start();
            return new Server(listener);
        }

        public void Stop()
        {
            listener.Stop();
        }

        public void HandleRequest()
        {
            var client = listener.AcceptTcpClient();
            Console.WriteLine("New client has connected from address {0}",
                ((IPEndPoint) client.Client.RemoteEndPoint).Address.ToString());
            var rawRequest = requestReader.ReadRawRequest(client);
            var request = requestParser.ParseRequest(rawRequest);
            //Authenticate(request, client);

            var cookie = request.Headers["Cookie"];

            if (String.IsNullOrWhiteSpace(cookie)) // TODO: проверять параметры куки (sid, не протухла ли)
            {
                var isAuthenticated = authenticator.CheckAuthentication(request);
                if (!isAuthenticated)
                {
                    //var response =
                    //  "HTTP/1.1 401 Unauthorized \r\nWWW-Authenticate: Digest realm=\"Enter login and password\",nonce=\'ololo\'\r\nContent-Length: 0\r\nConnection: close\r\n\r\n";
                    var authPage = fileReader.ReadFile(Environment.CurrentDirectory + "\\auth.html");
                    var response = String.Format(
                        "HTTP/1.1 401 Unauthorized \r\nContent-type: text/html\r\nContent-Length: {0}\r\nConnection: close\r\n\r\n",
                        1024*authPage.Count()); // 1024 ли?
                    var buffer = Encoding.UTF8.GetBytes(response);
                    client.GetStream().Write(buffer, 0, buffer.Length);
                    foreach (var element in authPage)
                    {
                        client.GetStream().Write(element, 0, element.Length);
                    }
                    return;
                }
            }

            // Приводим ее к изначальному виду, преобразуя экранированные символы
            // Например, "%20" -> " "
            var requestUri = request.RequestedUri.ToString();

            // Если в строке содержится двоеточие, передадим ошибку 400
            // Это нужно для защиты от URL типа http://example.com/../../file.txt
            if (requestUri.IndexOf("..") >= 0)
            {
                SendError(client, 400);
                return;
            }

            // Если строка запроса оканчивается на "/", то добавим к ней index.html
            if (requestUri.EndsWith("/"))
            {
                requestUri += "index.html";
            }

            string filePath = Environment.CurrentDirectory + requestUri.Replace(@"/", @"\");

            // Если в папке www не существует данного файла, посылаем ошибку 404
            if (!File.Exists(filePath))
            {
                SendError(client, 404);
                return;
            }

            // Получаем расширение файла из строки запроса
            string Extension = requestUri.Substring(requestUri.LastIndexOf('.'));

            // Тип содержимого
            string contentType = "";

            // Пытаемся определить тип содержимого по расширению файла
            switch (Extension)
            {
                case ".htm":
                case ".html":
                    contentType = "text/html";
                    break;
                case ".css":
                    contentType = "text/stylesheet";
                    break;
                case ".js":
                    contentType = "text/javascript";
                    break;
                case ".jpg":
                    contentType = "image/jpeg";
                    break;
                case ".jpeg":
                case ".png":
                case ".gif":
                    contentType = "image/" + Extension.Substring(1);
                    break;
                default:
                    if (Extension.Length > 1)
                    {
                        contentType = "application/" + Extension.Substring(1);
                    }
                    else
                    {
                        contentType = "application/unknown";
                    }
                    break;
            }

            // Открываем файл, страхуясь на случай ошибки
            IEnumerable<byte[]> file;
            try
            {
                file = fileReader.ReadFile(filePath); // TODO: подумать про результаты операций
            }
            catch (Exception)
            {
                // Если случилась ошибка, посылаем клиенту ошибку 500

                SendError(client, 500);
                return;
            }

            // Посылаем заголовки
            string headers;
            if (String.IsNullOrWhiteSpace(cookie)) // TODO: сделать нормально
            {
                headers = "HTTP/1.1 200 OK\r\nSet-Cookie: RMID=732423sdfs73242; expires=Fri, 31 Oct 2016 23:59:59 GMT; path=/; domain=localhost\r\nContent-Type: " + contentType + "\r\nContent-Length: " +
                          file.Sum(x => x.Length) + "\r\n\r\n";
            }
            else
            {
                headers = "HTTP/1.1 200 OK\r\nContent-Type: " + contentType + "\r\nContent-Length: " +
                          file.Sum(x => x.Length) + "\r\n\r\n";
            }
            byte[] headersBuffer = Encoding.ASCII.GetBytes(headers);
            client.GetStream().Write(headersBuffer, 0, headersBuffer.Length);
            foreach (var filePiece in file)
            {
                client.GetStream().Write(filePiece, 0, filePiece.Length);
            }

            client.Close();
        }

        private void Authenticate(Request request, TcpClient client) // TODO: вынести все ответы в одно место
        {
            string response;
            byte[] buffer;
            if (!authenticator.CheckAuthentication(request))
            {
                response =
                    "HTTP/1.1 401 Unauthorized \r\nWWW-Authenticate: Digest realm=\"User Visible Realm\", nonce=\'ololo\'\r\nContent-Length: 0\r\nConnection: close\r\n\r\n";
                buffer = Encoding.UTF8.GetBytes(response);
                client.GetStream().Write(buffer, 0, buffer.Length);
            }
            response =
                    "HTTP/1.1 200 Ok \r\nContent-Length: 0\r\nConnection: close\r\n\r\n";
            buffer = Encoding.UTF8.GetBytes(response);
            client.GetStream().Write(buffer, 0, buffer.Length);
        }

        private void SendError(TcpClient Client, int Code)
        {
            // Получаем строку вида "200 OK"
            // HttpStatusCode хранит в себе все статус-коды HTTP/1.1
            string CodeStr = Code.ToString() + " " + ((HttpStatusCode)Code).ToString();
            // Код простой HTML-странички
            string Html = "<html><body><h1>" + CodeStr + "</h1></body></html>";
            // Необходимые заголовки: ответ сервера, тип и длина содержимого. После двух пустых строк - само содержимое
            string Str = "HTTP/1.1 " + CodeStr + "\nContent-type: text/html\nContent-Length:" + Html.Length.ToString() + "\n\n" + Html;
            // Приведем строку к виду массива байт
            byte[] Buffer = Encoding.ASCII.GetBytes(Str);
            // Отправим его клиенту
            Client.GetStream().Write(Buffer, 0, Buffer.Length);
            // Закроем соединение
            Client.Close();
        }
    }
}
