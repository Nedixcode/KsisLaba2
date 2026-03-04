using System.Net;
using System.Net.Sockets;

namespace KsisLaba2
{
    public class DnsResolver
    {
        public string ResolveHostname(IPAddress addr, int timeoutMs)
        {
            try
            {
                var task = Task.Run(() => Dns.GetHostEntry(addr).HostName);
                return task.Wait(timeoutMs) ? task.Result : addr.ToString();
            }
            catch { return addr.ToString(); }
        }

        public IPAddress ResolveTarget(string targetHost)
        {
            if (IPAddress.TryParse(targetHost, out var address))
            {
                return address;
            }

            return Dns.GetHostEntry(targetHost).AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                ?? throw new Exception($"Нет IPv4 адреса для '{targetHost}'");
        }
    }
}