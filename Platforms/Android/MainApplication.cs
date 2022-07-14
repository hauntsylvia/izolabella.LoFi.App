using Android.App;
using Android.Runtime;
using izolabella.Music.Platforms.Android;

namespace izolabella.LoFi.App
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr Handle, JniHandleOwnership Ownership)
            : base(Handle, Ownership)
        {
            //Stream S = FileSystem.OpenAppPackageFileAsync("MidnightVisitors.wav").Result;
            //AndroidMusicPlayer A = new(new(48000, 2, S.Length), Android.Media.ChannelOut.Stereo);
            //new Task(async () => await A.StartAsync()).Start(); 
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}