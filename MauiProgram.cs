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