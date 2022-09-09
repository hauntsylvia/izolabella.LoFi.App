using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using izolabella.LoFi.Wide.Services.Results;
using izolabella.Music.Structure.Clients;
using izolabella.Music.Structure.Music.Artists;
using izolabella.Music.Structure.Music.Songs;
using izolabella.Music.Structure.Players;
using izolabella.Music.Structure.Requests;

namespace izolabella.LoFi.Wide.Services.Implementations;

public class GenericMusicService
{
    public GenericMusicService(Func<NowPlayingResult, int, MusicPlayer> GetNextMusicPlayer, MainPage Reference)
    {
        this.GetNextMusicPlayer = GetNextMusicPlayer;
        this.Reference = Reference;
        this.Reference.OnVolumeChanged += this.VolumeChangedAsync;
        this.Reference.Client.OnDisconnect += this.DisconnectedAsync;
        this.Reference.Client.OnReconnect += this.ReconnectedAsync;
        this.Client = Reference.Client;
        this.NextSongRequested += this.NewSongAsync;
    }

    private async void ReconnectedAsync()
    {
        await this.SongPlayingTask;
        this.SongPlayingTaskCancellationToken = new();
        await this.StartAsync();
    }

    private async void DisconnectedAsync()
    {
        this.SongPlayingTaskCancellationToken.Cancel();
        await this.SongPlayingTask;
    }

    public Task StopAsync()
    {
        this.SongPlayingTaskCancellationToken.Cancel();
        this.LastMusicPlayer?.SetVolume(0f);
        this.LastMusicPlayer?.Dispose();
        return Task.CompletedTask;
    }

    public Task StartAsync()
    {
        Task StartTask = this.StartPlayingAsync();
        return Task.CompletedTask;
    }

    public delegate Task NextSongRequestedHandler(bool FirstSong, NowPlayingResult NowPlayingInformation);
    public event NextSongRequestedHandler NextSongRequested;

    public delegate Task BufferReloadHandler();
    public event BufferReloadHandler? BufferReloaded;

    public Func<NowPlayingResult, int, MusicPlayer> GetNextMusicPlayer { get; }

    public MainPage Reference { get; }

    public MusicPlayer? LastMusicPlayer { get; private set; }

    public IzolabellaLoFiClient Client { get; }

    private List<IzolabellaSong>? Queue { get; set; }

    public int BufferSize { get; } = 192000 * 3;

    private int Index { get; set; } = 0;

    public Task SongPlayingTask { get; private set; } = Task.CompletedTask;

    public CancellationTokenSource SongPlayingTaskCancellationToken { get; private set; } = new();

    private async Task VolumeChangedAsync(float NewVol)
    {
        if(this.LastMusicPlayer != null)
        {
            await this.LastMusicPlayer.SetVolume(NewVol);
        }
    }

    private async Task NewSongAsync(bool FirstSong, NowPlayingResult NowPlayingInformation)
    {
        float Volume = this.LastMusicPlayer?.LastVolume ?? 0f;
        if (!FirstSong)
        {
            this.SongPlayingTaskCancellationToken.Cancel();
            await this.SongPlayingTask;
            this.LastMusicPlayer?.Dispose();
        }
        this.LastMusicPlayer = this.GetNextMusicPlayer.Invoke(NowPlayingInformation, this.BufferSize);
        await this.LastMusicPlayer.SetVolume(Volume);
        await this.LastMusicPlayer.StartAsync();
        this.Index = 0;
        this.SongPlayingTaskCancellationToken = new();
        this.SongPlayingTask = this.FeedPlayerLoopAsync(this.LastMusicPlayer, NowPlayingInformation, DateTime.MinValue, null);
    }

    private async Task<NowPlayingResult> StartPlayingAsync()
    {
        List<IzolabellaSong> Queue = await this.GetQueueAsync();
        if(Queue.Any())
        {
            Request<List<IzolabellaAuthor>> ReqAuthors = await this.Client.GetSongAuthorsAsync(Queue.First().Id);
            List<IzolabellaAuthor> Authors = ReqAuthors.Success ? ReqAuthors.Result : new();
            NowPlayingResult Result = new(Queue.First(), DateTime.UtcNow.Add(Queue.First().GetTimeFromByteLength(Queue.First().FileInformation.LengthInBytes)), Authors);
            this.NextSongRequested?.Invoke(true, Result);
            return Result;
        }
        else
        {
            return NowPlayingResult.Zero;
        }
    }

    private async Task FeedPlayerLoopAsync(MusicPlayer PlayerToFeed, NowPlayingResult NowPlayingInformation, DateTime EndsAt, Request<byte[]>? SongData, bool Consumed = false)
    {
        Task BufferInvoke = this.BufferReloaded?.Invoke() ?? Task.CompletedTask;
        TimeSpan TrueTimeLeft = EndsAt.Subtract(DateTime.UtcNow);
        TrueTimeLeft = TrueTimeLeft < TimeSpan.Zero ? TimeSpan.Zero : TrueTimeLeft;
        if(NowPlayingInformation.Started && TrueTimeLeft <= TimeSpan.FromMilliseconds(5))
        {
            int LengthOfSongBytes = NowPlayingInformation.Playing.FileInformation.LengthInBytes;
            if(this.Index >= LengthOfSongBytes)
            {
                this.NextSongRequested?.Invoke(false, await this.StartPlayingAsync());
                return;
            }
            SongData ??= await this.Client.GetBytesAsync(NowPlayingInformation.Playing.Id, this.Index, this.BufferSize);
            if (SongData != null && SongData.Success && !this.SongPlayingTaskCancellationToken.Token.IsCancellationRequested)
            {
                this.Index += SongData.Result.Length;
                await PlayerToFeed.FeedBytesAsync(SongData.Result);
                Consumed = true;
                EndsAt = DateTime.UtcNow.Add(NowPlayingInformation.Playing.GetTimeFromByteLength(SongData.Result.LongLength));
                SongData = null;
            }
        }
        if(NowPlayingInformation.Started)
        {
            TimeSpan WaitTick = TrueTimeLeft >= TimeSpan.FromMilliseconds(100) ? TrueTimeLeft.Subtract(TimeSpan.FromMilliseconds(100)) : TrueTimeLeft;
            await Task.Delay(!Consumed ? TimeSpan.Zero : WaitTick);
            SongData = !Consumed || SongData == null ? await this.Client.GetBytesAsync(NowPlayingInformation.Playing.Id, this.Index, this.BufferSize) : SongData;
        }
        if(!this.SongPlayingTaskCancellationToken.Token.IsCancellationRequested)
        {
            this.SongPlayingTask = this.FeedPlayerLoopAsync(PlayerToFeed, NowPlayingInformation, EndsAt, SongData, Consumed);

        }

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
            return this.Queue;
        }
        return new();
    }
}
