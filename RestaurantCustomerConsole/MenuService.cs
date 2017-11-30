using RestaurantCustomerLib;
using RestaurantLib;
using System;
using System.Collections.Generic;

namespace RestaurantCustomerConsole
{
    internal class MenuService
    {
        public static SocketClient Client { get; private set; }
        internal static List<Dish> Menu { get; private set; }

        public MenuService()
        {
            Client = new SocketClient();
            MainLoop();
        }

        void MainLoop()
        {
            string command = "";
            while (true)
            {
                Console.Write("> ");
                command = Console.ReadLine();
                if (command == "connect") {
                    Client.Connect();
                    ClaimName();
                }                
                if (command == "menu") DisplayMenu();
                if (command == "exit") {
                    //Client.Disconnect();
                    break;
                }                
                if (command == "help") DisplayHelp();
                if (command.StartsWith("order")) {
                    string item = command.Substring(command.IndexOf(' '));
                    PlaceOrder(item);
                }
            }
        }

        void DisplayHelp()
        {
            Console.WriteLine("Type 'connect' to connect to the server. Note that this should happen automatically before 'release'.");
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
                Console.WriteLine($"{item.DishId}\t{item.Name}\t{item.Price} SEK\n{item.Description}");
            }
        }

        internal static void ClaimName()
        {
            string name;
            bool IsValid = true;
            do
            {
                Console.WriteLine("Type a unique name, so the kitchen can reply once your order is completed.\n(hint: your e-mail address is the most likely to be unique)");
                name = Console.ReadLine();
                if (name == "kitchen") IsValid = false;
            } while (!IsValid);
            Client.Login(name);
        }

        void PlaceOrder(string item)
        {
            int itemId = 0;
            if (!int.TryParse(item, out itemId))
            {
                // user inputted a dish name, locate its id
            }
            // send call to server
            
            Client.Order(itemId);
        }
    }
}
