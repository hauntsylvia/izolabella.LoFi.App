using Android.App;
using Android.OS;
using Android.Runtime;
using izolabella.Music.Platforms.Android;
using izolabella.Music.Structure.Music.Songs;
using static Android.OS.PowerManager;

namespace izolabella.LoFi.App
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr Handle, JniHandleOwnership Ownership)
            : base(Handle, Ownership)
        {
            //Stream S = FileSystem.OpenAppPackageFileAsync("MidnightVisitors.wav").Result;
            //AndroidMusicPlayer A = new(, Android.Media.ChannelOut.Stereo);
            //new Task(async () => await A.StartAsync()).Start();


            MainPage.VolumeChanged += async (Vol) =>
            {
                while (this.Player == null)
                {
                    await Task.Delay(10);
                }
                this.Player?.SetVolume((float)Vol);
            };
            PowerManager PM = (PowerManager)GetSystemService(PowerService);
            this.Lock = PM.NewWakeLock(WakeLockFlags.ScreenDim, "WLOCK");
            this.Lock.Acquire();
            this.StartControlLoop();
        }

        public WakeLock? Lock { get; }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public Task<List<IzolabellaSong>?> Songs => MainPage.Client.GetServerQueue();

        public IzolabellaSong? NowPlaying { get; set; }

        public AndroidMusicPlayer? Player { get; private set; }

        public int BytesIndex { get; private set; }

        public int BufferSize { get; } = 192000 * 7;

        public DateTime StartedAt { get; private set; } = DateTime.MinValue;

        public async Task<bool> UpdateSong()
        {
            bool SongOver = this.NowPlaying != null && this.Player != null && this.BytesIndex >= this.Player.File.FileInformation.LengthInBytes;
            if (this.NowPlaying == null || SongOver)
            {
                if(SongOver)
                {
                    this.BytesIndex = 0;
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
                this.NowPlaying = (await (this.Songs ?? Task.FromResult<List<IzolabellaSong>?>(new())))?.FirstOrDefault();
                if(this.NowPlaying != null)
                {
                    MainPage.MPSet(this.NowPlaying, this.NowPlaying.FileInformation.FileDuration);
                }
                this.StartedAt = DateTime.UtcNow;
                return true;
            }
            return false;
        }

        private async void StartControlLoop(DateTime? ExpectedEnd = null)
        {
            bool SongNeededToBeUpdated = await this.UpdateSong();
            if (this.NowPlaying != null)
            {
                this.Player ??= new AndroidMusicPlayer(this.NowPlaying, Android.Media.ChannelOut.Stereo);
                byte[] From = await MainPage.Client.GetBytesAsync(this.NowPlaying.Id, this.BytesIndex, BufferSize);
                this.BytesIndex += From.Length; 
                if(!SongNeededToBeUpdated && ExpectedEnd.HasValue)
                {
                    TimeSpan A = ExpectedEnd.Value.Subtract(DateTime.UtcNow);
                    await Task.Delay(A < TimeSpan.Zero ? TimeSpan.Zero : A);
                }
                if (SongNeededToBeUpdated)
                {
                    float PreVol = this.Player.Volume;
                    this.Player.Dispose();
                    this.Player = new(this.NowPlaying, Android.Media.ChannelOut.Stereo);
                    await this.Player.SetVolume(PreVol);
                }
                this.Lock?.Acquire();
                await Player.FeedBytesAsync(From);
                DateTime Exp = DateTime.UtcNow.Add(this.Player.File.GetTimeFromByteLength(From.Length));
                this.StartControlLoop(Exp);
            }
            else
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                this.StartControlLoop();
            }
        }
    }
}