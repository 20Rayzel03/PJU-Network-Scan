using PjuNetworkTester.Core.Settings;

namespace PjuNetworkTester.Core.Tests;

public sealed class AppLocalizerTests
{
    [Theory]
    [InlineData(AppLanguage.German, "Scan starten")]
    [InlineData(AppLanguage.English, "Start scan")]
    public void Translate_returns_language_specific_text(AppLanguage language, string expected)
    {
        var localizer = new AppLocalizer(language);

        var value = localizer.Translate(AppText.StartScan);

        Assert.Equal(expected, value);
    }

    [Fact]
    public void TranslateStatus_returns_localized_status_labels()
    {
        Assert.Equal("Nicht erreichbarer Bereich", new AppLocalizer(AppLanguage.German).TranslateStatusLabel(Scanning.ScanDisplayStatus.UnreachableSubnet));
        Assert.Equal("Unreachable subnet", new AppLocalizer(AppLanguage.English).TranslateStatusLabel(Scanning.ScanDisplayStatus.UnreachableSubnet));
    }
}
