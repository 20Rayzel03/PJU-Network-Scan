using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace PjuNetworkTester.Core.Scanning;

public sealed class PingNetworkProbe : INetworkProbe
{
    public async Task<ProbeResult> ProbeAsync(IPAddress address, TimeSpan timeout, CancellationToken cancellationToken)
    {
        using var ping = new Ping();

        try
        {
            var reply = await ping.SendPingAsync(address, (int)Math.Clamp(timeout.TotalMilliseconds, 1, int.MaxValue))
                .WaitAsync(cancellationToken);
            if (reply.Status != IPStatus.Success)
            {
                return ProbeResult.Offline();
            }

            var hostname = await TryReverseDnsAsync(address, cancellationToken);
            return ProbeResult.Online(reply.RoundtripTime, hostname);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return ProbeResult.Offline();
        }
    }

    private static async Task<string?> TryReverseDnsAsync(IPAddress address, CancellationToken cancellationToken)
    {
        try
        {
            var entry = await Dns.GetHostEntryAsync(address)
                .WaitAsync(cancellationToken);
            return string.IsNullOrWhiteSpace(entry.HostName) ? null : entry.HostName;
        }
        catch (SocketException)
        {
            return null;
        }
        catch
        {
            return null;
        }
    }
}
