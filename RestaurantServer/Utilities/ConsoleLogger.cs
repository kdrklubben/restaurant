using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantServer.Utilities
{
    internal static class ConsoleLogger
    {
        internal static void LogInformation(string message)
        {
            Log(ConsoleColor.Gray, "Info", message);
        }

        internal static void LogWarning(string message)
        {
            Log(ConsoleColor.Yellow, "Warning", message);
        }

        internal static void LogError(string message)
        {
            Log(ConsoleColor.Red, "Error", message);
        }

        private static void Log(ConsoleColor color, string prefix, string message)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"{ DateTime.Now.ToUniversalTime().ToString("HH:mm:ss") } { prefix }: { message }");
            Console.ResetColor();
        }
    }
}
