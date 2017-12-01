using System;
using System.Collections.Generic;
using System.Text;
using KitchenLib;

namespace RestaurantKitchenConsole
{
    internal class CliLogger : ILogger
    {
        public void LogInformation(string message)
        {
            var cursorLeft = Console.CursorLeft;
            Console.SetCursorPosition(0, Console.CursorTop - 2);
            Cli.PrintConsoleMessage(ConsoleColor.White, message, null);
            Console.SetCursorPosition(cursorLeft, Console.CursorTop + 1);
        }

        public void LogWarning(string message)
        {
            var cursorLeft = Console.CursorLeft;
            Console.SetCursorPosition(0, Console.CursorTop - 2);
            Cli.PrintConsoleMessage(ConsoleColor.Red, message, null);
            Console.SetCursorPosition(cursorLeft, Console.CursorTop + 1);
        }

        public void CloseConnection(bool isConnected)
        {
            Cli._connectionSucceeded = isConnected;
        }

        public void LogNewOrder(string message)
        {
            var cursorLeft = Console.CursorLeft;
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Cli.PrintConsoleMessage(ConsoleColor.White, message, null);
            Console.SetCursorPosition(cursorLeft, Console.CursorTop);
        }
    }
}
