using System;

namespace RestaurantCustomerConsole.EventHandlers
{
    internal static class Handlers
    {
        // TODO: Consider some user assistance, so info being written is not causing faulty commands, yet half-made commands are not removed
        internal static void HandleAuthConfirmed(string message)
        {
            Console.WriteLine(message);
        }
        internal static void HandleAuthDenied(string message)
        {
            Console.WriteLine(message);
            MenuService.ClaimName();
        }
        internal static void HandleOrderDone(string message)
        {
            Console.WriteLine(message);
        }
    }
}
