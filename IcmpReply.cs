using System.Net;

namespace KsisLaba2
{
    public class IcmpReply
    {
        public IPAddress? Address { get; set; }
        public long Rtt { get; set; }
    }
}