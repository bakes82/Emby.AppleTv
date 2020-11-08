using System.Collections.Generic;
using AppleTv.Api.DataContracts.BaseModel;

namespace AppleTv.Api.DataContracts.Sync.Watched
{
    public class TraktShowWatched : TraktShow
    {
        public string watched_at { get; set; }

        public List<TraktSeasonWatched> seasons { get; set; }
    }
}