using System;
using AppleTv.Api;
using MediaBrowser.Model.Plugins;

namespace AppleTv
{
    public class PluginConfig : BasePluginConfiguration
    {
        public string ChannelName { get; set; }
        public string TraktListUserName => "istoit";
        public string TraktListName => "apple-tv";
        
        public Guid Guid = new Guid("0BDD1CFA-0234-439B-82D6-D8DFE3075347"); // Also Needs Set In HTML File
        public string PluginName => "AppleTv";
        public string PluginDesc => "Movies from AppleTv+";
        public string Pin { get; set; }
        public bool Enabled { get; set; }
        public TraktUser TraktUser { get; set; }

        public PluginConfig()
        {
            TraktUser = new TraktUser();
            Enabled = true;
        }
    }
}