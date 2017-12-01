using System;
using System.Collections.Generic;
using RestaurantLib;

namespace RestaurantCustomerConsole.EventHandlers
{
    internal static class Handlers
    {
        public static void HandleServerUnavailable(){
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ERROR: The server is unavailable. Restarting connection process.");
            Console.ResetColor();
            MenuService.Client.Connect();
        }

        internal static void HandleLoginResponse(string message)
        {
            Console.WriteLine(message);
            MenuService.ClaimName();
        }
        internal static void HandleAuthConfirmed(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
            MenuService.Client.GetDishes();
        }
        internal static void HandleAuthDenied(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ERROR: " + message);
            Console.ResetColor();
            MenuService.ClaimName();
        }

        internal static void HandleGetDishes(List<Dish> data)
        {
            MenuService.Menu = data;
        }

        internal static void HandleOrderDone(string message)
        {
            Console.WriteLine(message);
        }

        internal static string HandlePromptIpAdress()
        {
            Console.WriteLine("Provide IP adress (defaults to 127.0.0.1)");
            return Console.ReadLine();
        }
    }
}