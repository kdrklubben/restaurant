using Newtonsoft.Json;
using RestaurantServer.Models;
using RestaurantServer.Utilities;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RestaurantServer.Systems
{
    internal class CustomerClient
    {
        private readonly Customer _client;

        public CustomerClient(Customer client)
        {
            _client = client;
            new Task(() => Listen()).Start();
        }

        private void Listen()
        {
            while (true && _client.Socket != null && _client.Socket.Connected)
            {
                byte[] buffer = new byte[1024];
                int byteCount = _client.Socket.Receive(buffer);
                if (byteCount == 0)
                    break;

                string response = Encoding.UTF8.GetString(buffer, 0, byteCount);

                if (!String.IsNullOrWhiteSpace(response))
                {
                    Regex placeOrderPattern = new Regex(@"(PLACEORDER);(.+)");
                    Regex getDishesPattern = new Regex(@"(GETDISHES);(.*)");
                    Regex getOrdersPattern = new Regex(@"(GETORDERS);(.*)");

                    if (placeOrderPattern.IsMatch(response))
                    {
                        Match match = placeOrderPattern.Match(response);
                        ServerSystem.Instance.PlaceOrder(JsonConvert.DeserializeObject<int>(match.Groups[2].Value), _client);
                    }
                    else if (getDishesPattern.IsMatch(response))
                    {
                        ServerSystem.Instance.SendDishes(_client.Socket);
                    }
                    else if (getOrdersPattern.IsMatch(response))
                    {
                        ServerSystem.Instance.SendCustomerOrders(_client);
                    }
                    else if (response == "DISCONNECT" || Regex.IsMatch("DISCONNECT;.*", response))
                    {
                        break;
                    } 
                }
            }

            SocketUtility.CloseConnection(_client.Socket);
        }
    }
}
