using System;

namespace RestaurantCustomerConsole.Extensions
{
    internal static class ConsoleExtentions
    {
        internal static void Error(string message)
        {
            int cursorLeft = MoveCursor();
            Print(message, ConsoleColor.Red);
            ResetCursor(cursorLeft);
        }

        internal static void Warn(string message)
        {
            int cursorLeft = MoveCursor();
            Print(message, ConsoleColor.Yellow);
            ResetCursor(cursorLeft);
        }

        internal static void Success(string message)
        {
            int cursorLeft = MoveCursor();
            Print(message, ConsoleColor.Green);
            ResetCursor(cursorLeft);
        }
        internal static void Info(string message)
        {
            int cursorLeft = MoveCursor();
            Print(message, ConsoleColor.White);
            ResetCursor(cursorLeft);
        }

        private static int MoveCursor()
        {
            Console.WriteLine();
            int cursorLeft = Console.CursorLeft;
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            return cursorLeft;
        }

        private static void ResetCursor(int cursorLeft)
        {
            Console.SetCursorPosition(cursorLeft, Console.CursorTop + 1);
        }

        private static void Print(string message,ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
