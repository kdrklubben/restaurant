using System;
using System.Collections.Generic;
using RestaurantLib;
using RestaurantCustomerConsole.Extensions;

namespace RestaurantCustomerConsole.EventHandlers
{
    internal static class Handlers
    {
        public static void HandleServerUnavailable(){
            ConsoleExtentions.Error("ERROR: The server is unavailable. Restarting connection process.");
            MenuService.Client.Connect();
        }

        internal static void HandleLoginResponse(string message)
        {
            ConsoleExtentions.Info(message);
            MenuService.ClaimName();
        }
        internal static void HandleAuthConfirmed(string message)
        {
            ConsoleExtentions.Success(message);
            MenuService.Client.GetDishes();
            MenuService.Run = true;
        }
        internal static void HandleAuthDenied(string message)
        {
            ConsoleExtentions.Error("ERROR: " + message);
            MenuService.ClaimName();
        }

        internal static void HandleGetOrders(List<Order> orders) => MenuService.Orders = orders;

        internal static void HandleGetDishes(List<Dish> data) => MenuService.Menu = data;

        internal static void HandleOrderDone(string message)
        {
            int id = int.Parse(message);
            Dish dish = MenuService.Orders.Find(x => x.OrderId == id).Dish;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Your ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(dish.Name);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(" order has been completed");
            Console.ResetColor();
        }

        internal static string HandlePromptIpAdress()
        {
            Console.WriteLine("Provide IP adress (defaults to 127.0.0.1)");
            Console.Write("> ");
            return Console.ReadLine();
        }

        internal static void HandleOrderPlaced(int dishId)
        {
            Dish dish = MenuService.Menu.Find(x => x.DishId == dishId);

            Console.Write("Your ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(dish.Name);
            Console.ResetColor();
            Console.Write(" order has been sent to the kitchen");            
        }
        internal static void HandleSetAvailable(DishAvailableModel model)
            => MenuService.Menu.Find(x => x.DishId == model.DishId).IsAvailable = model.IsAvailable;
    }
}