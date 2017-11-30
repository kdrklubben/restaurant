using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantLib.Extensions
{
    public static class SocketExtensions
    {
        public static byte[] ToUtf8ByteArray(this string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }
    }
}
