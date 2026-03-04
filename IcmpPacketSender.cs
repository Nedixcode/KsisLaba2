using System.Net;
using System.Net.Sockets;

namespace KsisLaba2
{
    public class IcmpPacketSender
    {
        private readonly Socket _sendSocket;
        private readonly IPAddress _targetAddress;
        private readonly ushort _processId;

        public IcmpPacketSender(Socket sendSocket, IPAddress targetAddress, ushort processId)
        {
            _sendSocket = sendSocket;
            _targetAddress = targetAddress;
            _processId = processId;
        }

        public void SendPacket(int ttl, int seq)
        {
            try
            {
                _sendSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, ttl);

                byte[] packet = new byte[40]; 
                packet[0] = 8; 

                packet[4] = (byte)(_processId >> 8);
                packet[5] = (byte)_processId;
                packet[6] = (byte)(seq >> 8);
                packet[7] = (byte)seq;

                for (int i = 8; i < packet.Length; i++)
                    packet[i] = (byte)"abcdefghijklmnopqrstuvwxyz0123456789"[(i - 8) % 36];

                ushort checksum = CalculateChecksum(packet);
                packet[2] = (byte)(checksum >> 8);
                packet[3] = (byte)checksum;

                _sendSocket.SendTo(packet, new IPEndPoint(_targetAddress!, 0));
            }
            catch {}
        }

        private ushort CalculateChecksum(byte[] data)
        {
            long sum = 0;
            for (int i = 0; i < data.Length; i += 2)
            {
                sum += (i + 1 < data.Length)
                    ? (data[i] << 8) | data[i + 1]
                    : data[i] << 8;
            }

            while ((sum >> 16) != 0)
                sum = (sum & 0xFFFF) + (sum >> 16);

            return (ushort)~sum;
        }
    }
}