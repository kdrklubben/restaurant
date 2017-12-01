using RestaurantCustomerLib;
using RestaurantLib;
using System;
using System.Collections.Generic;
using RestaurantCustomerLib.Delegates;
using RestaurantCustomerConsole.EventHandlers;

namespace RestaurantCustomerConsole
{
    internal class MenuService
    {
        public static SocketClient Client { get; private set; }
        internal static List<Dish> Menu { get; set; } = new List<Dish>();

        public MenuService()
        {
            Client = new SocketClient();            
            Client.Connect();
            ClaimName();

            // This path may seem long, but the rule is: The UI may bind to the lib, but the lib must not bind to the UI
            Client.Listener.GetDishes += new GetDishes(Handlers.HandleGetDishes);
            Client.Listener.AuthConfirmed += new AuthConfirmed(Handlers.HandleAuthConfirmed);
            Client.Listener.AuthDenied += new AuthDenied(Handlers.HandleAuthDenied);
            Client.Listener.OrderDone += new OrderDone(Handlers.HandleOrderDone);

            Client.GetDishes();
            MainLoop();
        }
        ~MenuService()
        {
            Client.Listener.GetDishes -= new GetDishes(Handlers.HandleGetDishes);
            Client.Listener.AuthConfirmed -= new AuthConfirmed(Handlers.HandleAuthConfirmed);
            Client.Listener.AuthDenied -= new AuthDenied(Handlers.HandleAuthDenied);
            Client.Listener.OrderDone -= new OrderDone(Handlers.HandleOrderDone);
        }

        void MainLoop()
        {
            string command = "";
            while (true)
            {
                Console.Write("> ");
                command = Console.ReadLine();
                //if (command == "connect") {
                //    Client.Connect();
                //    ClaimName();
                //}
                if (command == "menu") DisplayMenu();
                if (command == "exit") {
                    Client.Disconnect();
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
            //Console.WriteLine("Type 'connect' to connect to the server. Note that this should happen automatically before 'release'.");
            Console.WriteLine("Type 'menu' to view all menu options");
            Console.WriteLine("Type 'order [x]' to place an order. 'x' can be the dish's number or name");
            Console.WriteLine("Type 'exit' to close the application");
            Console.WriteLine("Type 'help' to see this list again");
        }

        void DisplayMenu()
        {
            foreach (Dish item in Menu)
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
            if (!int.TryParse(item, out int itemId))
            {
                itemId = Menu.Find(x => x.Name == item)?.DishId ?? 0;
            }
            if (itemId == 0)
            {
                Console.WriteLine($"Found no dish id or name matching '{item}'. Consider looking up the menu and try again.\nIf you used a name, try it's Id number instead.");
                return;
            }
            Client.Order(itemId);
        }
    }
}
