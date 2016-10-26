using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WebServer.DataModel;

namespace webserver
{
    public class Authenticator : IAuthenticator
    {
        private IDatabaseConnector databaseConnector;
        private MD5CryptoServiceProvider md5Computer;

        public Authenticator()
        {
            databaseConnector = new SqlConnector();
            md5Computer = new MD5CryptoServiceProvider();
        }

        public bool CheckAuthentication(Request request) // TODO: подумать на перевод на коды возврата
        {
            /*switch (request.Method)
            {
                case HttpMethod.GET:
                    return CheckDigestAuthentication(request);
                case HttpMethod.POST:
                    return CheckFormAuthentication(request);
                default:
                    return false;
            }*/
            return CheckFormAuthentication(request);
        }

        private bool CheckDigestAuthentication(Request request)
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

            var userName = userNameString.Split('=')[1].Trim(new[] { '\\', '\"' });

            var userPass = databaseConnector.SearchPasswordByLogin(userName);

            if (String.IsNullOrWhiteSpace(userPass))
            {
                return false;
            }

            var nonce = nonceString.Split('=')[1].Trim(new[] { '\\', '\"' });
            var response = responseString.Split('=')[1].Trim(new[] { '\\', '\"' });
            // Там base64
            var password = "ololo";
            var serverSideAuthA1String = new StringBuilder() // как на Вики - A1
                .Append(userName)
                .Append(":")
                .Append("Enter login and password")
                .Append(":")
                .Append(password)
                .ToString();
            var authA1Md5 = ComputeMD5Hash(serverSideAuthA1String);
            // TODO: сделать сущность, считающие хэши
            var serverSideAuthA2String = new StringBuilder() // как на Вики - A2
                .Append(request.Method)
                .Append(":")
                .Append(request.RequestedUri)
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

        private bool CheckFormAuthentication(Request request)
        {
            var login = request.QueryParameters["login"];
            var password = request.QueryParameters["password"];

            if (String.IsNullOrWhiteSpace(login) || String.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            var userPass = databaseConnector.SearchPasswordByLogin(login);

            if (String.IsNullOrWhiteSpace(userPass))
            {
                return false;
            }

            if (userPass != password)
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
    }
}
