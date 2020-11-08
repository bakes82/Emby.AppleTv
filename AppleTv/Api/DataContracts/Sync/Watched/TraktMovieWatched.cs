using AppleTv.Api.DataContracts.BaseModel;

namespace AppleTv.Api.DataContracts.Sync.Watched
{
    public class TraktMovieWatched : TraktMovie
    {
        public string watched_at { get; set; }
    }
}