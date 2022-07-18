using Android.App;
using Android.OS;
using Android.Runtime;
using izolabella.Music.Platforms.Android;
using izolabella.Music.Structure.Music.Songs;
using izolabella.Music.Structure.Requests;
using static Android.Net.Wifi.WifiEnterpriseConfig;
using static Android.OS.PowerManager;

namespace izolabella.LoFi.App
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr Handle, JniHandleOwnership Ownership)
            : base(Handle, Ownership)
        {
            MainPage.VolumeChanged += async (Vol) =>
            {
                while (this.Player == null)
                {
                    await Task.Delay(10);
                }
                this.Player?.SetVolume((float)Vol);
            };
            PowerManager? PM = (PowerManager?)GetSystemService(PowerService);
            this.Lock = PM?.NewWakeLock(WakeLockFlags.ScreenDim, "WLOCK");
            this.Lock?.Acquire();
            MainPage.OnReconnect += this.ReconnectAsync;
            this.StartControlLoop();
        }
        
        public bool Reconnect { get; set; }

        public WakeLock? Lock { get; }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public static Task<Request<List<IzolabellaSong>>> Songs => MainPage.Client.GetServerQueue();

        public IzolabellaSong? NowPlaying { get; set; }

        public AndroidMusicPlayer? Player { get; private set; }

        public int BytesIndex { get; private set; }

        public int BufferSize { get; } = 192000 * 7;

        public DateTime StartedAt { get; private set; } = DateTime.MinValue;

        private Task ReconnectAsync()
        {
            this.Reconnect = true;
            return Task.CompletedTask;
        }

        public async Task<bool> UpdateSong()
        {
            bool SongOver = this.NowPlaying != null && this.Player != null && this.BytesIndex >= this.Player.File.FileInformation.LengthInBytes;
            if (this.NowPlaying == null || SongOver || this.Reconnect)
            {
                this.Reconnect = false;
                if(SongOver)
                {
                    this.BytesIndex = 0;
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
                Request<List<IzolabellaSong>> Q = await Songs;
                if(Q.Success)
                {
                    this.NowPlaying = Q.Result.FirstOrDefault();
                    if (this.NowPlaying != null)
                    {
                        MainPage.MPSet(this.NowPlaying, this.NowPlaying.FileInformation.FileDuration);
                    }
                    this.StartedAt = DateTime.UtcNow;
                    return true;
                }
            }
            return false;
        }

        private async void StartControlLoop(DateTime? ExpectedEnd = null)
        {
            bool SongNeededToBeUpdated = await this.UpdateSong();
            if (this.NowPlaying != null)
            {
                this.Player ??= new AndroidMusicPlayer(this.NowPlaying, Android.Media.ChannelOut.Stereo, this.BufferSize);
                Request<byte[]> From = await MainPage.Client.GetBytesAsync(this.NowPlaying.Id, this.BytesIndex, BufferSize);
                if(From.Success)
                {
                    this.BytesIndex += From.Result.Length;
                    if (!SongNeededToBeUpdated && ExpectedEnd.HasValue)
                    {
                        TimeSpan A = ExpectedEnd.Value.Subtract(DateTime.UtcNow);
                        await Task.Delay(A < TimeSpan.Zero ? TimeSpan.Zero : A);
                    }
                    if (SongNeededToBeUpdated)
                    {
                        float PreVol = this.Player.Volume;
                        this.Player.Dispose();
                        this.Player = new(this.NowPlaying, Android.Media.ChannelOut.Stereo, this.BufferSize);
                        await this.Player.SetVolume(PreVol);
                    }
                    this.Lock?.Acquire();
                    await Player.FeedBytesAsync(From.Result);
                    DateTime Exp = DateTime.UtcNow.Add(this.Player.File.GetTimeFromByteLength(From.Result.LongLength));
                    this.StartControlLoop(Exp);
                }
                else
                {
                    this.StartControlLoop();
                }
            }
            else
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                this.StartControlLoop();
            }
        }
    }
}