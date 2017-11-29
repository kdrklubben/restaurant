using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantLib
{
    public class Order
    {
        public int OrderId { get; set; }
        public Dish Dish { get; set; }
        public bool IsDone { get; set; }
    }
}
