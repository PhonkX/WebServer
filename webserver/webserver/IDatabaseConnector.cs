using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webserver
{
    public interface IDatabaseConnector
    {
        string SearchPasswordByLogin(string login);
    }
}
