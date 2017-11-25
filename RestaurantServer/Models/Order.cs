using RestaurantLib;

namespace RestaurantServer.Models
{
    internal class Order
    {
        internal Dish Dish { get; set; }
        internal bool IsDone { get; set; }
    }
}
