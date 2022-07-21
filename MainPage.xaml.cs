using System.Reflection;
using izolabella.LoFi.App.Wide.Services.Implementations;
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

namespace izolabella.LoFi.App
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            this.SongNameLabel.Opacity = 0;
            this.ArtistNameLabel.Opacity = 0;
            this.VolumeSlider.Opacity = 0;
            this.VolumeSlider.Value = 0;

            MainPage.Client.OnError += this.ClientErrorAsync;
            Service = new GenericMusicService((A) =>
            {
                MusicPlayer? P = null;
                if(A.Started)
                {
#if WINDOWS
                    P = new WindowsMusicPlayer(A.Playing, TimeSpan.FromSeconds(15));
#elif ANDROID
                    P = new AndroidMusicPlayer(A.Playing, Android.Media.ChannelOut.Stereo, 192000);
#endif
                }
                return P ?? throw new PlatformNotSupportedException();
            });
            this.Loaded += this.PageReadyAsync;
        }

        private void ClientErrorAsync(Exception Ex)
        {
            throw new NotImplementedException();
        }

        private async void PageReadyAsync(object? sender, EventArgs e)
        {
            await this.Service.StartAsync();
        }

        public static string DevAuth => "942A87516E403D09B58C575784434BD3412FB66E8ACFA6F973427AE8E0A1B371";
        public static string IzolabellaDevAuth => "F0744E696705C239BAF094C2D754CB3A4A7DDE3B629B07308B53D993F0933395";

        public static IzolabellaLoFiClient Client { get; private set; } = new(IzolabellaDevAuth);

        public GenericMusicService Service { get; private set; }

#region old

        public delegate Task MusicPlayersNeedToReconnectHandler();
        public static event MusicPlayersNeedToReconnectHandler? OnReconnect;

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
            SetCurrentVolume((float)E.NewValue);
        }

        private void DragCompleted(object Sender, EventArgs E)
        {
        }

#endregion
    }
}