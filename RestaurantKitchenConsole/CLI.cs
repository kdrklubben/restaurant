using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KitchenLib;
using Newtonsoft.Json;

namespace RestaurantKitchenConsole
{
    public class Cli
    {
        private static bool _connectionSucceeded;
        private static ClientSocket _client;
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
                case "menu": Menu();
                    break;
                case "clear": Console.Clear();
                    break;
                default: Menu();
                    break;
            }
        }

        private static void Connect()
        {
            Console.WriteLine("Kitchen application connecting to server...");
            _client = new ClientSocket("127.0.0.1", "8080", out _connectionSucceeded);
            Console.WriteLine(_connectionSucceeded
                ? "Application is now connected to server."
                : "Something went wrong try again later.");
        }

        public static void ShowOrders()
        {
            if (_connectionSucceeded && KitchenDb.GetOrders().Count > 0)
                KitchenDb.GetOrders().ForEach(o => Console.WriteLine($"Orderid: {o.OrderId} --- Dish ordered: {o.Dish.Name}"));
            else
                Console.WriteLine(_connectionSucceeded ? "There are currently no unfinnished orders." : "Must be connected to server.");
        }

        private static void MarkOrderDone()
        {
            if (_connectionSucceeded)
                try
                {
                    Console.Write("Enter order-id to mark order as finnished: ");
                    var orderId = Console.ReadLine();
                    var order = KitchenDb.GetOrders().SingleOrDefault(o => o.OrderId == int.Parse(orderId));
                    if (order != null)
                    {
                        KitchenDb.GetOrders().Remove(order);
                        _client.ClientSend("ORDERDONE;" + JsonConvert.SerializeObject(order.OrderId));
                        Console.WriteLine($"Order {orderId} has now been completed");
                    }
                    else
                        Console.WriteLine("No order found by that id.");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            else
                Console.WriteLine("Must be connected to server.");
        }


        private static void Menu()
        {
            if (!_connectionSucceeded)
                Console.WriteLine("1. Connect to server");
            if (_connectionSucceeded)
            {
                Console.WriteLine("2. Show orders");
                Console.WriteLine("3. Mark order done");
            }
            Console.WriteLine("Type menu to view again.");
        }
    }
}
