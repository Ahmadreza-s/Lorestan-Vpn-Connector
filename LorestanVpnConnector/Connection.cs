using System.Net;

namespace LorestanVpnConnector
{
    public class Connection
    {
        public bool IsWifiConnected()
        {
            try
            {
                
                var str = new WebClient().DownloadString("http://internet.lu.ac.ir/");
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool IsConnected()
        {
            var str = new WebClient().DownloadString("http://internet.lu.ac.ir/status");
            return !str.Contains("login");
        }

        public bool Login(string username, string password)
        {
            var resp = new MyWebRequest("http://internet.lu.ac.ir/login", "POST",
                $"username={username}&password={password}").GetResponse();
            return !resp.Contains("login");
        }

        public void Logout() => new WebClient().DownloadString("http://internet.lu.ac.ir/logout");
    }
}
