using RestaurantLib;
using RestaurantLib.Extensions;
using RestaurantServer.Models;
using RestaurantServer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantServer.Systems
{
    internal sealed class ServerSystem
    {
        private static readonly ServerSystem _instance = new ServerSystem();
        private readonly Socket _socket;
        internal readonly List<Dish> Dishes;
        internal readonly List<Customer> CustomerConnections;
        internal Socket Kitchen { get; set; }

        static ServerSystem()
        { }

        private ServerSystem()
        {
            _socket = SocketUtility.CreateServerSocket();
            Dishes = SerializationUtility.ReadDishes();
            CustomerConnections = new List<Customer>();
        }

        internal static ServerSystem Instance
        {
            get { return _instance; }
        }

        internal void PlaceOrder(int dishId, Customer customer)
        {
            Dish dish = Dishes.SingleOrDefault(x => x.DishId == dishId);
            if (dish != null)
            {
                customer.Orders.Add(new Order() { Dish = dish, IsDone = false });
            }
        }

        internal void Listen()
        {
            while (true)
            {
                _socket.Listen(3);
                Socket clientSocket = _socket.Accept();
                
                new Task(() => WaitForAuthentication(clientSocket)).Start();
            }
        }

        private void WaitForAuthentication(Socket socket)
        {
            while (true)
            {
                socket.Send("Please enter your desired username".ToUtf8ByteArray());

                byte[] buffer = new byte[1024];
                int byteCount = socket.Receive(buffer);

                string response = Encoding.UTF8.GetString(buffer, 0, byteCount);

                ConsoleLogger.LogInformation($"Got message: { response }");

                if (response == "DISCONNECT")
                {
                    ConsoleLogger.LogWarning($"User gave up while choosing username ({ socket.RemoteEndPoint })");
                    SocketUtility.CloseConnection(socket);
                    break;
                }
                else if (!String.IsNullOrWhiteSpace(response))
                {
                    //if kitchen
                    if (response == "kitchen")
                    {
                        if (Kitchen == null || !Kitchen.Connected)
                        {
                            Kitchen = socket;
                            ConsoleLogger.LogInformation($"Kitchen logged on from { socket.RemoteEndPoint }.");
                            socket.Send("CODE:200;You are now authenticated as the kitchen".ToUtf8ByteArray());
                            break;
                        }
                        else
                        {
                            socket.Send("CODE:403;There is already a kitchen client connected".ToUtf8ByteArray());
                            SocketUtility.CloseConnection(socket);
                            break;
                        }
                    }

                    //if customer
                    if (!CustomerConnections.Any(x => x.Username == response))
                    {
                        //username is vacant
                        Customer customer = new Customer() { Socket = socket, Username = response };
                        CustomerConnections.Add(customer);

                        socket.Send($"AUTHCONRFIRMED;You are now logged in as { response }.".ToUtf8ByteArray());
                        new ServerClient(customer);

                        ConsoleLogger.LogInformation($"New user connected { response } from { customer.Socket.RemoteEndPoint }");

                        break;
                    }
                    else if (CustomerConnections.Any(x => x.Username == response && x.Socket.Connected))
                    {
                        //the username is occupied and socket busy
                        socket.Send($"CODE:403;Someone else is already using the username { response } - please choose a different one.".ToUtf8ByteArray());
                    }
                    else
                    {
                        //the username is occupied and socket vacant
                        Customer customer = CustomerConnections.Single(x => x.Username == response);
                        customer.Socket = socket;
                        
                        socket.Send($"CODE:200;Welcome back { response }. We've saved your orders for you, but the kitchen might have discarded old, unclaimed orders.".ToUtf8ByteArray());
                        new ServerClient(customer);

                        ConsoleLogger.LogInformation($"User { response } reconnected from { customer.Socket.RemoteEndPoint }");

                        break;
                    }
                }
            }
        }
    }
}
