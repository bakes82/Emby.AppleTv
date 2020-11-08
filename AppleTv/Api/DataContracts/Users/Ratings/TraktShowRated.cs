
using AppleTv.Api.DataContracts.BaseModel;

namespace AppleTv.Api.DataContracts.Users.Ratings
{
    
    public class TraktShowRated : TraktRated
    {
        public TraktShow show { get; set; }
    }
}