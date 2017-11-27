using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantCustomerLib
{
    //* On start, provide a unique identifier
    //* Let user view the menu
    //* Let user place an order
    //    * send that order to the server
    //* Recieve notification when user's order is complete
    //	* client must listen for notifications
    public class SocketClient
    {
        TcpClient client; // Helper on Socket class, e.g. defaults to tcp protocol
        NetworkStream networkstream;

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

                Task task = new Task(() => NotificationListen());
                task.Start();

                SendOrder(Console.ReadLine());
            }
            catch
            {
                Console.WriteLine("The server is unavailable");
                Connect();
            }
        }

        void SendOrder(string message)
        {
            Console.WriteLine("Exit with 'exit'");
            while (true)
            {                
                if (message == "exit") break;

                char[] chars = message.ToCharArray();
                byte[] bytesToSend = Encoding.ASCII.GetBytes(chars, 0, message.Length);
                networkstream.Write(bytesToSend, 0, bytesToSend.Length);
                networkstream.Flush();
            }
        }

        void NotificationListen()
        {
            Console.WriteLine("Recieved data from server");
            while (true)
            {
                byte[] buffer = new byte[1024];
                int recieve = networkstream.Read(buffer, 0, buffer.Length);
                string message = Encoding.ASCII.GetString(buffer, 0, recieve);
                Console.WriteLine($"\t\t<{message}>");
            }
        }

        public bool IsUniqueName(string name)
        {
            List<string> takenNames = new List<string>(); // equals result of a name-fetching call to the server
            foreach (string item in takenNames)
            {
                if (item == name)
                {
                    return false;
                }
            }
            return true;
        }
    }
}