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
        internal static List<Order> Orders { get; set; } = new List<Order>();
        public MenuService()
        {
            Client = new SocketClient();            
            Client.Connect();

            // This path may seem long, but the rule is: The UI may bind to the lib, but the lib must not bind to the UI
            Client.Listener.LoginResponse += new LoginResponse(Handlers.HandleLoginResponse);
            Client.Listener.GetDishes += new GetDishes(Handlers.HandleGetDishes);
            Client.Listener.AuthConfirmed += new AuthConfirmed(Handlers.HandleAuthConfirmed);
            Client.Listener.AuthDenied += new AuthDenied(Handlers.HandleAuthDenied);
            Client.Listener.OrderDone += new OrderDone(Handlers.HandleOrderDone);
            Client.ServerUnavailableError += new ServerUnavailableError(Handlers.HandleServerUnavailable);
            Client.PromptIpAddress += new PromptIpAddress(Handlers.HandlePromptIpAdress);
            MainLoop();
        }

        void MainLoop()
        {
            string command = "";
            while (true)
            {
                Console.Write("> ");
                command = Console.ReadLine().ToLower();
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
            Console.WriteLine("Type 'menu' to view all menu options");
            Console.WriteLine("Type 'order [x]' to place an order. 'x' can be the dish's number or name");
            Console.WriteLine("Type 'exit' to close the application");
            Console.WriteLine("Type 'help' to see this list again");
        }

        void DisplayMenu()
        {
            foreach (Dish item in Menu)
            {
                Console.WriteLine($"{item.DishId}\t{item.Name}\t{item.Price} SEK\n\t{item.Description}");
            }
        }

        internal static void ClaimName()
        {
            string name;
            bool IsValid = true;
            do
            {
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
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: Found no dish id or name matching '{item}'. Consider looking up the menu and try again.\nIf you used a name, try it's Id number instead.");
                Console.ResetColor();
                return;
            }
            else if (itemId > Menu.Count)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: Provided id is too high. Consider looking up the menu and try again.\nIf you used a name, try it's Id number instead.");
                Console.ResetColor();
                return;
            }

            Client.Order(itemId);
        }
    }
}
