using PjuNetworkTester.Core.Settings;

namespace PjuNetworkTester.Core.Tests;

public sealed class AppSettingsStoreTests
{
    [Fact]
    public async Task LoadAsync_returns_defaults_when_file_does_not_exist()
    {
        var settingsPath = Path.Combine(Path.GetTempPath(), $"pju-settings-{Guid.NewGuid():N}.json");
        var store = new AppSettingsStore(settingsPath);

        var settings = await store.LoadAsync(CancellationToken.None);

        Assert.Equal(AppLanguage.German, settings.Language);
        Assert.False(settings.ShowOfflineAddresses);
    }

    [Fact]
    public async Task SaveAsync_and_LoadAsync_roundtrip_settings()
    {
        var settingsPath = Path.Combine(Path.GetTempPath(), $"pju-settings-{Guid.NewGuid():N}.json");
        var store = new AppSettingsStore(settingsPath);
        var expected = new AppSettings(AppLanguage.English, ShowOfflineAddresses: true);

        try
        {
            await store.SaveAsync(expected, CancellationToken.None);

            var actual = await store.LoadAsync(CancellationToken.None);

            Assert.Equal(expected, actual);
        }
        finally
        {
            if (File.Exists(settingsPath))
            {
                File.Delete(settingsPath);
            }
        }
    }
}
