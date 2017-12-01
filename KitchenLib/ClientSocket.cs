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
        private readonly ILogger _logger;

        public ClientSocket(string address, string port, ILogger logger, out bool success)
        {
            _logger = logger;
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

        private void ParseCommand(string cmd, string data)
        {
            if (ValidateJson(data))
            {
                switch (cmd)
                {
                    case "GETORDERS":
                        KitchenDb.Orders.AddRange(JsonConvert.DeserializeObject<List<Order>>(data));
                        break;
                    case "PLACEORDER":
                        KitchenDb.Orders.Add(JsonConvert.DeserializeObject<Order>(data));
                        break;
                }
            }
            else
            {
                switch (cmd)
                {
                    case "AUTHCONFIRMED":
                        _logger.LogInformation(data);
                        ClientSend("GETORDERS;{}");
                        break;
                    case "AUTHDENIED":
                        _logger.LogInformation(data);
                        break;
                    case "LOGIN":
                        ClientSend("LOGIN;" + "kitchen");
                        break;
                }
            }
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
