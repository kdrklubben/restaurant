using RestaurantCustomerLib;
using RestaurantCustomerLib.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantCustomerConsole
{
    internal class MenuService
    {
        internal static SocketClient Client { get; private set; }
        internal static List<Dish> Menu { get; private set; }
        private void Initialize()
        {
            Client = new SocketClient();
            Initialize();
            MainLoop();
        }

        void MainLoop()
        {
            string command = "";
            while (true)
            {
                if (command == "menu") DisplayMenu();
                if (command == "exit") break;
                if (command == "help") DisplayHelp();
                if (command.StartsWith("order")) PlaceOrder(command.Substring(' '));
            }
        }

        void DisplayHelp()
        {
            Console.WriteLine("Type 'menu' to view all menu options");
            Console.WriteLine("Type 'order [x]' to place an order. 'x' can be the dish's number or name");
            Console.WriteLine("Type 'exit' to close the application");
            Console.WriteLine("Type 'help' to see this list again");
        }

        void DisplayMenu()
        {
            List<Dish> menu = new List<Dish>(); // TODO make call to server, GET dishes
            foreach (Dish item in menu)
            {
                Console.WriteLine($"{item.DishId}\t{item.Name}");
            }
        }

        void ClaimName()
        {
            string name;
            do
            {
                Console.WriteLine("Type a unique name, so the kitchen can reply once your order is completed.\n(hint: your e-mail address is the most likely to be unique)");
                name = Console.ReadLine();
                // Ignoring Validate, but should probably guard against SQL injections
            } while (!Client.IsUniqueName(name));
            Console.WriteLine($"Welcome {name}");
        }

        void PlaceOrder(string item)
        {
            if (!int.TryParse(item, out int itemId))
            {
                // user inputted a dish name, locate its id
            }
            // send call to server
        }
    }
}
