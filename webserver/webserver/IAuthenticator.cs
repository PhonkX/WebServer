using WebServer.DataModel;

namespace webserver
{
    public interface IAuthenticator
    {
        bool CheckAuthentication(Request request);
    }
}