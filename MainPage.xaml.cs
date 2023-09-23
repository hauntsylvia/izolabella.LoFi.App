using System.Reflection;
using izolabella.LoFi.Wide.Services.Implementations;
using izolabella.Maui.Util.GenericStructures.Animations.Implementations;
#if ANDROID
using izolabella.Music.Platforms.Android;
#elif WINDOWS
using izolabella.Music.Platforms.Windows;
#endif
using izolabella.Music.Structure.Clients;
using izolabella.Music.Structure.Music.Artists;
using izolabella.Music.Structure.Music.Songs;
using izolabella.Music.Structure.Players;
using izolabella.Music.Structure.Requests;
using izolabella.LoFi.Wide.Constants;
using izolabella.LoFi.Wide.Services.Results;
using izolabella.Music.Structure.Users;
using izolabella.Backend.Objects.Structures.Backend;

namespace izolabella.LoFi
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            this.Appearing += this.PageFocusAsync;
            this.InitializeComponent();
            this.Client = new();
            this.Client.OnDisconnect += this.DisconnectedAsync;
            this.Client.OnReconnect += this.ReconnectedAsync;

            Clipboard.Default.ClipboardContentChanged += this.ClipboardTextChanged;

            this.SongNameLabel.Opacity = 0;
            this.ArtistNameLabel.Opacity = 0;
            this.VolumeSlider.Opacity = 0;
            this.VolumeSlider.Value = 0;

            this.SongNameAnimator = new(ColorConfigs.Config, this.SongNameLabel);
            this.ArtistNamesAnimator = new(ColorConfigs.Config,  this.ArtistNameLabel);
            this.VolumeSliderAnimator = new(ColorConfigs.Config, this.VolumeSlider);

            this.Service = new GenericMusicService((A, BufferSize) =>
            {
                MusicPlayer? P = null;
                if (A.Started)
                {
#if WINDOWS
                    P = new WindowsMusicPlayer(A.Playing, BufferSize);
#elif ANDROID
                    P = new AndroidMusicPlayer(A.Playing, Android.Media.ChannelOut.Stereo, BufferSize);
#endif
                }
                return P ?? throw new PlatformNotSupportedException();
            }, this);
            this.Service.NextSongRequested += this.NewSongStartedAsync;
            StaticService = this.Service;
            this.Loaded += this.PageReadyAsync;
        }

        private async void PageFocusAsync(object? Sender, EventArgs E)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            this.ClipboardTextChanged(Sender, E);
        }

        private async void ClipboardTextChanged(object? Sender, EventArgs E)
        {
            string? Text = await Clipboard.Default.GetTextAsync();
            if (Text != null)
            {
                Request<LoFiUser?> Verify = await this.Client.TryVerifyAsync(Text, false);
                if (Verify.Success && Verify.Result != null)
                {
                    await DataStores.CredStore.DeleteAllAsync();
                    await DataStores.CredStore.SaveAsync(Verify.Result);
                    await this.Client.SetAuthorizationAsync(Text, false);
                    OnLoggedIn?.Invoke(Verify.Result);
                }
            }
        }

        private Task NewSongStartedAsync(bool FirstSong, NowPlayingResult NowPlayingInformation)
        {
            if (NowPlayingInformation.Started)
            {
                this.Dispatcher.Dispatch(() =>
                {
                    this.SongNameLabel.Text = NowPlayingInformation.Playing.Name;
                    this.ArtistNameLabel.Text = IzolabellaSong.GetAuthorDisplay(NowPlayingInformation.Authors);
                    this.VolumeSlider.IsEnabled = true;
                    this.VolumeSliderAnimator.Enable();
                    this.ArtistNamesAnimator.Enable();
                });
            }
            return Task.CompletedTask;
        }

        #region animators

        public MauiTextAnimator SongNameAnimator { get; }
        public MauiTextAnimator ArtistNamesAnimator { get; }
        public MauiSliderAnimator VolumeSliderAnimator { get; }

        #endregion

        #region complexities

        public static string DevAuth => "942A87516E403D09B58C575784434BD3412FB66E8ACFA6F973427AE8E0A1B371";
        public static string IzolabellaDevAuth => "F0744E696705C239BAF094C2D754CB3A4A7DDE3B629B07308B53D993F0933395";

        public IzolabellaLoFiClient Client { get; private set; }

        public GenericMusicService Service { get; private set; }
        private static GenericMusicService? StaticService { get; set; }

        private float VolumeBeforeDisconnect { get; set; } = 0f;

        public static async Task<GenericMusicService> GetService()
        {
            while (StaticService == null)
            {
                await Task.Delay(1000);
            }
            return StaticService;
        }

        #endregion

        #region events

        public delegate Task OnVolumeChangeHandler(float NewVolume);
        public event OnVolumeChangeHandler? OnVolumeChanged;

        public delegate Task OnLoggedInHandler(LoFiUser User);
        public static event OnLoggedInHandler? OnLoggedIn;

        #endregion

        #region ui stuff

        /// <summary>
        /// The method that'll resize the currently playing frame to the correct location based on desktop vs. mobile 
        /// resolutions.
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="Args"></param>
        private void MainPageSizeChanged(object Sender, EventArgs Args)
        {
            //if(Resize.AnimationIsRunning("1") || Resize.AnimationIsRunning("2") || Resize.AnimationIsRunning("3") || Resize.AnimationIsRunning("4"))
            //{
            //    return;
            //}
            if (this.Resize.AnimationIsRunning("1"))
            {
                this.Resize.AbortAnimation("1");
            }
            if (this.Resize.AnimationIsRunning("2"))
            {
                this.Resize.AbortAnimation("2");
            }
            if (this.Resize.AnimationIsRunning("3"))
            {
                this.Resize.AbortAnimation("3");
            }
            if (this.Resize.AnimationIsRunning("4"))
            {
                this.Resize.AbortAnimation("4");
            }
            if (this.MainGrid.Width > this.MainGrid.Height)
            {
                double TX = (this.Resize.Width / 2) - (this.MainGrid.Width / 2);
                double TY = this.MainGrid.Height - this.Resize.Height;

                this.Resize.Animate("1", new Animation((Value) => this.Resize.TranslationX = Value, this.Resize.TranslationX, TX, Easing.CubicOut), length: 500);

                this.Resize.Animate("2", new Animation((Value) => this.Resize.TranslationY = Value, this.Resize.TranslationY, TY, Easing.CubicOut), length: 500);
            }
            else
            {
                this.Resize.Animate("3", new Animation((Value) => this.Resize.TranslationX = Value, this.Resize.TranslationX, 0, Easing.CubicOut), length: 500);
                this.Resize.Animate("4", new Animation((Value) => this.Resize.TranslationY = Value, this.Resize.TranslationY, 0, Easing.CubicOut), length: 500);
            }
        }

        private void VolChanged(object Sender, ValueChangedEventArgs E)
        {
            OnVolumeChanged?.Invoke((float)E.NewValue);
        }

        private void ReconnectedAsync()
        {
            this.Dispatcher.Dispatch(() =>
            {
                this.VolumeSliderAnimator.Enable();
                this.ArtistNamesAnimator.Enable();
                this.VolumeSliderAnimator.SlideTo(this.VolumeBeforeDisconnect);
            });
        }

        private void DisconnectedAsync()
        {
            this.VolumeBeforeDisconnect = this.Service.LastMusicPlayer?.LastVolume ?? 0f;
            this.Dispatcher.Dispatch(() =>
            {
                this.VolumeSliderAnimator.Disable();
                this.ArtistNamesAnimator.Disable();
                this.VolumeSliderAnimator.SlideTo(0);
                this.ArtistNameLabel.Text = "..";
                this.SongNameLabel.Text = "Error";
            });
        }

        private void DragCompleted(object Sender, EventArgs E)
        {
        }

        #endregion

        #region page ready

        private void PageReadyAsync(object? Sender, EventArgs E)
        {
            this.SongNameLabel.Text = "Initializing . .";
            this.SongNameAnimator.Appear();
            Task StartClientTask = this.Client.StartAsync(null);
            this.ArtistNamesAnimator.Appear();
            this.VolumeSliderAnimator.Appear();
            this.VolumeSliderAnimator.SlideTo(0.25f);
            Task StartMusic = this.Service.StartAsync();
            new Task(async () =>
            {
                LoFiUser? C = (await DataStores.CredStore.ReadAllAsync<LoFiUser>()).FirstOrDefault();
                if (C != null)
                {
                    Request<LoFiUser?> Self = await this.Client.TryVerifyAsync(C.Credentials.Secret, false).ConfigureAwait(false);
                    if (Self.Success && Self.Result != null)
                    {
                        await this.Client.SetAuthorizationAsync(C.Credentials.Secret, false).ConfigureAwait(false);
                        OnLoggedIn?.Invoke(Self.Result);
                    }
                }
            }).Start();
        }

        #endregion
    }
}