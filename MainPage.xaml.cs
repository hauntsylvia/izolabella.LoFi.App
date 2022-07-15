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
            VolumeChangedSlider = VolumeSlider;
            VolumeChanged?.Invoke(GetCurrentVolume());
            this.StartCurrentlyPlayingLoop(null);
            VolumeChanged += this.MainPageVolumeChange;
            this.DefaultSongNameLabelOpacity = SongNameLabel.Opacity;
            this.DefaultArtistNameLabelOpacity = ArtistNameLabel.Opacity;
            this.DefaultVolumeSliderOpacity = VolumeSlider.Opacity;
            SongNameLabel.Opacity = 0;
            ArtistNameLabel.Opacity = 0;
            VolumeSlider.Opacity = 0;
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

        private async Task MainPageVolumeChange(double NewVol)
        {
            VolumeSlider.CancelAnimations();
            await VolumeSlider.FadeTo(1);
            await VolumeSlider.FadeTo(this.DefaultVolumeSliderOpacity, 250, Easing.CubicInOut);
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
                ArtistNameLabel.Text = string.Join(" & ", MusicPlayerSetSong.Authors.Select(A => A.Name));
                await SongNameLabel.FadeTo(this.DefaultSongNameLabelOpacity, 250, Easing.CubicInOut);
                await ArtistNameLabel.FadeTo(this.DefaultArtistNameLabelOpacity, 250, Easing.CubicInOut);
                await VolumeSlider.FadeTo(this.DefaultVolumeSliderOpacity, 250, Easing.CubicInOut);
                await Task.Delay(delay: TimeLeft.Value < TimeSpan.Zero ? TimeSpan.Zero : TimeLeft.Value);
            }
            await Task.Delay(TimeSpan.FromSeconds(1));
            this.StartCurrentlyPlayingLoop(LastPlayed);
        }

        public static float GetCurrentVolume()
        {
            return (float)(VolumeChangedSlider?.Value ?? 0.25f);
        }

        public static void SetCurrentVolume(float VolGain)
        {
            VolumeChanged?.Invoke(VolGain);
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
            Resize.AbortAnimation("1");
            Resize.AbortAnimation("2");
            Resize.AbortAnimation("3");
            Resize.AbortAnimation("4");
            if (MainGrid.Width > MainGrid.Height)
            {
                double TX = (Resize.Width / 2) - (MainGrid.Width / 2);
                double TY = MainGrid.Height - Resize.Height;

                Resize.Animate("1", new Animation((Value) =>
                {
                    Resize.TranslationX = Value;
                }, Resize.TranslationX, TX, Easing.CubicOut), length: 500);

                Resize.Animate("2", new Animation((Value) =>
                {
                    Resize.TranslationY = Value;
                }, Resize.TranslationY, TY, Easing.CubicOut), length: 500);
            }
            else
            {
                Resize.Animate("3", new Animation((Value) =>
                {
                    Resize.TranslationX = Value;
                }, Resize.TranslationX, 0, Easing.CubicOut), length: 500);
                Resize.Animate("4", new Animation((Value) =>
                {
                    Resize.TranslationY = Value;
                }, Resize.TranslationY, 0, Easing.CubicOut), length: 500);
            }
        }

        private void VolChanged(object Sender, ValueChangedEventArgs E)
        {
            VolumeChanged?.Invoke(E.NewValue);
        }

        private void DragCompleted(object Sender, EventArgs E)
        {
        }
    }
}