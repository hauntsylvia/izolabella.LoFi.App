using Android.App;
using Android.OS;
using Android.Runtime;
//using izolabella.LoFi.App.Platforms.Android.Services.Implementations;
using izolabella.Music.Platforms.Android;
using izolabella.Music.Structure.Music.Songs;
using izolabella.Music.Structure.Requests;
using static Android.Net.Wifi.WifiEnterpriseConfig;
using static Android.OS.PowerManager;

namespace izolabella.LoFi.App.Platforms.Android
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr Handle, JniHandleOwnership Ownership) : base(Handle, Ownership)
        {
        }

        protected override MauiApp CreateMauiApp()
        {
            return MauiProgram.CreateMauiApp();
        }
    }
}