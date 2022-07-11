using System.Reflection;

namespace izolabella.LoFi.App
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
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