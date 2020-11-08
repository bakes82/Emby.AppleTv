using AppleTv.Api.DataContracts.BaseModel;

namespace AppleTv.Api.DataContracts.Sync.Ratings
{
    public class TraktEpisodeRated : TraktRated
    {
        public int? number { get; set; }

        public TraktEpisodeId ids { get; set; }
    }
}