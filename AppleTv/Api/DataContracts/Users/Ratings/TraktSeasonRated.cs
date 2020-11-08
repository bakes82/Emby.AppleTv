
using AppleTv.Api.DataContracts.BaseModel;

namespace AppleTv.Api.DataContracts.Users.Ratings
{
    
    public class TraktSeasonRated : TraktRated
    {
        public TraktSeason season { get; set; }
    }
}