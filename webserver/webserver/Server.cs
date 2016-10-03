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
        private MD5CryptoServiceProvider md5Computer;

        private Server(TcpListener listener)
        {
            this.listener = listener;
            requestParser = new RequestParser();
            requestReader = new RequestReader();
            fileReader = new FileReader();
            md5Computer = new MD5CryptoServiceProvider();
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
            var isAuthenticated = CheckAuthentication(request);
            if (!CheckAuthentication(request))
            {
                var response =
                    "HTTP/1.1 401 Unauthorized \r\nWWW-Authenticate: Digest realm=\"Enter login and password\",nonce=\'ololo\'\r\nContent-Length: 0\r\nConnection: close\r\n\r\n";
                var buffer = Encoding.UTF8.GetBytes(response);
                client.GetStream().Write(buffer, 0, buffer.Length);
                return;
            }

            // Приводим ее к изначальному виду, преобразуя экранированные символы
            // Например, "%20" -> " "
            var requestUri = Uri.UnescapeDataString(request.RequestedResource);

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
            string headers = "HTTP/1.1 200 OK\r\nContent-Type: " + contentType + "\r\nContent-Length: " + file.Sum(x => x.Length) + "\r\n\r\n";
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
            if (!CheckAuthentication(request))
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

        private bool CheckAuthentication(Request request)
        {
            var authString = request.Headers["Authorization"];
            if (String.IsNullOrWhiteSpace(authString))
            {
                return false;
            }

            var authStringElements = authString.Split(',');
            var userNameString = authStringElements.FirstOrDefault(x => x.Contains("username"));
            var responseString = authStringElements.FirstOrDefault(x => x.Contains("response"));
            var nonceString = authStringElements.FirstOrDefault(x => x.Contains("nonce"));
            if (String.IsNullOrWhiteSpace(userNameString)
                || String.IsNullOrWhiteSpace(responseString)
                || String.IsNullOrWhiteSpace(nonceString))
            {
                return false;
            }

            var userName = userNameString.Split('=')[1].Trim(new [] { '\\', '\"' });
            if (userName != "au")
            {
                return false;
            }
            
            var nonce = nonceString.Split('=')[1].Trim(new[] { '\\', '\"' });
            var response = responseString.Split('=')[1].Trim(new[] { '\\', '\"' });
            //var loginPass = Encoding.UTF8.GetString(Convert.FromBase64String(loginPasswordString.Split(' ')[1])).Split(':');
            // Там base64
            var password = "ololo";
            var serverSideAuthA1String = new StringBuilder() // как на Вики - A1
                .Append(userName)
                .Append(":")
                .Append("Enter login and password")
                .Append(":")
                .Append(password)
                .ToString();
            //var authA1Md5 = Encoding.ASCII.GetString(md5Computer.ComputeHash(Encoding.ASCII.GetBytes(serverSideAuthA1String)));
            var authA1Md5 = ComputeMD5Hash(serverSideAuthA1String);
            // TODO: сделать сущность, считающие хэши
            var serverSideAuthA2String = new StringBuilder() // как на Вики - A2
                .Append(request.Method)
                .Append(":")
                .Append(request.RequestedResource)
                .ToString();
            var authA2Md5 = ComputeMD5Hash(serverSideAuthA2String);

            var serverSideAuthString = new StringBuilder() // как на Вики - A1
                .Append(authA1Md5)
                .Append(":")
                .Append(nonce)
                .Append(":")
                .Append(authA2Md5)
                .ToString();

            var authMd5 = ComputeMD5Hash(serverSideAuthString);

            if (authMd5 != response)
            {
                return false;
            }

            return true;
        }

        private string ComputeMD5Hash(string s) // TODO: вынести в другой класс
        {
            var stringBytes = s.Select(c => Convert.ToByte(c));
            var stream = new MemoryStream();
            foreach (var stringByte in stringBytes)
            {
                stream.WriteByte(stringByte);
            }
            var hash = md5Computer.ComputeHash(stream.ToArray()).Select(x => x.ToString("x2"));
            return String.Join("", hash);
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
