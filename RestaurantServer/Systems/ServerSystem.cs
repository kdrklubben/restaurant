using RestaurantLib;
using RestaurantServer.Models;
using RestaurantServer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestaurantServer.Systems
{
    internal sealed class ServerSystem
    {
        private static readonly ServerSystem _instance = new ServerSystem();
        internal readonly List<Dish> Dishes;
        internal readonly List<Customer> CustomerConnections;

        static ServerSystem()
        { }

        private ServerSystem()
        {
            Dishes = SerializationUtility.ReadDishes();
            CustomerConnections = new List<Customer>();
        }

        internal static ServerSystem Instance
        {
            get { return _instance; }
        }

        internal void PlaceOrder(int dishId, Customer customer)
        {
            Dish dish = Dishes.SingleOrDefault(x => x.DishId == dishId);
            if (dish != null)
            {
                customer.Orders.Add(new Order() { Dish = dish, IsDone = false });
            }
        }

        internal void Listen()
        {

        }
    }
}
