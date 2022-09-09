using CommunityToolkit.Maui.Alerts;
using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace izolabella.LoFi.WinUI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : MauiWinUIApplication
{
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
        MainPage.OnLoggedIn += this.LoggedInAsync;
    }

    private Task LoggedInAsync(Music.Structure.Users.LoFiUser User)
    {
        var T = Toast.Make($"Verified as {User.Profile.DisplayName}.");
        T.Show();
        return Task.CompletedTask;
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}