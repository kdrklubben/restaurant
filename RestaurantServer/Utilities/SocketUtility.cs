using RestaurantServer.Models;
using RestaurantServer.Systems;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System;

namespace RestaurantServer.Utilities
{
    internal static class SocketUtility
    {
        internal static void CloseAllConnections()
        {
            if (ServerSystem.Instance.CustomerConnections.Count > 0)
            {
                foreach (Customer customer in ServerSystem.Instance.CustomerConnections)
                {
                    try
                    {
                        if (customer.Socket.Connected)
                            customer.Socket.Shutdown(SocketShutdown.Both);
                    }
                    finally
                    {
                        customer.Socket.Close();
                    }
                }
            }
        }

        internal static void CloseConnection(Socket socket)
        {
            try
            {
                if (socket.Connected)
                    socket.Shutdown(SocketShutdown.Both);
            }
            finally
            {
                socket.Close();
            }
        }

        internal static Socket CreateServerSocket()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080));
            return socket;
        }

        internal static void Send(Socket socket, string command, string message)
        {
            throw new NotImplementedException();
        }
    }
}
