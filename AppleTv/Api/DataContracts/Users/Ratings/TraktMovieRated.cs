
using AppleTv.Api.DataContracts.BaseModel;

namespace AppleTv.Api.DataContracts.Users.Ratings
{
    
    public class TraktMovieRated : TraktRated
    {
        public TraktMovie movie { get; set; }
    }
}