using System.Collections.Generic;
using AppleTv.Api.DataContracts.BaseModel;
using AppleTv.Api.DataContracts.Sync.Collection;
using AppleTv.Api.DataContracts.Sync.Ratings;
using AppleTv.Api.DataContracts.Sync.Watched;

namespace AppleTv.Api.DataContracts.Sync
{
    public class TraktSync<TMovie, TShow, TEpisode>
    {
        public List<TMovie> movies { get; set; }

        public List<TShow> shows { get; set; }

        public List<TEpisode> episodes { get; set; }
    }

    public class TraktSyncRated : TraktSync<TraktMovieRated, TraktShowRated, TraktEpisodeRated>
    {
    }

    public class TraktSyncWatched : TraktSync<TraktMovieWatched, TraktShowWatched, TraktEpisodeWatched>
    {
    }

    public class TraktSyncCollected : TraktSync<TraktMovieCollected, TraktShowCollected, TraktEpisodeCollected>
    {
    }

    public class TraktSyncUncollected : TraktSync<TraktMovie, TraktShowCollected, TraktEpisodeCollected>
    {
    }
}