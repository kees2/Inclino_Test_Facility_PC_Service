using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ITF_PC_Service_Configurator
{
    static class UDPSend
    {
        static private Socket sock;
        static private IPAddress serverAddr;
        static private IPEndPoint endPoint;

        static private byte[] sendData;

        static public void initUDPSend()
        {
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            serverAddr = IPAddress.Parse("192.168.0.10");
            endPoint = new IPEndPoint(serverAddr, 52256);
        }

        static public void sendTemp(byte temperature)
        {
            byte[] sendData = new byte[1];
            sendData[0] = temperature;
            UDPSendData(sendData);
        }

        static private void UDPSendData(byte[] sendData)
        {
            sock.SendTo(sendData, endPoint);
        }

    }
}
