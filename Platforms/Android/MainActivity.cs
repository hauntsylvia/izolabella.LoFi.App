using Android.App;
using Android.Content.PM;
using Android.OS;
using izolabella.Music.Structure.Music.Songs;
using Android.Content;
using izolabella.LoFi.App.Platforms.Android.Services.Implementations;

namespace izolabella.LoFi.App.Platforms.Android
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        public static bool AlreadyStarted { get; set; } = false;

        public Intent? I { get; set; }

        public void StartMusicService()
        {
            this.I = new(this, typeof(MusicService));
            if (OperatingSystem.IsAndroidVersionAtLeast(26))
            {
                //this.StartForegroundService(I);
            }
            else
            {
                //this.StartService(I);
            }
        }

        public void StopMusicService()
        {
            if (this.I != null)
            {
                this.StopService(this.I);
            }
        }

        protected override void OnCreate(Bundle? SavedInstanceState)
        {
            if (!AlreadyStarted)
            {
                AlreadyStarted = true;
                this.StartMusicService();
            }
            base.OnCreate(SavedInstanceState);
        }

        protected override void OnDestroy()
        {
            this.StopMusicService();
            base.OnDestroy();
        }
    }
}