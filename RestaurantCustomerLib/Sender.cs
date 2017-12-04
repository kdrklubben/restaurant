using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace RestaurantCustomerLib
{
    class Sender
    {
        private NetworkStream networkstream;
        public Sender(NetworkStream stream)
        {
            networkstream = stream;
        }

        public void Command(string command)
        {            
            bool isValid = Regex.IsMatch(command, "();()");
            if (!isValid) return;

            byte[] bytesToSend = Encoding.ASCII.GetBytes(command);
            networkstream.Write(bytesToSend, 0, bytesToSend.Length);
        }
    }
}
