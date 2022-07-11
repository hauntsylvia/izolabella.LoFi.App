using izolabella.Music.Windows;
using Microsoft.UI.Xaml;
using NAudio.Wave;
using System.Reflection;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace izolabella.LoFi.App.WinUI
{
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
            string Dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Stream S = new FileStream(Path.Combine(Dir, "MidnightVisitors.wav"), FileMode.Open, FileAccess.ReadWrite);
            WindowsMusicPlayer P = new(new(S, 48000, 2));
            P.StartAsync();
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}