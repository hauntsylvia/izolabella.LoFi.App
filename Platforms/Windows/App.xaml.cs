using izolabella.Music.Platforms.Windows;
using izolabella.Music.Structure.Clients;
using izolabella.Music.Structure.Music.Artists;
using izolabella.Music.Structure.Music.Songs;
using izolabella.Music.Structure.Requests;
using Microsoft.Maui.Controls;
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
            //MainPage.VolumeChanged += async (Vol) =>
            //{
            //    while(this.Player == null)
            //    {
            //        await Task.Delay(200);
            //    }
            //    this.Player?.SetVolume((float)Vol);
            //};
            //this.ControlSongLoop();
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        private int From { get; set; } = 0;

        public TimeSpan BufferDur { get; } = TimeSpan.FromSeconds(15);

        public int Max => this.Player?.Provider.BufferLength ?? 1;

        public int BufferSize => (int)this.Max / 4;

        public List<IzolabellaSong>? ServerQueue { get; set; }

        public IzolabellaSong? CurrentlyPlaying => this.ServerQueue?.FirstOrDefault();
        public IEnumerable<IzolabellaAuthor>? CurrentlyPlayingAuthors { get; private set; }

        public TimeSpan TimeLeft => this.CurrentlyPlaying != null ? this.CurrentlyPlaying.FileInformation.FileDuration.Subtract(this.CurrentlyPlaying.GetTimeFromByteLength(this.From)) : TimeSpan.Zero;

        public WindowsMusicPlayer? Player { get; private set; }

        public TimeSpan TimeToFinish => this.CurrentlyPlaying?.GetTimeFromByteLength(this.CurrentlyPlaying.FileInformation.LengthInBytes - this.From) ?? TimeSpan.Zero;

        private async Task<bool> UpdateSong()
        {
            if(this.TimeToFinish <= TimeSpan.Zero)
            {
                this.ServerQueue ??= (await MainPage.Client.GetServerQueue()).Result ?? new();
                this.From = 0;
                this.ServerQueue.Add(this.ServerQueue.ElementAt(0));
                this.ServerQueue.RemoveAt(0);
                if(this.CurrentlyPlaying != null)
                {
                    Request<List<IzolabellaAuthor>> AuthorsReq = await MainPage.Client.GetSongAuthorsAsync(this.CurrentlyPlaying.Id);
                    if(AuthorsReq.Success)
                    {
                        this.CurrentlyPlayingAuthors = AuthorsReq.Result;
                    }
                    if(this.Player != null)
                    {
                        this.Player.Dispose();
                    }
                    this.Player = new WindowsMusicPlayer(this.CurrentlyPlaying, this.BufferDur);
                    await this.Player.StartAsync();
                }
                return true;
            }
            return false;
        }

        private async void ControlSongLoop()
        {
            bool SongUpdated = await this.UpdateSong();
            if (this.CurrentlyPlaying != null && this.CurrentlyPlayingAuthors != null)
            {
                this.Player ??= new(this.CurrentlyPlaying, this.BufferDur);
                MainPage.MPSet(this.CurrentlyPlaying, this.CurrentlyPlayingAuthors, this.TimeToFinish);
                Request<byte[]> Feed = await this.FillArrayAsync(this.CurrentlyPlaying);
                if(Feed.Success)
                {
                    TimeSpan WaitFor = TimeSpan.FromSeconds(1);
                    if (!SongUpdated && this.Player.Provider.BufferedDuration != TimeSpan.Zero)
                    {
                        WaitFor = this.Player.Provider.BufferedDuration.Subtract(TimeSpan.FromMilliseconds(10));
                    }
                    await Task.Delay(WaitFor);
                    await this.Player.FeedBytesAsync(Feed.Result);
                }
            }
            this.ControlSongLoop();
        }

        private async Task<Request<byte[]>> FillArrayAsync(IzolabellaSong Current)
        {
            Request<byte[]> Feed = await MainPage.Client.GetBytesAsync(Current.Id, this.From, (int)this.Max);
            if(Feed.Success)
            {
                this.From += Feed.Result.Length;
            }
            return Feed;
        }
    }
}