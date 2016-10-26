using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace webserver
{
    public class SqlConnector: IDatabaseConnector
    {
        private string conn;

        public SqlConnector()
        {
            conn = @"Data Source=PHONKXPC\SQLEXPRESS;Initial Catalog=Webserver;Integrated Security=True";
        }

        public string SearchPasswordByLogin(string login)
        {
            string password = "";
            using (SqlConnection sc = new SqlConnection(conn))
            {
                sc.Open();
                string searchUser = @"SELECT Password FROM dbo.Users WHERE Login=@login";
                var sqlCommand = new SqlCommand(searchUser, sc);
                var loginParam = new SqlParameter("@login", SqlDbType.NVarChar);
                loginParam.Value = login;
                sqlCommand.Parameters.Add(loginParam);
                password = (string) sqlCommand.ExecuteScalar();
            }

            return password;
        }
    }
}
