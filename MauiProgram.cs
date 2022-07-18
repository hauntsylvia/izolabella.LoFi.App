using System.Reflection;
using izolabella.Music.Structure.Clients;
using izolabella.Music.Structure.Music.Songs;

namespace izolabella.LoFi.App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            MauiAppBuilder Builder = MauiApp.CreateBuilder();
            Builder
                .UseMauiApp<App>()
                .ConfigureFonts(Fonts => Fonts.AddFont("RobotoMonoMedium.ttf", "RobotoMonoMedium"));
            return Builder.Build();
        }
    }
}