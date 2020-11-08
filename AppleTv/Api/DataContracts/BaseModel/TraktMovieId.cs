
namespace AppleTv.Api.DataContracts.BaseModel
{
    public class TraktMovieId : TraktId
    {
        public string imdb { get; set; }

        public int? tmdb { get; set; }
    }
}