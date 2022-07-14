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
            IzolabellaSong? S = Client.GetCurrentlyPlayingAsync().Result;
            SetAction((S) =>
            {
                SongNameLabel.Text = S.Name;
                ArtistNameLabel.Text = S.Author.Name;
            });
            if (S != null)
            {
                SongNameLabel.Text = S.Name;
                ArtistNameLabel.Text = S.Author.Name;
            }
        }

        public static IzolabellaLoFiClient Client { get; } = new();

        public static List<Action<IzolabellaSong>> Actions { get; set; } = new();

        public static void SetNP(IzolabellaSong NP)
        {
            foreach(Action<IzolabellaSong> Act in Actions)
            {
                Act.Invoke(NP);
            }
        }

        public static void SetAction(Action<IzolabellaSong> A)
        {
            Actions.Add(A);
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