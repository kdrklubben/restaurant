using System;
using System.Collections.Generic;
using System.Text;

namespace KitchenLib
{
    public interface ILogger
    {
        void LogInformation(string message);
        void LogWarning(string message);

        void CloseConnection(bool isConnected);
    }
}
