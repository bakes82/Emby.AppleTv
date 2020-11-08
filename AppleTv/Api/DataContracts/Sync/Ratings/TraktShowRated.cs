using System.Collections.Generic;
using AppleTv.Api.DataContracts.BaseModel;

namespace AppleTv.Api.DataContracts.Sync.Ratings
{
    public class TraktShowRated : TraktRated
    {
        public string title { get; set; }

        public int? year { get; set; }

        public TraktShowId ids { get; set; }

        public List<TraktSeasonRated> seasons { get; set; }

        public class TraktSeasonRated : TraktRated
        {
            public int? number { get; set; }

            public List<TraktEpisodeRated> episodes { get; set; }
        }
    }
}