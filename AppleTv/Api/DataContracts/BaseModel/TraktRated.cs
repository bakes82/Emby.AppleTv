
namespace AppleTv.Api.DataContracts.BaseModel
{
    public abstract class TraktRated
    {
        public int? rating { get; set; }

        public string rated_at { get; set; }
    }
}