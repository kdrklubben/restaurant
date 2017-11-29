using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantServer.Utilities
{
    internal static class ConsoleLogger
    {
        internal static void LogInformation(string message)
        {
            Console.WriteLine($"Info: { message }");
        }

        internal static void LogWarning(string message)
        {
            Console.WriteLine($"Warning: { message }");
        }

        internal static void LogError(string message)
        {
            Console.WriteLine($"Error: { message }");
        }
    }
}
