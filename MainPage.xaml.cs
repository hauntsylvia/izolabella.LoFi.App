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
            SongNameLabel.Text = NowPlaying.Name;
            ArtistNameLabel.Text = NowPlaying.Author.Name;
        }

        public static IzolabellaSong? NowPlaying { get; set; }

        public static List<MainPage> Actions { get; set; } = new();

        public static void SetNP(IzolabellaSong NP)
        {
            NowPlaying = NP;
            foreach(MainPage P in Actions)
            {
                P.SongNameLabel.Text = NP.Name;
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
            if (MainGrid.Width > MainGrid.Height)
            {
                Resize.TranslateTo(Resize.X - (MainGrid.Width / 2) + 50, MainGrid.Height - 100, 150, Easing.CubicOut);
            }
            else
            {
                Resize.TranslateTo(0, 0, 150, Easing.CubicOut);
            }
        }
    }
}