﻿using System;
using System.Collections.Generic;
using System.Text;
using RestaurantLib;

namespace KitchenLib
{
    public static class KitchenDb
    {
        // Waiting for other feature branches to merge before Order can be used properly
        public static List<Order> Orders = new List<Order>();
        public static List<Dish> Dishes = new List<Dish>();
        public static Dictionary<bool, string> StatusDict = new Dictionary<bool, string> { { true, "available" }, { false, "unavailable" } };

        public static List<Order> GetOrders()
        {
            return Orders;
        }
        public static List<Dish> GetDishes()
        {
            return Dishes;
        }
    }
}
