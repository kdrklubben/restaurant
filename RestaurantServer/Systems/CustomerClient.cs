using Newtonsoft.Json;
using RestaurantLib;
using RestaurantServer.Models;
using RestaurantServer.Utilities;
using System;
using System.Linq;
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
                int byteCount = 0;
                try
                {
                    byteCount = _client.Socket.Receive(buffer);
                }
                catch (Exception)
                {
                    ConsoleLogger.LogError($"User from { _client.Socket.RemoteEndPoint } has forcibly closed the connection.");
                    break;
                }
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
                        int dishId = JsonConvert.DeserializeObject<int>(match.Groups[2].Value);
                        Dish dish = ServerSystem.Instance.Dishes.SingleOrDefault(x => x.DishId == dishId);
                        if (dish != null)
                        {
                            ServerSystem.Instance.PlaceOrder(dish, _client);
                            ConsoleLogger.LogInformation($"User { _client.Username } has placed a new order for { dish.Name }");
                        }
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
                        ConsoleLogger.LogInformation($"User { _client.Username } from { _client.Socket.RemoteEndPoint } has disconnected");
                        SocketUtility.CloseConnection(_client.Socket);
                        break;
                    }
                    else
                    {
                        ConsoleLogger.LogError($"Invalid format received when listening to { _client.Username } ({ _client.Socket.RemoteEndPoint })\n\t{ response }");
                    }
                }
                else
                {
                    ConsoleLogger.LogError($"Invalid format received when listening to { _client.Username } ({ _client.Socket.RemoteEndPoint })");
                }
            }

            SocketUtility.CloseConnection(_client.Socket);
        }
    }
}
