using Newtonsoft.Json;
using RestaurantServer.Utilities;
using System;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RestaurantServer.Systems
{
    internal class KitchenClient
    {
        internal readonly Socket Socket;

        public KitchenClient(Socket socket)
        {
            Socket = socket;
            new Task(() => Listen()).Start();
        }

        private void Listen()
        {
            while (true && Socket != null && Socket.Connected)
            {
                byte[] buffer = new byte[1024];
                int byteCount = Socket.Receive(buffer);
                if (byteCount == 0)
                    break;

                string response = Encoding.UTF8.GetString(buffer, 0, byteCount);

                if (!String.IsNullOrWhiteSpace(response))
                {
                    Regex dishReadyPattern = new Regex(@"(ORDERDONE);(.*)");
                    Regex getDishesPattern = new Regex(@"(GETDISHES);(.*)");
                    Regex getOrdersPattern = new Regex(@"(GETORDERS);(.*)");

                    if (dishReadyPattern.IsMatch(response))
                    {
                        Match match = dishReadyPattern.Match(response);
                        int orderId = JsonConvert.DeserializeObject<int>(match.Groups[1].Value);
                        ServerSystem.Instance.ConfirmOrder(orderId);
                    }
                    else if (getDishesPattern.IsMatch(response))
                    {
                        ServerSystem.Instance.SendDishes(Socket);
                    }
                    else if (getOrdersPattern.IsMatch(response))
                    {
                        ServerSystem.Instance.SendUnfinishedOrdersToKitchen();
                    }
                    else if (response == "DISCONNECT" || Regex.IsMatch("DISCONNECT;.*", response))
                    {
                        break;
                    }
                }
            }

            SocketUtility.CloseConnection(Socket);
        }
    }
}
