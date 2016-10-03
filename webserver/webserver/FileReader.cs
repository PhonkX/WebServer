using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webserver
{
    public class FileReader
    {
        public IEnumerable<byte[]> ReadFile(string filename)
        {
            using (var fs = File.OpenRead(filename))
            {
                byte[] buffer = new byte[1024];
                while (fs.Read(buffer, 0, buffer.Length) > 0)
                {
                    yield return buffer;
                }
            }
        }
    }
}
