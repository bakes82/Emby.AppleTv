
using AppleTv.Api.DataContracts.BaseModel;

namespace AppleTv.Api.DataContracts.Users.Ratings
{
    
    public class TraktEpisodeRated : TraktRated
    {
        public TraktEpisode episode { get; set; }
    }
}