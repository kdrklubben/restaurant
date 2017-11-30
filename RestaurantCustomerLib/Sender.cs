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
            //Match data = Regex.Match(command,"();()");
            //string order = data.Groups[0].ToString();
            //string json = data.Groups[1].ToString();
            //char[] chars = $"{order};{json}".ToCharArray();
            
            bool isValid = Regex.IsMatch(command, "();()");
            if (!isValid) return;
            char[] chars = command.ToCharArray();
            byte[] bytesToSend = Encoding.ASCII.GetBytes(chars, 0, command.Length);
            networkstream.Write(bytesToSend, 0, bytesToSend.Length);
        }
    }
}
