using RestaurantServer.Models;
using System.Collections.Generic;
using System.Net.Sockets;

namespace RestaurantServer.Utilities
{
    internal static class SocketUtility
    {
        internal readonly static List<Customer> CustomerConnections = new List<Customer>();

        internal static void CloseAllConnections()
        {
            if (CustomerConnections.Count > 0)
            {
                foreach (Customer customer in CustomerConnections)
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

                CustomerConnections.RemoveRange(0, CustomerConnections.Count - 1);
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
            CustomerConnections.RemoveAll(x => x.Socket == socket);
        }
    }
}
