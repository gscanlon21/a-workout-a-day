﻿using Hybrid.Code;
using Lib.Services;

namespace Hybrid;

public partial class App : Application
{
    private bool HasLoggedIn => Email != null && Token != null;

    private string? Email { get; set; }
    private string? Token { get; set; }

    /// <param name="serviceProvider">https://github.com/dotnet/maui/issues/11485</param>
    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();

        Email = Preferences.Default.Get<string?>(nameof(PreferenceKeys.Email), null);
        Token = Preferences.Default.Get<string?>(nameof(PreferenceKeys.Token), null);
        GlobalExceptionHandler.UnhandledException += async (sender, args) =>
        {
            await serviceProvider.GetRequiredService<UserService>().LogException(Email, Token, args.ExceptionObject.ToString());
        };

        if (HasLoggedIn)
        {
            MainPage = serviceProvider.GetRequiredService<AppShell>();
        }
        else
        {
            MainPage = serviceProvider.GetRequiredService<AuthShell>();
        }
    }
}