using System;
using System.Collections.Generic;
using RestaurantLib;

namespace RestaurantCustomerConsole.EventHandlers
{
    internal static class Handlers
    {
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
            Console.WriteLine(message);
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
    }
}