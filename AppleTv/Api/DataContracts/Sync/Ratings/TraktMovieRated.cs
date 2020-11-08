using AppleTv.Api.DataContracts.BaseModel;

namespace AppleTv.Api.DataContracts.Sync.Ratings
{
    public class TraktMovieRated : TraktRated
    {
        public string title { get; set; }

        public int? year { get; set; }

        public TraktMovieId ids { get; set; }
    }
}