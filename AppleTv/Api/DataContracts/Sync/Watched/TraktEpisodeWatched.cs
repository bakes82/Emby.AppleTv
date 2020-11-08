using AppleTv.Api.DataContracts.BaseModel;

namespace AppleTv.Api.DataContracts.Sync.Watched
{
    public class TraktEpisodeWatched : TraktEpisode
    {
        public string watched_at { get; set; }
    }
}