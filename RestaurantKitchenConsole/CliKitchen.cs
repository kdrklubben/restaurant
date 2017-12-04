using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using KitchenLib;
using Newtonsoft.Json;

namespace RestaurantKitchenConsole
{
    public class Cli
    {
        public static bool _connectionSucceeded;
        private static ClientSocket _client;
        private static readonly CliLogger Logger = new CliLogger();
        public Cli()
        {
            Menu();
            Commands();
        }

        private static void Commands()
        {
            while (true)
            {
                Console.Write("\n\nEnter command: ");
                CmdParse(Console.ReadLine().ToLower());
            }
        }

        private static void CmdParse(string cmd)
        {
            switch (cmd)
            {
                case "1": Connect();
                    break;
                case "2": ShowOrders();
                    break; 
                case "3": MarkOrderDone();
                    break;
                case "4": ToggleOrderAvailability();
                    break;
                case "menu": Menu();
                    break;
                case "clear": Console.Clear();
                    break;
                case "exit":
                case "x": Disconnect();
                    break;
                default: Menu();
                    break;
            }
        }

        private static void Connect()
        {
            PrintConsoleMessage(ConsoleColor.Cyan, "Kitchen application connecting to server...", null);
            _client = new ClientSocket("127.0.0.1", "8080", Logger, out _connectionSucceeded);
            if (_connectionSucceeded)
                PrintConsoleMessage(ConsoleColor.Green, "Application is now connected to server.", null);
            else
                PrintConsoleMessage(ConsoleColor.Green, "Something went wrong try again later.", null);
        }

        private static void ShowOrders()
        {
            ClientSocket.NewOrderCounter = 0;
            if (_connectionSucceeded && KitchenDb.GetOrders().Count > 0)
            KitchenDb.GetOrders().ForEach(o => PrintConsoleMessage(ConsoleColor.Black, $"Orderid: {o.OrderId} --- Dish ordered: {o.Dish.Name}", ConsoleColor.White));
            else
                Console.WriteLine(_connectionSucceeded ? "There are currently no unfinished orders." : "Must be connected to server.");
        }

        private static void MarkOrderDone()
        {
            if (_connectionSucceeded)
                try
                {
                    Console.Write("Enter order-id to mark order as finished: ");
                    var orderId = Console.ReadLine();
                    var order = KitchenDb.GetOrders().SingleOrDefault(o => o.OrderId == int.Parse(orderId));
                    if (order != null)
                    {
                        KitchenDb.GetOrders().Remove(order);
                        _client.MarkOrderDone(order.OrderId);
                        PrintConsoleMessage(ConsoleColor.Green, $"Order {orderId} has now been completed", null);
                    }
                    else
                        PrintConsoleMessage(ConsoleColor.White, "Order-id does not exists.", null);
                }
                catch (Exception)
                {
                    PrintConsoleMessage(ConsoleColor.Yellow, "That is not a valid order-id", null);
                }
            else
                PrintConsoleMessage(ConsoleColor.Red, "Must be connected to server.", null);
        }

        private static void ToggleOrderAvailability()
        {
            if (_connectionSucceeded)
                try
                {
                    Console.Write("Choose which to toggle availability");
                    var dishId = Console.ReadLine();
                    var dish = KitchenDb.GetDishes().SingleOrDefault(o => o.DishId == int.Parse(dishId));
                    if (dish != null)
                    {
                        if (dish.IsAvailable)
                            PrintConsoleMessage(ConsoleColor.Green, $"{dish.Name} is currently set to available", null);
                        else
                            PrintConsoleMessage(ConsoleColor.Red, $"{dish.Name} is currently set to unavailable", null);

                        while (true)
                        {
                            Console.WriteLine("1. To set dish available.");
                            Console.Write("2. To set dish unavailable.");
                            var setAvailabilityStatus = Console.ReadLine();
                            if (setAvailabilityStatus == "x") break;
                            if (setAvailabilityStatus == "1" || setAvailabilityStatus == "2")
                            {
                                //_client.();
                            }
                            else
                                PrintConsoleMessage(ConsoleColor.Yellow, "Not a valid command try again or type x to quit.", null);
                        }
                    }
                    else
                        PrintConsoleMessage(ConsoleColor.White, "Dish-id does not exists.", null);
                }
                catch (Exception)
                {
                    PrintConsoleMessage(ConsoleColor.Yellow, "That is not a valid dish-id", null);
                }
            else
                PrintConsoleMessage(ConsoleColor.Red, "Must be connected to server.", null);
        }

        private static void Disconnect()
        {
            _client.DisconnectFromServer();
            _connectionSucceeded = false;
            PrintConsoleMessage(ConsoleColor.Green, "You successfully disconnected from server", null);
        }

        private static void Menu()
        {
            if (!_connectionSucceeded)
                Console.WriteLine("1. Connect to server");
            if (_connectionSucceeded)
            {
                Console.WriteLine("2. Show orders");
                Console.WriteLine("3. Mark order done");
                Console.WriteLine("4. Toggle dish availability");
                Console.WriteLine("Type exit to disconnect from server.");
            }
            Console.WriteLine("Type menu to view again.");
        }

        internal static void PrintConsoleMessage(ConsoleColor foreColor, string message, ConsoleColor? backColor)
        {
            Console.ForegroundColor = foreColor;
            if (backColor != null)
                Console.BackgroundColor = (ConsoleColor) backColor;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
