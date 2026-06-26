using System.Net;

namespace PjuNetworkTester.Core.Scanning;

public interface INetworkProbe
{
    Task<ProbeResult> ProbeAsync(IPAddress address, TimeSpan timeout, CancellationToken cancellationToken);
}
