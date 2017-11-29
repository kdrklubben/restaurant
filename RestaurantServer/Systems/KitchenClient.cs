using RestaurantServer.Models;
using RestaurantServer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RestaurantServer.Systems
{
    internal class KitchenClient
    {
        internal readonly Socket _socket;

        public KitchenClient(Socket socket)
        {
            _socket = socket;
            new Task(() => Listen()).Start();
        }

        private void Listen()
        {
            while (true && _socket != null && _socket.Connected)
            {
                byte[] buffer = new byte[1024];
                int byteCount = _socket.Receive(buffer);

                string response = Encoding.UTF8.GetString(buffer, 0, byteCount);

                if (!String.IsNullOrWhiteSpace(response))
                {
                    Regex dishReadyPattern = new Regex(@"(DISHREADY:)(\p{L}+),(\d+)");
                    Regex removeOrderPattern = new Regex(@"(REMOVEORDER:)(\p{L}+),(\d+)");
                    if (response == "DISCONNECT")
                    {
                        break;
                    }
                    else if (dishReadyPattern.IsMatch(response))
                    {
                        //message received to kitchen
                        //send order done to customer
                    }
                    else if (removeOrderPattern.IsMatch(response))
                    {
                        Match match = removeOrderPattern.Match(response);
                        //delete the order
                        Customer customer = ServerSystem.Instance.CustomerConnections.FirstOrDefault(x => x.Username == match.Groups[1].ToString());
                        Order order = customer.Orders.FirstOrDefault(x => x.Dish.DishId == int.Parse(match.Groups[2].ToString()));
                        if (order != null)
                        {
                            customer.Orders.Remove(order);
                            SocketUtility.Send(customer.Socket, $"REMOVEORDER:{ order.Dish.DishId };", $"An order for { order.Dish.Name } has been removed!");
                        }
                    }
                }
            }

            SocketUtility.CloseConnection(_socket);
        }
    }
}
