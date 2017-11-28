using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
                ClientSend("GETORDERS");
                ClientRecieveIncomingOrders();
                new Task(ClientRecieveIncomingOrders).Start();
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
            var bytesToSend = Encoding.ASCII.GetBytes(data.ToCharArray(), 0, data.Length);
            _stream.Write(bytesToSend, 0, bytesToSend.Length);
            _stream.Flush();
        }

        public void ClientRecieveIncomingOrders()
        {
            while (true)
            {
                var data = new byte[1024];
                int recv = _stream.Read(data, 0, data.Length);
                var order = Encoding.ASCII.GetString(data, 0, recv);
                if (ValidateJson(order))
                    KitchenDb.Orders.Add(JsonConvert.DeserializeObject<Order>(order));

            }
        }

        public void ClientRecieveOrdersOnce()
        {
            var data = new byte[1024];
            int recv = _stream.Read(data, 0, data.Length);
            var orders = Encoding.ASCII.GetString(data, 0, recv);
            if (ValidateJson(orders))
                KitchenDb.Orders = JsonConvert.DeserializeObject<List<Order>>(orders);
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
