using System.Collections.Generic;
using AppleTv.Api.DataContracts.BaseModel;

namespace AppleTv.Api.DataContracts.Sync.Watched
{
    public class TraktSeasonWatched : TraktSeason
    {
        public string watched_at { get; set; }

        public List<TraktEpisodeWatched> episodes { get; set; }
    }
}
