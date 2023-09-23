using System.Reflection;
using izolabella.Music.Structure.Clients;
using izolabella.Music.Structure.Music.Songs;
using izolabella.Music.Structure.Players;
using CommunityToolkit.Maui;

namespace izolabella.LoFi
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            MauiAppBuilder Builder = MauiApp.CreateBuilder();
            Builder.UseMauiApp<App>().ConfigureFonts(Fonts => Fonts.AddFont("RobotoMonoMedium.ttf", "RobotoMonoMedium")).UseMauiCommunityToolkit();
            return Builder.Build();
        }
    }
}