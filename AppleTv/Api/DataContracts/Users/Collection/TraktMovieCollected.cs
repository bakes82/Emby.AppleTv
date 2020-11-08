
using AppleTv.Api.DataContracts.BaseModel;

namespace AppleTv.Api.DataContracts.Users.Collection
{
    
    public class TraktMovieCollected
    {
        public string collected_at { get; set; }

        public TraktMetadata metadata { get; set; }

        public TraktMovie movie { get; set; }
    }
}