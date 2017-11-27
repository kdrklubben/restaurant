using RestaurantLib;
using RestaurantServer.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantServer.Systems
{
    internal class ServerSystem
    {
        private readonly List<Dish> _dishes;

        public ServerSystem()
        {
            _dishes = SerializationUtility.ReadDishes();
        }
    }
}
