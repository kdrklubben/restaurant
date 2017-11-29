using System;
using System.Collections.Generic;
using System.Text;
using KitchenLib;

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
                case "help": Menu();
                    break;
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
            Console.WriteLine("help");
        }

        private static void Connect()
        {
            Console.WriteLine("Kitchen application connecting to server...");
            //_client = new ClientSocket("127.0.0.1", "8081", out _connectionSucceeded);
            Console.WriteLine(_connectionSucceeded
                ? "Application is now connected to server."
                : "Something went wrong try again later.");
        }
    }
}
