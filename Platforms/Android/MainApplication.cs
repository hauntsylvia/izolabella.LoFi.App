using Android.App;
using Android.Runtime;
using izolabella.Music.IAndroid;

namespace izolabella.LoFi.App
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr Handle, JniHandleOwnership Ownership)
            : base(Handle, Ownership)
        {
            Stream S = FileSystem.OpenAppPackageFileAsync("MidnightVisitors.wav").Result;
            AndroidMusicPlayer A = new(new(S, 48000, 2), Android.Media.ChannelOut.Stereo);
            A.StartAsync();
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}