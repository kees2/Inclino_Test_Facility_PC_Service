using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace UDP_Test
{
    class UDPSend
    {
        private Socket sock;
        private IPAddress serverAddr;
        private IPEndPoint endPoint;

        private byte[] sendData;

        public void initUDPSend()
        {
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            serverAddr = IPAddress.Parse("192.168.0.10");
            endPoint = new IPEndPoint(serverAddr, 52256);
        }

        public void sendTemp(byte temperature)
        {
            byte[] sendData = new byte[1];
            sendData[0] = temperature;
            UDPSendData(sendData);
        }

        private void UDPSendData(byte[] sendData)
        {
            sock.SendTo(sendData, endPoint);
        }

    }
}
