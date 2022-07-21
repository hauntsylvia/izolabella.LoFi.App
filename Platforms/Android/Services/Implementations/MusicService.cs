using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using izolabella.Music.Platforms.Android;
using izolabella.Music.Structure.Music.Songs;
using izolabella.Music.Structure.Requests;
using static Android.OS.PowerManager;
using static Android.Net.Wifi.WifiEnterpriseConfig;
using static Android.Media.ChannelOut;
using Android.Media;
using Android.Content.PM;
using izolabella.LoFi.App.Platforms.Android.Notifications;
using izolabella.Music.Structure.Music.Artists;

namespace izolabella.LoFi.App.Platforms.Android.Services.Implementations
{
    [Service(Name = $"com.izolabella.LoFi.{nameof(MusicService)}", Exported = true, ForegroundServiceType = ForegroundService.TypeMediaPlayback)]
    public class MusicService : Service
    {
        public bool Reconnect { get; set; }

        public static Task<Request<List<IzolabellaSong>>> Songs => MainPage.Client.GetServerQueue();

        public IzolabellaSong? NowPlaying { get; set; }
        public IEnumerable<IzolabellaAuthor>? NowPlayingAuthors { get; private set; }

        public AndroidMusicPlayer? Player { get; private set; }

        public int BytesIndex { get; private set; }

        public int BufferSize { get; } = 192000 * 7;

        public DateTime StartedAt { get; private set; } = DateTime.MinValue;

        public override IBinder OnBind(Intent? I)
        {
            return new Binder();
        }

        public override StartCommandResult OnStartCommand(Intent? I, StartCommandFlags F, int StartId)
        {
            IzolabellaMusicChannel Channel = new("LoFi", "The running notifications.");
            NotificationManager? NotificationManager = (NotificationManager?)GetSystemService(NotificationService);
            if (NotificationManager != null && OperatingSystem.IsAndroidVersionAtLeast(29) && NotificationManager.GetNotificationChannel(Channel.Id) == null)
            {
                NotificationChannel Ch = Channel.Build();
                Ch.SetSound(null, null);
                NotificationManager?.CreateNotificationChannel(Ch);
            }

            Intent OnClickIntent = new(this, typeof(MainActivity));
            TaskStackBuilder? StackBuilder = TaskStackBuilder.Create(this);
            StackBuilder?.AddParentStack(Java.Lang.Class.FromType(typeof(MainActivity)));
            StackBuilder?.AddNextIntent(OnClickIntent);

            PendingIntent? OnClickPendingIntent = StackBuilder?.GetPendingIntent(0, PendingIntentFlags.Mutable);

            Notification Notification = IzolabellaNotification.Factory(this, Channel)
                .SetContentIntent(OnClickPendingIntent)
                .SetOngoing(true)
                .SetContentText("Playing . . .")
                .SetSmallIcon(Resource.Drawable.navigation_empty_icon)
                .SetVisibility(NotificationVisibility.Public)
                .SetCategory("LoFi")
                .Build();
            if (OperatingSystem.IsAndroidVersionAtLeast(29))
            {
                this.StartForeground(110042340, Notification, ForegroundService.TypeMediaPlayback);
            }

            MainPage.OnReconnect += this.ReconnectAsync;
            MainPage.VolumeChanged += async (Vol) =>
            {
                while (this.Player == null)
                {
                    await Task.Delay(10);
                }
                this.Player?.SetVolume((float)Vol);
            };
            this.StartControlLoop(null);
            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            if(this.Player != null)
            {
                this.Player.SetVolume(0f);
                this.Player.Dispose();
            }
            this.Player = null;
            this.NowPlaying = null;
            base.OnDestroy();
        }

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
                if (SongOver)
                {
                    this.BytesIndex = 0;
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
                Request<List<IzolabellaSong>> Q = await Songs;
                if (Q.Success)
                {
                    this.NowPlaying = Q.Result.FirstOrDefault();
                    if (this.NowPlaying != null)
                    {
                        Request<List<IzolabellaAuthor>> A = await MainPage.Client.GetSongAuthorsAsync(this.NowPlaying.Id);
                        if(A.Success)
                        {
                            this.NowPlayingAuthors = A.Result;
                            MainPage.MPSet(this.NowPlaying, this.NowPlayingAuthors, this.NowPlaying.FileInformation.FileDuration);
                        }
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
                this.Player ??= new AndroidMusicPlayer(this.NowPlaying, ChannelOut.Stereo, this.BufferSize);
                Request<byte[]> From = await MainPage.Client.GetBytesAsync(this.NowPlaying.Id, this.BytesIndex, BufferSize);
                if (From.Success)
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
                        this.Player = new(this.NowPlaying, ChannelOut.Stereo, this.BufferSize);
                        await this.Player.SetVolume(PreVol);
                    }
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
