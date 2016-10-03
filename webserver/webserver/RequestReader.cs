using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace webserver
{
    public class RequestReader
    {
        public string ReadRawRequest(TcpClient client)
        {
            StringBuilder request = new StringBuilder();
            // Буфер для хранения принятых от клиента данных
            byte[] buffer = new byte[1024];
            // Переменная для хранения количества байт, принятых от клиента
            int count;
            // Читаем из потока клиента до тех пор, пока от него поступают данные
            while ((count = client.GetStream().Read(buffer, 0, buffer.Length)) > 0)
            {
                // Преобразуем эти данные в строку и добавим ее к переменной Request
                string partialRequest = Encoding.ASCII.GetString(buffer, 0, count);
                // Запрос должен обрываться последовательностью \r\n\r\n
                // Либо обрываем прием данных сами, если длина строки Request превышает 4 килобайта
                // Нам не нужно получать данные из POST-запроса (и т. п.), а обычный запрос
                // по идее не должен быть больше 4 килобайт
                if (partialRequest.IndexOf("\r\n\r\n") >= 0 || partialRequest.Length > 4096)
                {
                    request.Append(partialRequest);
                    break;
                }

                request.Append(partialRequest);
            }

            return request.ToString();
        }
    }
}
