using izolabella.Maui.Util.GenericStructures.Animations.Bases;

namespace izolabella.LoFi.App
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}