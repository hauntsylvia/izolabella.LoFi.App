using System.Reflection;
using izolabella.Music.Structure.Clients;
using izolabella.Music.Structure.Music.Songs;

namespace izolabella.LoFi.App
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            this.DefaultSongNameLabelOpacity = SongNameLabel.Opacity;
            this.DefaultArtistNameLabelOpacity = ArtistNameLabel.Opacity;
            this.DefaultVolumeSliderOpacity = VolumeSlider.Opacity;
            SongNameLabel.Opacity = 0;
            ArtistNameLabel.Opacity = 0;
            VolumeSlider.Opacity = 0;
            VolumeSlider.Value = 0;
            VolumeChangedSlider = VolumeSlider;

            this.StartCurrentlyPlayingLoop(null);
            VolumeChanged += this.MainPageVolumeChange;
        }

        public double DefaultSongNameLabelOpacity { get; }
        public double DefaultArtistNameLabelOpacity { get; }
        public double DefaultVolumeSliderOpacity { get; }

        public static Slider? VolumeChangedSlider { get; private set; }

        public delegate Task ChangeVolumeHandler(double NewVol);
        public static event ChangeVolumeHandler? VolumeChanged;

        public static IzolabellaLoFiClient Client { get; } = new("942A87516E403D09B58C575784434BD3412FB66E8ACFA6F973427AE8E0A1B371");

        private static IzolabellaSong? MusicPlayerSetSong { get; set; }
        private static TimeSpan? TimeLeft { get; set; }

        public static bool LoopOnce { get; set; } = true;

        private async Task MainPageVolumeChange(double NewVol)
        {
            VolumeSlider.CancelAnimations();
            await VolumeSlider.FadeTo(1);
            await VolumeSlider.FadeTo(this.DefaultVolumeSliderOpacity, 250, Easing.CubicInOut);
        }

        private async void AnimateSlider(double NewVal)
        {
            VolumeSlider.Value = 0;
            VolumeSlider.Animate("StartupVolUp", new Animation((V) =>
            {
                VolumeSlider.Value = V;
                SetCurrentVolume((float)VolumeSlider.Value);
            }, 0, NewVal, Easing.CubicOut), length: 2000);
            LoopOnce = false;
            await Task.CompletedTask;
        }

        public static void MPSet(IzolabellaSong Song, TimeSpan TimeRemaining)
        {
            MusicPlayerSetSong = Song;
            TimeLeft = TimeRemaining;
        }

        public async void StartCurrentlyPlayingLoop(IzolabellaSong? LastPlayed)
        {
            if((LastPlayed != MusicPlayerSetSong || LastPlayed == null) && MusicPlayerSetSong != null && TimeLeft.HasValue)
            {
                LastPlayed = MusicPlayerSetSong;
                await SongNameLabel.FadeTo(0, 250, Easing.CubicInOut);
                await ArtistNameLabel.FadeTo(0, 250, Easing.CubicInOut);
                await VolumeSlider.FadeTo(0, 250, Easing.CubicInOut);
                SongNameLabel.Text = MusicPlayerSetSong.Name;
                ArtistNameLabel.Text = LastPlayed.AuthorNamesConcat;
                await SongNameLabel.FadeTo(this.DefaultSongNameLabelOpacity, 250, Easing.CubicInOut);
                await ArtistNameLabel.FadeTo(this.DefaultArtistNameLabelOpacity, 250, Easing.CubicInOut);
                await VolumeSlider.FadeTo(this.DefaultVolumeSliderOpacity, 250, Easing.CubicInOut);
                if (LoopOnce)
                {
                    this.AnimateSlider(0.25);
                }
                await Task.Delay(delay: TimeLeft.Value < TimeSpan.Zero ? TimeSpan.Zero : TimeLeft.Value);
            }
            await Task.Delay(TimeSpan.FromSeconds(1));
            this.StartCurrentlyPlayingLoop(LastPlayed);
        }

        public static float GetCurrentVolume()
        {
            return (float)(VolumeChangedSlider?.Value ?? 0f);
        }

        public static void SetCurrentVolume(float VolGain)
        {
            if(!LoopOnce)
            {
                VolumeChanged?.Invoke(Math.Pow((double)VolGain, 1.2));
            }
        }

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
    }
}