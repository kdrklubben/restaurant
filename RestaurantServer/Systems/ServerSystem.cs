using Newtonsoft.Json;
using RestaurantLib;
using RestaurantLib.Extensions;
using RestaurantServer.Extensions;
using RestaurantServer.Models;
using RestaurantServer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
                customer.Orders.Add(new Order() { Dish = dish, IsDone = false, OrderPlaced = DateTime.Now });
                //todo add broadcast to kitchen
            }
        }

        internal void ConfirmOrder(int orderId)
        {
            Customer orderCustomer = CustomerConnections.SingleOrDefault(x => x.Orders.Any(y => y.OrderId == orderId));
            if (orderCustomer != null)
            {
                Order order = orderCustomer.Orders.Single(x => x.OrderId == orderId);
                order.IsDone = true;
                if (orderCustomer.Socket.Connected)
                {
                    orderCustomer.Socket.SendString("ORDERDONE", JsonConvert.SerializeObject(orderId));
                }
                orderCustomer.Orders.Remove(order);
            }
        }

        internal void SendDishes(Socket socket)
        {
            socket.SendString("GETDISHES", JsonConvert.SerializeObject(Dishes));
        }

        internal void SendOrders(Socket socket)
        {
            List<Order> orders = new List<Order>();
            CustomerConnections.ForEach(x => orders.AddRange(x.Orders.Where(y => !y.IsDone)));

            socket.SendString("GETORDERS", JsonConvert.SerializeObject(orders));
        }

        private void Listen()
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
            ConsoleLogger.LogInformation($"New connection from { socket.RemoteEndPoint }. Waiting for authentication.");
            while (true && socket != null && socket.Connected)
            {
                socket.SendString("LOGIN", "Please enter your desired username");

                byte[] buffer = new byte[1024];
                int byteCount = socket.Receive(buffer);
                if (byteCount == 0)
                    break;

                string response = Encoding.UTF8.GetString(buffer, 0, byteCount);

                Regex loginPattern = new Regex("(LOGIN);p{L}+");
                if (response == "DISCONNECT" || Regex.IsMatch("DISCONNECT;.*", response))
                {
                    ConsoleLogger.LogWarning($"User gave up while choosing username ({ socket.RemoteEndPoint })");
                    SocketUtility.CloseConnection(socket);
                    break;
                }
                else if (loginPattern.IsMatch(response))
                {
                    string username = loginPattern.Match(response).Groups[1].Value;
                    if (username == "kitchen")
                    {
                        if (Kitchen == null || !Kitchen.Connected)
                        {
                            Kitchen = socket;
                            ConsoleLogger.LogInformation($"Kitchen logged on from { socket.RemoteEndPoint }.");
                            socket.SendString("AUTHCONFIRMED", "You are now authenticated as the kitchen");
                            break;
                        }
                        else
                        {
                            socket.SendString("AUTHDENIED", "There is already a kitchen client connected");
                            SocketUtility.CloseConnection(socket);

                            ConsoleLogger.LogWarning($"Refused kitchen login attempt from { socket.RemoteEndPoint }");
                            break;
                        }
                    }
                    else if (!CustomerConnections.Any(x => x.Username == username))
                    {
                        //username is vacant
                        Customer customer = new Customer() { Socket = socket, Username = username };
                        CustomerConnections.Add(customer);

                        socket.SendString("AUTHCONFIRMED", $"You are now logged in as { username }.");
                        new CustomerClient(customer);

                        ConsoleLogger.LogInformation($"New user { username } connected from { customer.Socket.RemoteEndPoint }");
                        break;
                    }
                    else if (CustomerConnections.Any(x => x.Username == username && x.Socket.Connected))
                    {
                        //the username is occupied and socket busy
                        socket.SendString("AUTHDENIED", $"Someone else is already using the username { username } - please choose a different one.");
                    }
                    else
                    {
                        //the username is occupied and socket vacant
                        Customer customer = CustomerConnections.Single(x => x.Username == username);
                        customer.Socket = socket;

                        socket.SendString("AUTHCONFIRMED", $"Welcome back { username }. We've saved your orders for you, but old orders might have been discarded.");
                        new CustomerClient(customer);

                        ConsoleLogger.LogInformation($"User { username } reconnected from { customer.Socket.RemoteEndPoint }");
                        break;
                    }
                }
                else
                {
                    ConsoleLogger.LogError($"Invalid format from { socket.RemoteEndPoint } while waiting for authentication:\n{ response }");
                }
            }
        }

        internal void StartServer()
        {
            new Task(() => Listen()).Start();
            new Task(() => Timer()).Start();
            Console.WriteLine("Server started using loopback address. Press ESC to shutdown and quit.");

            while (true)
            {
                if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                    ShutdownServer();
            }
        }

        internal void ShutdownServer()
        {
            SocketUtility.CloseAllConnections();
            Environment.Exit(0);
        }

        internal void Timer()
        {
            new Timer(RemoveOldOrders, new AutoResetEvent(false), 60000, 60000);
        }

        internal void RemoveOldOrders(Object stateinfo)
        {
            foreach (var customer in CustomerConnections)
            {
                customer.Orders.RemoveAll(x => x.OrderPlaced - DateTime.Now > TimeSpan.FromHours(1) && x.IsDone);               
            }
            ConsoleLogger.LogInformation($"All done orders that were placed before {DateTime.Now.AddHours(-1)} were cleaned up.");
        }
    }   
}
