using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;

namespace PjuNetworkTester.Core.Networking;

public interface IArpTableReader
{
    Task<IReadOnlyDictionary<IPAddress, string>> ReadAsync(CancellationToken cancellationToken);
}

public sealed class ArpTableReader : IArpTableReader
{
    public async Task<IReadOnlyDictionary<IPAddress, string>> ReadAsync(CancellationToken cancellationToken)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var output = await RunCommandAsync("arp", "-a", cancellationToken);
            return ArpTableParser.ParseWindowsArpOutput(output);
        }

        var ipNeighborOutput = await TryRunCommandAsync("ip", "neighbor", cancellationToken);
        if (!string.IsNullOrWhiteSpace(ipNeighborOutput))
        {
            return ArpTableParser.ParseLinuxIpNeighborOutput(ipNeighborOutput);
        }

        var arpOutput = await TryRunCommandAsync("arp", "-a", cancellationToken);
        return string.IsNullOrWhiteSpace(arpOutput)
            ? new Dictionary<IPAddress, string>()
            : ArpTableParser.ParseWindowsArpOutput(arpOutput);
    }

    private static async Task<string> TryRunCommandAsync(string fileName, string arguments, CancellationToken cancellationToken)
    {
        try
        {
            return await RunCommandAsync(fileName, arguments, cancellationToken);
        }
        catch
        {
            return string.Empty;
        }
    }

    private static async Task<string> RunCommandAsync(string fileName, string arguments, CancellationToken cancellationToken)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        process.Start();
        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        return await outputTask;
    }
}
