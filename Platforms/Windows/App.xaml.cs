using izolabella.Music.Platforms.Windows;
using izolabella.Music.Structure.Clients;
using izolabella.Music.Structure.Music.Songs;
using Microsoft.UI.Xaml;
using NAudio.Wave;
using System.Reflection;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace izolabella.LoFi.App.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            //string? Dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //if (Dir != null)
            //{
            //    //Stream S = new FileStream(Path.Combine(Dir, "MidnightVisitors.wav"), FileMode.Open, FileAccess.ReadWrite);
            //    //WindowsMusicPlayer P = new(new(S, 48000, 2));
            //    //P.StartAsync();
            //}

            new Task(() => this.ControlSongLoop()).Start();
        }

        private int from = 0;
        public int From
        {
            get => this.Last != null && this.Last.FileSize > this.from ? this.from : 0;
            set => this.from = this.Last != null && this.Last.FileSize < value ? 0 : value;
        }

        public TimeSpan BufferDur { get; } = TimeSpan.FromSeconds(15);

        public int Max => this.Player?.Provider.BufferLength ?? 1;

        public int BufferSize => (int)this.Max / 4;

        public IzolabellaSong? Last { get; private set; }
        public WindowsMusicPlayer? Player { get; private set; }

        public double MaxQueue => 20;

        public List<IEnumerable<byte>> Queue { get; private set; } = new();

        private async void ControlSongLoop()
        {
            this.Last ??= await MainPage.Client.GetCurrentlyPlayingAsync();
            if (this.Last != null)
            {
                MainPage.SetNP(this.Last);
                if (this.Player == null)
                {
                    this.Player = new WindowsMusicPlayer(new(48000, 2, this.Last.FileSize), this.BufferDur);
                    await this.Player.StartAsync();
                }
                await this.FillArrayAsync();
                byte[] Feed = this.Queue.ToList().Take(this.BufferSize).SelectMany(B => B.ToList()).ToArray();
                if(this.Player.Provider.BufferedDuration != TimeSpan.Zero)
                {
                    await Task.Delay(this.Player.Provider.BufferedDuration.Subtract(TimeSpan.FromMilliseconds(20)));
                }
                await this.Player.FeedBytesAsync(Feed);
                if (this.Queue.Count >= this.BufferSize)
                {
                    this.Queue.RemoveRange(0, this.BufferSize);
                }
                else
                {
                    this.Queue.Clear();
                }
                this.ControlSongLoop();
            }
        }

        private async Task FillArrayAsync()
        {
            if(this.Queue.Count < this.MaxQueue)
            {
                byte[] Feed = await MainPage.Client.GetBytesAsync(null, this.From, (int)this.Max);
                this.From += Feed.Length;
                this.Queue.Add(Feed);
            }
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}