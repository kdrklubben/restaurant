using RestaurantServer.Models;
using RestaurantServer.Utilities;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RestaurantServer.Systems
{
    internal class ServerClient
    {
        private readonly Customer _client;

        public ServerClient(Customer client)
        {
            _client = client;
            Task listen = new Task(() => Listen());
            listen.Start();
        }

        private void Listen()
        {
            while (true && _client != null && _client.Socket.Connected)
            {
                byte[] buffer = new byte[1024];
                int byteCount = _client.Socket.Receive(buffer);

                string response = Encoding.UTF8.GetString(buffer, 0, byteCount);

                if (!String.IsNullOrWhiteSpace(response))
                {
                    Regex order = new Regex(@"(ORDER:)(\d+)");
                    if (response == "DISCONNECT")
                    {
                        break;
                    }
                    else if (order.IsMatch(response))
                    {
                        foreach (Match match in order.Matches(response))
                        {
                            ServerSystem.Instance.PlaceOrder(int.Parse(match.Groups[1].ToString()), _client);
                        }
                    }
                }
            }

            SocketUtility.CloseConnection(_client.Socket);
        }
    }
}
