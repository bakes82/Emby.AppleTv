using System;
using AppleTv.Api.DataContracts.BaseModel;
using MediaBrowser.Controller.Entities;

namespace AppleTv.Api.DataContracts
{
    public class TraktListsItem
    {
        public int rank { get; set; } 
        public int id { get; set; } 
        public DateTime listedat { get; set; } 
        public object notes { get; set; } 
        public string type { get; set; } 
        public TraktMovie movie { get; set; } 
        public BaseItem EmbyMovie { get; set; }
    }
}