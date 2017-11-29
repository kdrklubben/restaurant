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
                Console.Write("Enter command: ");
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
                default: Menu();
                    break;
            }
        }

        private static void Connect()
        {
            Console.WriteLine("Kitchen application connecting to server...");
            _client = new ClientSocket("127.0.0.1", "8081", out _connectionSucceeded);
            Console.WriteLine(_connectionSucceeded
                ? "Application is now connected to server."
                : "Something went wrong try again later.");
            if (_connectionSucceeded)
                Menu();
        }

        public static void ShowOrders()
        {
            KitchenDb.GetOrders().ForEach(o => Console.WriteLine($"Orderid: {o.OrderId} --- Dish ordered: {o.Dish.Name}"));
        }

        private static void MarkOrderDone()
        {
            try
            {
                var order = KitchenDb.GetOrders().SingleOrDefault(o => o.OrderId == int.Parse(Console.ReadLine()));
                if (order != null)
                {
                    KitchenDb.GetOrders().Remove(order);
                    _client.ClientSend("ORDERDONE;" + JsonConvert.SerializeObject(order.OrderId));
                }
                    
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        private static void Menu()
        {
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
