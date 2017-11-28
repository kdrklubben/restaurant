using System;
using System.Collections.Generic;
using System.Text;

namespace KitchenLib
{
    public static class KitchenDb
    {
        // Waiting for other feature branches to merge before Order can be used properly
        public static List<Order> Orders = new List<Order>();

        public static List<Order> GetOrders()
        {
            return Orders;
        }
    }
}
