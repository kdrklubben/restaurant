using RestaurantServer.Systems;
using System;

namespace RestaurantServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerSystem.Instance.StartServer();
        }
    }
}
