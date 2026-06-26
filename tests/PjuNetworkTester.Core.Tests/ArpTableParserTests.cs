using System.Net;
using PjuNetworkTester.Core.Networking;

namespace PjuNetworkTester.Core.Tests;

public sealed class ArpTableParserTests
{
    [Fact]
    public void ParseWindowsArpOutput_extracts_dynamic_mac_entries()
    {
        const string arpOutput = """
Schnittstelle: 10.1.5.100 --- 0x6
  Internetadresse       Physische Adresse     Typ
  10.1.5.1              aa-bb-cc-11-22-33     dynamisch
  10.1.5.20             44-55-66-77-88-99     dynamisch
  224.0.0.22            01-00-5e-00-00-16     statisch
""";

        var entries = ArpTableParser.ParseWindowsArpOutput(arpOutput);

        Assert.Equal("AA:BB:CC:11:22:33", entries[IPAddress.Parse("10.1.5.1")]);
        Assert.Equal("44:55:66:77:88:99", entries[IPAddress.Parse("10.1.5.20")]);
    }

    [Fact]
    public void ParseLinuxIpNeighborOutput_extracts_reachable_mac_entries()
    {
        const string neighborOutput = """
10.1.5.1 dev eth0 lladdr aa:bb:cc:11:22:33 REACHABLE
10.1.5.20 dev eth0 lladdr 44:55:66:77:88:99 STALE
10.1.5.30 dev eth0 FAILED
""";

        var entries = ArpTableParser.ParseLinuxIpNeighborOutput(neighborOutput);

        Assert.Equal("AA:BB:CC:11:22:33", entries[IPAddress.Parse("10.1.5.1")]);
        Assert.Equal("44:55:66:77:88:99", entries[IPAddress.Parse("10.1.5.20")]);
        Assert.False(entries.ContainsKey(IPAddress.Parse("10.1.5.30")));
    }
}
