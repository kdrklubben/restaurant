using Newtonsoft.Json;
using RestaurantLib;
using RestaurantServer.Extensions;
using RestaurantServer.Models;
using RestaurantServer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        internal KitchenClient Kitchen { get; set; }
        private int OrderIdCounter { get; set; }

        static ServerSystem()
        { }

        private ServerSystem()
        {
            _socket = SocketUtility.CreateServerSocket();
            Dishes = SerializationUtility.ReadDishes();
            CustomerConnections = new List<Customer>();
            PrintSplash();
        }

        internal static ServerSystem Instance
        {
            get { return _instance; }
        }

        internal void PlaceOrder(Dish dish, Customer customer)
        {
            if (dish != null)
            {
                OrderIdCounter++;
                Order order = new Order() { OrderId = OrderIdCounter, Dish = dish, IsDone = false };
                customer.Orders.Add(order);
                if (Kitchen != null && Kitchen.Socket.Connected)
                {
                    Kitchen.Socket.SendString("PLACEORDER", JsonConvert.SerializeObject(order));
                }
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
                    orderCustomer.Orders.Remove(order);
                }
                ConsoleLogger.LogInformation($"{ orderCustomer.Username }'s order with ID { orderId } for { order.Dish.Name } has been marked as done.");
            }
        }

        internal void SendDishes(Socket socket)
        {
            socket.SendString("GETDISHES", JsonConvert.SerializeObject(Dishes));
        }

        internal void SendUnfinishedOrdersToKitchen()
        {
            if (Kitchen != null && Kitchen.Socket.Connected)
            {
                List<Order> orders = new List<Order>();
                CustomerConnections.ForEach(x => orders.AddRange(x.Orders.Where(y => !y.IsDone)));

                Kitchen.Socket.SendString("GETORDERS", JsonConvert.SerializeObject(orders));
            }
        }

        internal void SendCustomerOrders(Customer customer)
        {
            customer.Socket.SendString("GETORDERS", JsonConvert.SerializeObject(customer.Orders));
        }

        internal void SetDishAvailable(int dishId, bool isAvailable)
        {
            Dish dish = Dishes.SingleOrDefault(x => x.DishId == dishId);
            if (dish != null)
            {
                dish.IsAvailable = isAvailable;
                if (isAvailable)
                    ConsoleLogger.LogInformation($"{ dish.Name } has been set to available");
                else
                    ConsoleLogger.LogInformation($"{ dish.Name } has been set to unavailable");

                DishAvailableModel dishAvailable = new DishAvailableModel() { DishId = dishId, IsAvailable = isAvailable };
                List<Socket> sockets = new List<Socket>();
                CustomerConnections.ForEach(x => sockets.Add(x.Socket));

                SocketUtility.SendStringToAll(sockets, "SETAVAILABLE", JsonConvert.SerializeObject(dishAvailable));
            }
            else
            {
                ConsoleLogger.LogError($"Invalid dish ID ({ dishId }) from kitchen when setting availability");
            }
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
                int byteCount = 0;
                try
                {
                    byteCount = socket.Receive(buffer);
                }
                catch (Exception)
                {
                    ConsoleLogger.LogError($"Connection with { socket.RemoteEndPoint } was forcibly closed.");
                    break;
                }
                if (byteCount == 0)
                    break;

                string response = Encoding.UTF8.GetString(buffer, 0, byteCount);

                Regex loginPattern = new Regex(@"(LOGIN);(\p{L}+)");
                if (response == "DISCONNECT" || Regex.IsMatch("DISCONNECT;.*", response))
                {
                    ConsoleLogger.LogWarning($"User gave up while choosing username ({ socket.RemoteEndPoint })");
                    SocketUtility.CloseConnection(socket);
                    break;
                }
                else if (loginPattern.IsMatch(response))
                {
                    string username = loginPattern.Match(response).Groups[2].Value;
                    if (username == "kitchen")
                    {
                        if (Kitchen == null || !Kitchen.Socket.Connected)
                        {
                            Kitchen = new KitchenClient(socket);
                            ConsoleLogger.LogInformation($"Kitchen logged on from { socket.RemoteEndPoint }.");
                            socket.SendString("AUTHCONFIRMED", "You are now authenticated as the kitchen");
                            break;
                        }
                        else
                        {
                            socket.SendString("AUTHDENIED", "There is already a kitchen client connected");

                            ConsoleLogger.LogWarning($"Refused kitchen login attempt from { socket.RemoteEndPoint }");
                            SocketUtility.CloseConnection(socket);

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
                    ConsoleLogger.LogError($"Invalid format from { socket.RemoteEndPoint } while waiting for authentication:\n\t{ response }");
                }
            }
        }

        internal void StartServer()
        {
            new Task(() => Listen()).Start();
            new Task(() => Timer()).Start();
            ConsoleLogger.LogInformation($"Server started on { ((IPEndPoint)_socket.LocalEndPoint).Address }:{ ((IPEndPoint)_socket.LocalEndPoint).Port }. Press ESC to shutdown and quit.");

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

        private void Timer()
        {
            new Timer(RemoveOldOrders, new AutoResetEvent(false), 300000, 300000);
        }

        private void RemoveOldOrders(Object stateinfo)
        {
            foreach (var customer in CustomerConnections)
            {
                customer.Orders.RemoveAll(x => x.IsDone && (x.OrderPlaced - DateTime.Now > TimeSpan.FromHours(1)));
            }
            ConsoleLogger.LogInformation($"All unclaimed, finished orders that were placed before { DateTime.Now.AddHours(-1).ToUniversalTime() } have been removed.");
        }

        private void PrintSplash()
        {
            Console.WriteLine(@"   ___          __  ___       ___           __ ");
            Console.WriteLine(@"  / _ \___ ___ / /_/ _ |__ __/ _ \___ ___  / /_");
            Console.WriteLine(@" / , _/ -_|_-</ __/ __ / // / , _/ -_) _ \/ __/");
            Console.WriteLine(@"/_/|_|\__/___/\__/_/ |_\_,_/_/|_|\__/_//_/\__/ ");
            Console.WriteLine(@"  / __/__ _____  _____ ____  _  _<  // _ \     ");
            Console.WriteLine(@" _\ \/ -_) __/ |/ / -_) __/ | |/ / // // /     ");
            Console.WriteLine(@"/___/\__/_/  |___/\__/_/    |___/_(_)___/      ");
            Console.WriteLine("\n\n");
        }
    }
}
