namespace PjuNetworkTester.Core.Scanning;

public sealed record ScanProgress(int CompletedAddresses, int TotalAddresses)
{
    public int PercentComplete => TotalAddresses <= 0
        ? 0
        : (int)Math.Round(CompletedAddresses * 100d / TotalAddresses, MidpointRounding.AwayFromZero);
}
