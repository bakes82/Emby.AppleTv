using System.Collections.Generic;
using AppleTv.Api.DataContracts.BaseModel;

namespace AppleTv.Api.DataContracts.Sync.Collection
{
    public class TraktShowCollected : TraktShow
    {
        public List<TraktSeasonCollected> seasons { get; set; }

        public class TraktSeasonCollected
        {
            public int number { get; set; }

            public List<TraktEpisodeCollected> episodes { get; set; }
        }
    }
}