using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace KsisLaba2
{
    public class Traceroute
    {
        private const int MaxHops = 30;
        private const int Timeout = 3000;
        private const int PacketsPerHop = 3;

        private readonly bool _dnsMode;
        private readonly IPAddress _targetAddress;
        private readonly ushort _processId;
        private readonly DnsResolver _dnsResolver;
        private readonly IcmpPacketSender _packetSender;
        private readonly IcmpPacketReceiver _packetReceiver;

        private int _globalSeq;

        public Traceroute(bool dnsMode, IPAddress targetAddress, ushort processId)
        {
            _dnsMode = dnsMode;
            _targetAddress = targetAddress;
            _processId = processId;
            _dnsResolver = new DnsResolver();

            var sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
            var receiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
            receiveSocket.Bind(new IPEndPoint(IPAddress.Any, 0));
            receiveSocket.ReceiveTimeout = Timeout;

            _packetSender = new IcmpPacketSender(sendSocket, targetAddress, processId);
            _packetReceiver = new IcmpPacketReceiver(receiveSocket, processId);
        }

        public void Run()
        {
            string targetName = _dnsMode ? _dnsResolver.ResolveHostname(_targetAddress, 500) : _targetAddress.ToString();
            Console.WriteLine($"Трассировка к {targetName} [{_targetAddress}]\n");
            Console.WriteLine("       1       2       3     Адрес узла");
            Console.WriteLine("-------------------------------------------");

            bool targetReached = false;

            for (int ttl = 1; ttl <= MaxHops && !targetReached; ttl++)
            {
                Console.Write($"{ttl,2} ");
                var hopAddresses = new HashSet<IPAddress>();
                var rtts = new List<long>();

                for (int i = 0; i < PacketsPerHop; i++)
                {
                    int currentSeq = _globalSeq++;
                    var sw = Stopwatch.StartNew();

                    _packetSender.SendPacket(ttl, currentSeq);

                    var reply = _packetReceiver.ReceivePacket(sw, currentSeq);
                    sw.Stop();

                    if (reply?.Address != null)
                    {
                        hopAddresses.Add(reply.Address);
                        rtts.Add(reply.Rtt);
                        Console.Write($"{reply.Rtt,4}ms  ");

                        if (reply.Address.Equals(_targetAddress))
                        {
                            targetReached = true;
                        }
                    }
                    else
                    {
                        Console.Write("    *   ");
                    }
                }

                if (hopAddresses.Count > 0)
                {
                    var addr = hopAddresses.Last();
                    string name = _dnsMode ? _dnsResolver.ResolveHostname(addr, 500) : addr.ToString();

                    if (name != addr.ToString())
                        Console.Write($"  {name} [{addr}]");
                    else
                        Console.Write($"  {addr}");
                }
                else
                {
                    Console.Write("  *");
                }

                Console.WriteLine();
            }

            Console.WriteLine($"\nТрассировка завершена.");
        }
    }
}