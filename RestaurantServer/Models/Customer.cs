using System.Collections.Generic;
using System.Net.Sockets;

namespace RestaurantServer.Models
{
    internal class Customer
    {
        internal Socket Socket { get; set; }
        internal string Username { get; set; }
        internal List<Order> Orders { get; set; } = new List<Order>();
    }
}
