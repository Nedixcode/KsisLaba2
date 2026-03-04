using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace KsisLaba2
{
    public class IcmpPacketReceiver
    {
        private readonly Socket _receiveSocket;
        private readonly ushort _processId;

        public IcmpPacketReceiver(Socket receiveSocket, ushort processId)
        {
            _receiveSocket = receiveSocket;
            _processId = processId;
        }

        public IcmpReply? ReceivePacket(Stopwatch sw, int expectedSeq)
        {
            try
            {
                byte[] buffer = new byte[4096];
                EndPoint ep = new IPEndPoint(IPAddress.Any, 0);
                int received = _receiveSocket.ReceiveFrom(buffer, ref ep);

                if (received < 20) return null;

                int ipHeaderLen = (buffer[0] & 0x0F) * 4;
                if (buffer[9] != 1) return null;

                var sourceAddr = new IPAddress(buffer.Skip(12).Take(4).ToArray());
                int icmpOffset = ipHeaderLen;

                if (icmpOffset + 4 > received) return null;

                byte type = buffer[icmpOffset];

                bool CheckNestedPacket(int offset, out int nestedId, out int nestedSeq)
                {
                    nestedId = nestedSeq = 0;
                    int nestedIpOffset = offset + 8;
                    if (nestedIpOffset + 20 > received) return false;

                    int nestedIpHeaderLen = (buffer[nestedIpOffset] & 0x0F) * 4;
                    int nestedIcmpOffset = nestedIpOffset + nestedIpHeaderLen;

                    if (nestedIcmpOffset + 8 > received || buffer[nestedIcmpOffset] != 8)
                        return false;

                    nestedId = (buffer[nestedIcmpOffset + 4] << 8) | buffer[nestedIcmpOffset + 5];
                    nestedSeq = (buffer[nestedIcmpOffset + 6] << 8) | buffer[nestedIcmpOffset + 7];
                    return true;
                }

                if (type == 0)
                {
                    if (icmpOffset + 8 > received) return null;
                    int id = (buffer[icmpOffset + 4] << 8) | buffer[icmpOffset + 5];
                    int seq = (buffer[icmpOffset + 6] << 8) | buffer[icmpOffset + 7];

                    return (id == _processId && seq == expectedSeq)
                        ? new IcmpReply { Address = sourceAddr, Rtt = sw.ElapsedMilliseconds }
                        : null;
                }

                if (type == 11 || type == 3)
                {
                    if (CheckNestedPacket(icmpOffset, out int nestedId, out int nestedSeq) &&
                        nestedId == _processId && nestedSeq == expectedSeq)
                    {
                        return new IcmpReply { Address = sourceAddr, Rtt = sw.ElapsedMilliseconds };
                    }
                }
            }
            catch {}

            return null;
        }
    }
}