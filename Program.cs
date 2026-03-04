using System.Net;

namespace KsisLaba2
{
    class Program
    {
        private static readonly ushort processId = (ushort)(DateTime.Now.Ticks & 0xFFFF);

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Использование: dotnet run -- [-dns] <цель>");
                return;
            }

            bool dnsMode = args[0].Equals("-dns", StringComparison.OrdinalIgnoreCase);
            string targetHost = args[dnsMode ? 1 : 0];

            try
            {
                var dnsResolver = new DnsResolver();
                var targetAddress = dnsResolver.ResolveTarget(targetHost);

                var traceroute = new Traceroute(dnsMode, targetAddress, processId);
                traceroute.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }
}