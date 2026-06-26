namespace PjuNetworkTester.Core.Networking;

public static class DefaultVendors
{
    public static IReadOnlyDictionary<string, string> Vendors { get; } = new Dictionary<string, string>
    {
        ["001A2B"] = "Ubiquiti Networks",
        ["001B21"] = "Intel Corporate",
        ["B827EB"] = "Raspberry Pi Foundation",
        ["DCA632"] = "Raspberry Pi Trading",
        ["F4F5D8"] = "Google",
        ["3C5A37"] = "Samsung Electronics",
        ["F0D5BF"] = "Intel Corporate",
        ["00E04C"] = "Realtek Semiconductor",
        ["00155D"] = "Microsoft Corporation",
        ["B8AC6F"] = "Dell Inc.",
        ["F8B156"] = "Dell Inc.",
        ["D067E5"] = "Dell Inc.",
        ["6C3BE5"] = "Hewlett Packard",
        ["A45D36"] = "Hewlett Packard",
        ["000C29"] = "VMware",
        ["005056"] = "VMware",
        ["080027"] = "PCS Systemtechnik / VirtualBox"
    };
}
