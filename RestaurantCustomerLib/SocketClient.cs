using Newtonsoft.Json;
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

        IPEndPoint RemoteEndPoint()
        {
            // In a real production environment, the server EndPoint would be known, so this is available for development purposes
            IPAddress iPAddress;
            bool isValid = false;
            string input = "";
            do
            {
                Console.WriteLine("Provide IP adress (defaults to 127.0.0.1)");
                input = Console.ReadLine();
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
            Console.WriteLine("Contacting server");
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
                Console.WriteLine("The server is unavailable");
                Connect();
            }
        }
        public void Disconnect()
        {
            sender.Command("DISCONNECT");
        }
        public void GetDishes()
        {
            sender.Command("GETDISHES");
        }
        public void Order(int id)
        {
            //sender.Command($"PLACEORDER;{JsonConvert.SerializeObject(id)}");
            // TODO remove debug once proven as working
            sender.Command($"PLACEORDER;1");
        }
        public void Login(string name)
        {
            sender.Command($"LOGIN;{name}");
        }
    }
}