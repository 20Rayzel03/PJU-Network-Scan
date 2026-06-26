using PjuNetworkTester.Core.Networking;

namespace PjuNetworkTester.Core.Tests;

public sealed class OuiVendorLookupTests
{
    [Theory]
    [InlineData("AA:BB:CC:11:22:33")]
    [InlineData("aa-bb-cc-11-22-33")]
    [InlineData("AABBCC112233")]
    public void FindVendor_accepts_common_mac_formats(string macAddress)
    {
        var lookup = new OuiVendorLookup(new Dictionary<string, string>
        {
            ["AABBCC"] = "PJU Test Vendor"
        });

        var vendor = lookup.FindVendor(macAddress);

        Assert.Equal("PJU Test Vendor", vendor);
    }

    [Fact]
    public void FindVendor_returns_null_for_unknown_prefix()
    {
        var lookup = new OuiVendorLookup(new Dictionary<string, string>
        {
            ["AABBCC"] = "PJU Test Vendor"
        });

        var vendor = lookup.FindVendor("11:22:33:44:55:66");

        Assert.Null(vendor);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-mac")]
    [InlineData("AA:BB")]
    public void FindVendor_returns_null_for_invalid_mac(string macAddress)
    {
        var lookup = new OuiVendorLookup(new Dictionary<string, string>
        {
            ["AABBCC"] = "PJU Test Vendor"
        });

        var vendor = lookup.FindVendor(macAddress);

        Assert.Null(vendor);
    }
}
