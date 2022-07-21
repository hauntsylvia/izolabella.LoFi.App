using System.Reflection;
using izolabella.LoFi.App.Wide.Services.Implementations;
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
using izolabella.LoFi.App.Wide.Constants;

namespace izolabella.LoFi.App
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            this.Client = new(IzolabellaDevAuth);

            this.SongNameLabel.Opacity = 0;
            this.ArtistNameLabel.Opacity = 0;
            this.VolumeSlider.Opacity = 0;
            this.VolumeSlider.Value = 0;

            this.SongNameAnimator = new(ColorConfigs.Config, false, this.SongNameLabel);
            this.ArtistNamesAnimator = new(ColorConfigs.Config, true, this.ArtistNameLabel);
            this.VolumeSliderAnimator = new(ColorConfigs.Config, false, this.VolumeSlider);

            this.Client.OnError += this.ClientErrorAsync;
            Service = new GenericMusicService((A, BufferSize) =>
            {
                MusicPlayer? P = null;
                if(A.Started)
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
            this.Loaded += this.PageReadyAsync;
        }

        private Task NewSongStartedAsync(bool FirstSong, Wide.Services.Results.NowPlayingResult NowPlayingInformation)
        {
            if(NowPlayingInformation.Started)
            {
                this.SongNameLabel.Text = NowPlayingInformation.Playing.Name;
                this.ArtistNameLabel.Text = "??";
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

        #endregion

        #region events

        public delegate Task OnVolumeChangeHandler(float NewVolume);
        public event OnVolumeChangeHandler? OnVolumeChanged;

        public delegate Task MusicPlayersNeedToReconnectHandler();
        public event MusicPlayersNeedToReconnectHandler? OnReconnect;

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
            if(Resize.AnimationIsRunning("1"))
            {
                Resize.AbortAnimation("1");
            }
            if(Resize.AnimationIsRunning("2"))
            {
                Resize.AbortAnimation("2");
            }
            if(Resize.AnimationIsRunning("3"))
            {
                Resize.AbortAnimation("3");
            }
            if(Resize.AnimationIsRunning("4"))
            {
                Resize.AbortAnimation("4");
            }
            if (MainGrid.Width > MainGrid.Height)
            {
                double TX = (Resize.Width / 2) - (MainGrid.Width / 2);
                double TY = MainGrid.Height - Resize.Height;

                Resize.Animate("1", new Animation((Value) => Resize.TranslationX = Value, Resize.TranslationX, TX, Easing.CubicOut), length: 500);

                Resize.Animate("2", new Animation((Value) => Resize.TranslationY = Value, Resize.TranslationY, TY, Easing.CubicOut), length: 500);
            }
            else
            {
                Resize.Animate("3", new Animation((Value) => Resize.TranslationX = Value, Resize.TranslationX, 0, Easing.CubicOut), length: 500);
                Resize.Animate("4", new Animation((Value) => Resize.TranslationY = Value, Resize.TranslationY, 0, Easing.CubicOut), length: 500);
            }
        }

        private void VolChanged(object Sender, ValueChangedEventArgs E)
        {
            OnVolumeChanged?.Invoke((float)E.NewValue);
        }

        private void ClientErrorAsync(Exception Ex)
        {
            this.OnReconnect?.Invoke();
        }

        private void DragCompleted(object Sender, EventArgs E)
        {
        }

        #endregion

        #region page ready

        private async void PageReadyAsync(object? Sender, EventArgs E)
        {
            this.SongNameAnimator.Appear();
            this.ArtistNamesAnimator.Appear();
            this.VolumeSliderAnimator.Appear();
            this.VolumeSliderAnimator.SlideTo(0.25f);
            await this.Service.StartAsync();
        }

        #endregion
    }
}