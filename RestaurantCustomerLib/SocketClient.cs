using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
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
            // Realistically, the server EndPoint is known, so this shouldn't be required of the user... right?
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

            int port;
            do
            {
                Console.WriteLine("Provide a port (defaults to 8080)");
                input = Console.ReadLine();
                if (input == "")
                {
                    port = 8080;
                    break;
                }
                isValid = int.TryParse(input, out port);
            } while (!isValid);

            IPEndPoint endPoint = new IPEndPoint(iPAddress, port);
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
                int recv = networkstream.Read(buffer, 0, buffer.Length);
                Match response = Regex.Match(Encoding.ASCII.GetString(buffer, 0, recv),"();()");

                Console.WriteLine(response.Groups[1].ToString());

                // TODO Find way to not proceed until task is started
                Task task = new Task(() => Listener = new Listener(networkstream));
                task.Start();

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
            sender.Command($"PLACEORDER;{JsonConvert.SerializeObject(id)}");
        }
        public void Login(string name)
        {
            sender.Command($"LOGIN;{name}");
        }
    }
}