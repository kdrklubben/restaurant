using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantCustomerLib
{
    public class SocketClient
    {
        TcpClient client;
        internal static NetworkStream networkstream;
        public Listener Listener { get; private set; }
        private Sender sender;
        public SocketClient()
        {
            Connect();
        }

        IPEndPoint RemoteEndPoint()
        {
            // Realistically, the server EndPoint is known, so this shouldn't be required of the user... right?
            IPAddress iPAddress;
            bool isValid = false;
            do
            {
                Console.WriteLine("Provide IP adress");
                isValid = IPAddress.TryParse(Console.ReadLine(), out iPAddress);
            } while (!isValid);

            int port = 0;
            do
            {
                Console.WriteLine("Provide a port");
                isValid = int.TryParse(Console.ReadLine(), out port);
            } while (!isValid);

            IPEndPoint endPoint = new IPEndPoint(iPAddress, port);
            return endPoint;
        }

        private void Connect()
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
                string response = Encoding.ASCII.GetString(buffer, 0, recv);

                Console.WriteLine(response);

                Task task = new Task(() => Listener = new Listener(networkstream));
                task.Start();

                sender = new Sender(networkstream);
                //SendOrder(Console.ReadLine());
            }
            catch
            {
                Console.WriteLine("The server is unavailable");
                Connect();
            }
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