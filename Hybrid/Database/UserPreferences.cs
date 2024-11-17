
namespace Hybrid.Database;

public class UserPreferences
{
    public Lazy<string> Email = new(() => Preferences.Default.Get(nameof(PreferenceKeys.Email), ""));
    public Lazy<string> Token = new(() => Preferences.Default.Get(nameof(PreferenceKeys.Token), ""));
}
