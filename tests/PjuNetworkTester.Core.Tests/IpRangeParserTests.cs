using PjuNetworkTester.Core.Networking;

namespace PjuNetworkTester.Core.Tests;

public sealed class IpRangeParserTests
{
    [Fact]
    public void Parse_single_ip_returns_same_start_and_end()
    {
        var result = IpRangeParser.Parse("10.1.5.20");

        Assert.Equal("10.1.5.20", result.Start.ToString());
        Assert.Equal("10.1.5.20", result.End.ToString());
        Assert.Equal(1u, result.AddressCount);
    }

    [Fact]
    public void Parse_explicit_range_preserves_start_and_end()
    {
        var result = IpRangeParser.Parse("10.1.2.1 - 10.1.25.255");

        Assert.Equal("10.1.2.1", result.Start.ToString());
        Assert.Equal("10.1.25.255", result.End.ToString());
        Assert.Equal(6143u, result.AddressCount);
    }

    [Fact]
    public void Parse_cidr_network_returns_complete_network_range()
    {
        var result = IpRangeParser.Parse("10.1.5.0/24");

        Assert.Equal("10.1.5.0", result.Start.ToString());
        Assert.Equal("10.1.5.255", result.End.ToString());
        Assert.Equal(256u, result.AddressCount);
    }

    [Fact]
    public void Parse_cidr_host_address_returns_complete_network_range()
    {
        var result = IpRangeParser.Parse("10.1.5.1/24");

        Assert.Equal("10.1.5.0", result.Start.ToString());
        Assert.Equal("10.1.5.255", result.End.ToString());
        Assert.Equal(256u, result.AddressCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-ip")]
    [InlineData("10.1.5.1/33")]
    [InlineData("10.1.5.255 - 10.1.5.1")]
    public void Parse_invalid_input_throws_clear_validation_error(string input)
    {
        var exception = Assert.Throws<FormatException>(() => IpRangeParser.Parse(input));

        Assert.Contains("IP-Bereich", exception.Message);
    }
}
