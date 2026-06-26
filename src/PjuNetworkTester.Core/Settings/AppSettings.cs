namespace PjuNetworkTester.Core.Settings;

public enum AppLanguage
{
    German,
    English
}

public sealed record AppSettings(AppLanguage Language, bool ShowOfflineAddresses)
{
    public static AppSettings Default { get; } = new(AppLanguage.German, ShowOfflineAddresses: false);
}
