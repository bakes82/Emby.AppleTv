
using AppleTv.Api.DataContracts.BaseModel;

namespace AppleTv.Api.DataContracts.Users.Watched
{
    
    public class TraktMovieWatched
    {
        public int plays { get; set; }

        public string last_watched_at { get; set; }

        public TraktMovie movie { get; set; }
    }
}