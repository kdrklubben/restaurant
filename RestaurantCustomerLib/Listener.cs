using RestaurantCustomerLib.Delegates;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace RestaurantCustomerLib
{
    public class Listener
    {
        public bool listen;
        private NetworkStream networkstream;
        public event AuthConfirmed AuthConfirmed;
        public event AuthDenied AuthDenied;
        public event OrderDone OrderDone;
        public Listener(NetworkStream stream)
        {
            networkstream = stream;
            listen = true;
            CommandListener();
        }
        public void CommandListener()
        {
            while (listen)
            {
                byte[] buffer = new byte[1024];
                int recieve = networkstream.Read(buffer, 0, buffer.Length);
                string message = Encoding.ASCII.GetString(buffer, 0, recieve);
                
                Match data = Regex.Match(message,"();()");
                // Invoke events for corresponding order type, in order to never directly call a specific UI from the lib
                switch (data.Groups[0].ToString())
                {
                    case "AUTHCONFIRMED":
                        AuthConfirmed.Invoke(data.Groups[1].ToString());
                        break;
                    case "AUTHDENIED":
                        AuthDenied.Invoke(data.Groups[1].ToString());
                        break;
                    case "ORDERDONE":
                        OrderDone.Invoke(data.Groups[1].ToString());
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
