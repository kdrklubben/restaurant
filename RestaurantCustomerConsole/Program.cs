using RestaurantCustomerConsole.EventHandlers;
using RestaurantCustomerLib.Delegates;

namespace RestaurantCustomerConsole
{
    class Program
    {
        
        static void Main(string[] args)
        {
            MenuService menu = new MenuService();
            // This path may seem long, but the rule is: The UI may bind to the lib, but the lib must not bind to the UI
            menu.Client.Listener.AuthConfirmed += new AuthConfirmed(Handlers.HandleAuthConfirmed);
            menu.Client.Listener.AuthDenied += new AuthDenied(Handlers.HandleAuthDenied);
            menu.Client.Listener.OrderDone += new OrderDone(Handlers.HandleOrderDone);
        }        
    }
}
