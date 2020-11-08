using AppleTv.Api.DataContracts.BaseModel;

namespace AppleTv.Api.DataContracts.Scrobble
{
    public class TraktScrobbleMovie
    {
        public TraktMovie movie { get; set; }

        public float progress { get; set; }

        public string app_version { get; set; }

        public string app_date { get; set; }
    }
}