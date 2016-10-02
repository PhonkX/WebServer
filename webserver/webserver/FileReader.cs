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
                byte[] b = new byte[1024];
                while (fs.Read(b, 0, b.Length) > 0)
                {
                    yield return b;
                }
            }
        }
    }
}
