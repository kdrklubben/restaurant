using Newtonsoft.Json;
using RestaurantCustomerLib.Delegates;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RestaurantCustomerLib
{
    public class SocketClient
    {
        TcpClient client;
        internal static NetworkStream networkstream;
        public Listener Listener { get; private set; }
        private Sender sender;
        public event ServerUnavailableError ServerUnavailableError;
        public event PromptIpAddress PromptIpAddress;
        public event OrderPlaced OrderPlaced;
        IPEndPoint RemoteEndPoint()
        {
            // In a real production environment, the server EndPoint would be known, so this is available for development purposes
            IPAddress iPAddress;
            bool isValid = false;
            string input = "";
            do
            {
                input = PromptIpAddress.Invoke(); 
                if (input == "")
                {
                    input = "127.0.0.1";
                }
                isValid = IPAddress.TryParse(input, out iPAddress);
            } while (!isValid);

            IPEndPoint endPoint = new IPEndPoint(iPAddress, 8080);
            return endPoint;
        }

        public void Connect()
        {
            var endpoint = RemoteEndPoint();
            byte[] buffer = new byte[1024];
            client = new TcpClient();
            try
            {
                client.Connect(endpoint);
                networkstream = client.GetStream();

                Listener = new Listener(networkstream);
                new Task(() => Listener.CommandListener()).Start();

                sender = new Sender(networkstream);
            }
            catch
            {
                ServerUnavailableError.Invoke();
            }
        }
        public void Disconnect() => sender.Command("DISCONNECT;");
        public void GetDishes() => sender.Command("GETDISHES;");
        public void Order(int id)
        {
            sender.Command($"PLACEORDER;{JsonConvert.SerializeObject(id)}");
            OrderPlaced.Invoke(id);
            sender.Command("GETORDERS;");
        }

        public void Login(string name) => sender.Command($"LOGIN;{name}");
    }
}