using Newtonsoft.Json;
using RestaurantLib.Extensions;
using System.Net.Sockets;

namespace RestaurantServer.Extensions
{
    internal static class ServerSocketExtensions
    {
        internal static void SendString(this Socket socket, string command, string message)
        {
            socket.Send($"{ command };{ JsonConvert.SerializeObject(message) }".ToUtf8ByteArray());
        }
    }
}
