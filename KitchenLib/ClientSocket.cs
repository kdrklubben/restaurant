using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestaurantLib;

namespace KitchenLib
{
    public class ClientSocket
    {
        private readonly TcpClient _client = new TcpClient();
        private NetworkStream _stream;
        private readonly IPEndPoint _endPoint;

        public ClientSocket(string address, string port, out bool success)
        {
            _endPoint = RemoteEndPoint(address, port);
            success = Connect();
        }

        private bool Connect()
        {
            try
            {
                _client.Connect(_endPoint);
                _stream = _client.GetStream();
                new Task(Listen).Start();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private IPEndPoint RemoteEndPoint(string address, string port)
        {
            return new IPEndPoint(IPAddress.Parse(address), Int32.Parse(port));
        }

        public void ClientSend(string data)
        {
            var bytesToSend = Encoding.ASCII.GetBytes(data);
            _stream.Write(bytesToSend, 0, bytesToSend.Length);
        }

        public void Listen()
        {
            while (true)
            {
                var data = new byte[1024];
                int recv = _stream.Read(data, 0, data.Length);
                var receivedData = Encoding.ASCII.GetString(data, 0, recv).Split(";");
                ParseCommand(receivedData[0], receivedData[1]);
            }
        }

        private void ParseCommand(string cmd, string json)
        {
            if (!ValidateJson(json)) return;

            switch (cmd)
            {
                case "GETORDERS":
                    KitchenDb.Orders.AddRange(JsonConvert.DeserializeObject<List<Order>>(json));
                    break;
                case "PLACEORDER": KitchenDb.Orders.Add(JsonConvert.DeserializeObject<Order>(json));
                    break;
                case "AUTHCONFIRMED":
                    ShowInfoMessage(JsonConvert.DeserializeObject<string>(json));
                    ClientSend("GETORDERS;{}");
                    break;
                case "AUTHDENIED":
                    ShowInfoMessage(JsonConvert.DeserializeObject<string>(json));
                    break;
                case "LOGIN":
                    ClientSend("LOGIN;" + "kitchen");
                    break;
            }
        }

        private void ShowInfoMessage(string message)
        {
            var cursorLeft = Console.CursorLeft;
            Console.SetCursorPosition(0, Console.CursorTop - 2);
            Console.WriteLine(message);
            Console.SetCursorPosition(cursorLeft, Console.CursorTop + 1);
        }
        
        private bool ValidateJson(string json)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<dynamic>(json);
                return true;
            }
            catch // not valid
            {
                return false;
            }
        }
    }
}
