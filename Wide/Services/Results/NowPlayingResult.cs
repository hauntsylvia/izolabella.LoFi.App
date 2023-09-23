using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using izolabella.LoFi.Wide.Services.Enums;
using izolabella.Music.Structure.Music.Artists;
using izolabella.Music.Structure.Music.Songs;

namespace izolabella.LoFi.Wide.Services.Results
{
    public class NowPlayingResult
    {
        public NowPlayingResult(IzolabellaSong Playing, DateTime ExpectedEnd, IEnumerable<IzolabellaAuthor> Authors)
        {
            this.Authors = Authors;
            this.Playing = Playing;
            this.ExpectedEnd = ExpectedEnd.ToUniversalTime();
        }

        private NowPlayingResult()
        {
        }

        public static NowPlayingResult Zero => new();

        public NowPlayingStatus Status { get; }

        [MemberNotNullWhen(true, nameof(this.Authors))]
        [MemberNotNullWhen(true, nameof(this.Playing))]
        [MemberNotNullWhen(true, nameof(this.ExpectedEnd))]
        [MemberNotNullWhen(true, nameof(this.TimeLeft))]
        public bool Started => this.Status == NowPlayingStatus.Started;

        public IEnumerable<IzolabellaAuthor>? Authors { get; }

        public IzolabellaSong? Playing { get; }

        public DateTime? ExpectedEnd { get; }

        /// <summary>
        /// expected end - now
        /// </summary>
        public TimeSpan? TimeLeft => this.ExpectedEnd?.Subtract(DateTime.UtcNow);
    }
}