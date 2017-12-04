using Newtonsoft.Json;
using RestaurantCustomerLib.Delegates;
using RestaurantLib;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace RestaurantCustomerLib
{
    public class Listener
    {
        public bool listen;
        private NetworkStream networkstream;
        private SocketClient client;
        public event LoginResponse LoginResponse;
        public event AuthConfirmed AuthConfirmed;
        public event AuthDenied AuthDenied;
        public event OrderDone OrderDone;
        public event GetDishes GetDishes;
        public event GetOrders GetOrders;
        public event SetAvailable SetAvailable;
        public Listener(NetworkStream stream)
        {
            networkstream = stream;
            listen = true;
        }
        public void CommandListener()
        {
            while (listen)
            {
                byte[] buffer = new byte[1024];
                int recieve = networkstream.Read(buffer, 0, buffer.Length);
                string message = Encoding.UTF8.GetString(buffer, 0, recieve);

                string[] data = message.Split(';');
                // Invoke events for corresponding order type, in order to never directly call a specific UI from the lib
                switch (data[0])
                {
                    case "LOGIN":
                        LoginResponse.Invoke(data[1]);
                        break;
                    case "GETDISHES":
                        List<Dish> dishes = JsonConvert.DeserializeObject<List<Dish>>(data[1]);
                        GetDishes.Invoke(dishes);
                        break;
                    case "GETORDERS":
                        List<Order> orders = JsonConvert.DeserializeObject<List<Order>>(data[1]);
                        GetOrders.Invoke(orders);
                        break;
                    case "AUTHCONFIRMED":
                        AuthConfirmed.Invoke(data[1]);
                        break;
                    case "AUTHDENIED":
                        AuthDenied.Invoke(data[1]);
                        break;
                    case "ORDERDONE":
                        OrderDone.Invoke(data[1]);
                        break;
                    case "SETAVAILABLE":
                        DishAvailableModel available = JsonConvert.DeserializeObject<DishAvailableModel>(data[1]);
                        SetAvailable.Invoke(available);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
