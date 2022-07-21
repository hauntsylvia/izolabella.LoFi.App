using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using izolabella.LoFi.App.Wide.Services.Results;
using izolabella.Music.Structure.Clients;
using izolabella.Music.Structure.Music.Songs;
using izolabella.Music.Structure.Players;
using izolabella.Music.Structure.Requests;

namespace izolabella.LoFi.App.Wide.Services.Implementations
{
    public class GenericMusicService
    {
        public GenericMusicService(Func<NowPlayingResult, MusicPlayer> GetNextMusicPlayer)
        {
            this.GetNextMusicPlayer = GetNextMusicPlayer;
            this.Client = MainPage.Client;
            this.NextSongRequested += this.NewSongAsync;
            MainPage.VolumeChanged += this.VolumeChangedAsync;
        }

        public Task StartAsync()
        {
            Task StartTask = this.StartPlayingAsync();
            return Task.CompletedTask;
        }

        public delegate Task NextSongRequestedHandler(bool FirstSong, NowPlayingResult NowPlayingInformation);
        public event NextSongRequestedHandler NextSongRequested;

        public Func<NowPlayingResult, MusicPlayer> GetNextMusicPlayer { get; }

        public MusicPlayer? LastMusicPlayer { get; private set; }

        public IzolabellaLoFiClient Client { get; }

        private List<IzolabellaSong>? Queue { get; set; }

        public int BufferSize { get; } = 192000;

        private int Index { get; set; } = 0;

        public Task SongPlayingTask { get; private set; } = Task.CompletedTask;

        private Task VolumeChangedAsync(double NewVol)
        {
            if(this.LastMusicPlayer != null)
            {
                this.LastMusicPlayer.SetVolume((float)NewVol);
            }
            return Task.CompletedTask;
        }

        private async Task<NowPlayingResult> StartPlayingAsync()
        {
            List<IzolabellaSong> Queue = await this.GetQueueAsync();
            if(Queue.Any())
            {
                NowPlayingResult Result = new(Queue.First(), DateTime.UtcNow.Add(Queue.First().GetTimeFromByteLength(Queue.First().FileInformation.LengthInBytes)));
                this.NextSongRequested?.Invoke(true, Result);
                return Result;
            }
            else
            {
                return NowPlayingResult.Zero;
            }
        }

        private async Task NewSongAsync(bool FirstSong, NowPlayingResult NowPlayingInformation)
        {
            this.LastMusicPlayer = this.GetNextMusicPlayer.Invoke(NowPlayingInformation);
            await this.LastMusicPlayer.StartAsync();
            this.Index = 0;
            this.SongPlayingTask = this.FeedPlayerLoopAsync(this.LastMusicPlayer, NowPlayingInformation, TimeSpan.Zero);
        }

        private async Task FeedPlayerLoopAsync(MusicPlayer PlayerToFeed, NowPlayingResult NowPlayingInformation, TimeSpan TimeLeft)
        {
            if(NowPlayingInformation.Started && TimeLeft <= TimeSpan.Zero)
            {
                int LengthOfSongBytes = NowPlayingInformation.Playing.FileInformation.LengthInBytes;
                if(this.Index >= LengthOfSongBytes)
                {
                    this.NextSongRequested?.Invoke(false, await this.StartPlayingAsync());
                    return;
                }
                Request<byte[]> SongData = await this.Client.GetBytesAsync(NowPlayingInformation.Playing.Id, this.Index, this.BufferSize);
                if (SongData.Success)
                {
                    this.Index += SongData.Result.Length;
                    await PlayerToFeed.FeedBytesAsync(SongData.Result);
                    TimeLeft = NowPlayingInformation.Playing.GetTimeFromByteLength(LengthOfSongBytes - this.Index);
                }
            }
            await Task.Delay(TimeSpan.FromSeconds(1));
            _ = this.FeedPlayerLoopAsync(PlayerToFeed, NowPlayingInformation, TimeLeft);
            //    return Task.CompletedTask;
        }

        public async Task<List<IzolabellaSong>> GetQueueAsync()
        {
            if (this.Queue == null || !this.Queue.Any())
            {
                Request<List<IzolabellaSong>> QueueReq = await this.Client.GetServerQueue();
                if (QueueReq.Success && QueueReq.Result.Any())
                {
                    this.Queue = QueueReq.Result;
                }
                return QueueReq.Success ? QueueReq.Result : this.Queue ?? new();
            }
            else if(this.Queue != null && this.Queue.Any())
            {
                IzolabellaSong First = this.Queue.First();
                this.Queue.RemoveAt(0);
                this.Queue.Add(First);
            }
            return new();
        }
    }
}
