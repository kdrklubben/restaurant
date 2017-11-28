using RestaurantServer.Systems;

namespace RestaurantServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerSystem server = ServerSystem.Instance;
            server.Listen();
        }
    }
}
