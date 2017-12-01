﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KitchenLib;
using Newtonsoft.Json;

namespace RestaurantKitchenConsole
{
    public class Cli
    {
        private static bool _connectionSucceeded;
        private static ClientSocket _client;
        private static readonly CliLogger Logger = new CliLogger();
        public Cli()
        {
            Menu();
            Commands();
        }

        private static void Commands()
        {
            while (true)
            {
                Console.Write("\n\nEnter command: ");
                CmdParse(Console.ReadLine().ToLower());
            }
        }
        private static void CmdParse(string cmd)
        {
            switch (cmd)
            {
                case "1": Connect();
                    break;
                case "2": ShowOrders();
                    break; 
                case "3": MarkOrderDone();
                    break;
                case "menu": Menu();
                    break;
                case "clear": Console.Clear();
                    break;
                case "exit":
                case "x": Disconnect();
                    break;
                default: Menu();
                    break;
            }
        }

        private static void Connect()
        {
            PrintConsoleMessage(ConsoleColor.Cyan, "Kitchen application connecting to server...", null);
            _client = new ClientSocket("172.20.201.39", "8080", Logger, out _connectionSucceeded);
            //_client = new ClientSocket("127.0.0.1", "8080", _logger, out _connectionSucceeded);
            if (_connectionSucceeded)
                PrintConsoleMessage(ConsoleColor.Green, "Application is now connected to server.", null);
            else
                PrintConsoleMessage(ConsoleColor.Green, "Something went wrong try again later.", null);
        }

        private static void ShowOrders()
        {
            if (_connectionSucceeded && KitchenDb.GetOrders().Count > 0)
                KitchenDb.GetOrders().ForEach(o => Console.WriteLine($"Orderid: {o.OrderId} --- Dish ordered: {o.Dish.Name}"));
            else
                Console.WriteLine(_connectionSucceeded ? "There are currently no unfinnished orders." : "Must be connected to server.");
        }

        private static void MarkOrderDone()
        {
            if (_connectionSucceeded)
                try
                {
                    Console.Write("Enter order-id to mark order as finnished: ");
                    var orderId = Console.ReadLine();
                    var order = KitchenDb.GetOrders().SingleOrDefault(o => o.OrderId == int.Parse(orderId));
                    if (order != null)
                    {
                        KitchenDb.GetOrders().Remove(order);
                        _client.MarkOrderDone(order.OrderId);
                        Console.WriteLine($"Order {orderId} has now been completed");
                    }
                    else
                        Console.WriteLine("No order found by that id.");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            else
                Console.WriteLine("Must be connected to server.");
        }

        private static void Disconnect()
        {
            _client.DisconnectFromServer();
            PrintConsoleMessage(ConsoleColor.Green, "You successfully disconnected from server", null);
        }

        private static void Menu()
        {
            if (!_connectionSucceeded)
                Console.WriteLine("1. Connect to server");
            if (_connectionSucceeded)
            {
                Console.WriteLine("2. Show orders");
                Console.WriteLine("3. Mark order done");
                Console.WriteLine("Type exit to disconnect from server.");
            }
            Console.WriteLine("Type menu to view again.");
        }

        internal static void PrintConsoleMessage(ConsoleColor foreColor, string message, ConsoleColor? backColor)
        {
            Console.ForegroundColor = foreColor;
            if (backColor != null)
                Console.BackgroundColor = (ConsoleColor) backColor;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
