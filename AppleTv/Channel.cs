using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppleTv.Api;
using AppleTv.Helpers;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Channels;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Channels;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.MediaInfo;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Serialization;

namespace AppleTv
{
    public class Channel : IChannel, IHasCacheKey, IRequiresMediaInfoCallback
    {
        private IHttpClient HttpClient         { get; }
        private ILogger Logger                 { get; }
        private IJsonSerializer JsonSerializer { get; }
        private ILibraryManager LibraryManager { get; }
        private IUserManager UserManager { get; }
        private TraktApi TraktApi { get; }

        public Channel(IHttpClient httpClient, ILogManager logManager, IJsonSerializer jsonSerializer, ILibraryManager lib, TraktApi traktApi, IUserManager userManager)
        {
            HttpClient     = httpClient;
            Logger         = logManager.GetLogger(GetType().Name);
            JsonSerializer = jsonSerializer;
            LibraryManager = lib;
            TraktApi = traktApi;
            UserManager = userManager;
        }

        public string DataVersion => "12";

        public InternalChannelFeatures GetChannelFeatures()
        {
            return new InternalChannelFeatures
            {
                ContentTypes = new List<ChannelMediaContentType>
                {
                    ChannelMediaContentType.Movie
                },

                MediaTypes = new List<ChannelMediaType>
                {
                    ChannelMediaType.Video
                },

                SupportsContentDownloading = true,
                SupportsSortOrderToggle    = true,
            };
        }
        
        private async Task<ChannelItemResult> GetChannelItemsInternal()
        {
            var users = UserManager.Users;

            var channels = LibraryManager.GetItemList(new InternalItemsQuery()
            {
                Recursive = true,
                IncludeItemTypes = new []{ "Channel" }
            }).Where(x => !string.IsNullOrEmpty(x.Name)).ToList();

            var currentChannel = channels.First(x => x.Name == Plugin.Instance.PluginConfiguration.ChannelName).Id.ToString().Replace("-", string.Empty);
            
            if (!Plugin.Instance.PluginConfiguration.Enabled)
            {
                foreach (var user in users)
                {
                    user.Policy.EnableAllChannels = false;

                    var enabledChannels = user.Policy.EnabledChannels != null ? user.Policy.EnabledChannels.ToList() : new List<string>();
                    var disabledChannels = user.Policy.BlockedChannels != null ? user.Policy.BlockedChannels.ToList() : new List<string>();
                    
                    //Logger.Info("Enabled " + string.Join(",", enabledChannels.Select(x => x.ToString()).ToArray()));
                    //Logger.Info("Blocked " + string.Join(",", disabledChannels.Select(x => x.ToString()).ToArray()));

                    if (!disabledChannels.Contains(currentChannel))
                    {
                        disabledChannels.Add(currentChannel);
                        //Logger.Info("Added To Disabled " + currentChannel);
                    }

                    if (enabledChannels.Any())
                    {
                        if (enabledChannels.Contains(currentChannel))
                        {
                            enabledChannels.Remove(currentChannel);
                        }
                    }
                    else
                    {
                        foreach (var channel in channels.Where(x => x.Name != Plugin.Instance.PluginConfiguration.ChannelName))
                        {
                            enabledChannels.Add(channel.Id.ToString().Replace("-", string.Empty));
                        }
                    }

                    user.Policy.EnabledChannels = enabledChannels.ToArray();
                    user.Policy.BlockedChannels = disabledChannels.ToArray();
                    
                    //Logger.Info("Enabled " + string.Join(",", enabledChannels.Select(x => x.ToString()).ToArray()));
                    //Logger.Info("Blocked " + string.Join(",", disabledChannels.Select(x => x.ToString()).ToArray()));

                    UserManager.UpdateUser(user);
                }
            }
            else
            {
                foreach (var user in users)
                {
                    user.Policy.EnableAllChannels = false;

                    var enabledChannels = user.Policy.EnabledChannels != null ? user.Policy.EnabledChannels.ToList() : new List<string>();
                    var disabledChannels = user.Policy.BlockedChannels != null ? user.Policy.BlockedChannels.ToList() : new List<string>();
                    
                    //Logger.Info("Enabled " + string.Join(",", enabledChannels.Select(x => x.ToString()).ToArray()));
                    //Logger.Info("Blocked " + string.Join(",", disabledChannels.Select(x => x.ToString()).ToArray()));

                    if (disabledChannels.Contains(currentChannel))
                    {
                        disabledChannels.Remove(currentChannel);
                    }
                    
                    if (enabledChannels.Any())
                    {
                        if (enabledChannels.Contains(currentChannel))
                        {
                            continue;
                        }

                        enabledChannels.Add(currentChannel);
                    }
                    else
                    {
                        foreach (var channel in channels)
                        {
                            enabledChannels.Add(channel.Id.ToString().Replace("-", string.Empty));
                        }
                    }

                    user.Policy.EnabledChannels = enabledChannels.ToArray();
                    user.Policy.BlockedChannels = disabledChannels.ToArray();
                    
                    //Logger.Info("Enabled " + string.Join(",", enabledChannels.Select(x => x.ToString()).ToArray()));
                    //Logger.Info("Blocked " + string.Join(",", disabledChannels.Select(x => x.ToString()).ToArray()));

                    UserManager.UpdateUser(user);
                }
            }

            var newItems = new List<ChannelItemInfo>();

            if (Plugin.Instance.PluginConfiguration.Enabled == false)
            {
                return await Task.FromResult(new ChannelItemResult
                {
                    Items = newItems.ToList()
                }); 
            }
            
            if (string.IsNullOrEmpty(Plugin.Instance.PluginConfiguration.Pin) && string.IsNullOrEmpty(Plugin.Instance.PluginConfiguration.TraktUser.AccessToken))
            {
                return await Task.FromResult(new ChannelItemResult
                {
                    Items = newItems.ToList()
                }); 
            }
            
            var config = Plugin.Instance.Configuration;
            var channelLibraryItem = LibraryManager.GetItemList(new InternalItemsQuery
            {
                Name = config.ChannelName
            });
            
            // ReSharper disable once ComplexConditionExpression
            var ids = LibraryManager.GetInternalItemIds(new InternalItemsQuery
            {
                IncludeItemTypes = new[] {"Movie"}
            });

            var libraryItems = LibraryManager.GetInternalItemIds(new InternalItemsQuery
            {
                ParentIds = new[] {channelLibraryItem[0].InternalId}
            }).ToList();

            if (!string.IsNullOrEmpty(Plugin.Instance.PluginConfiguration.Pin))
            {
                Plugin.Instance.PluginConfiguration.TraktUser.PIN = Plugin.Instance.PluginConfiguration.Pin;
            }
            
            var listData = await TraktApi.GetTraktUserListItems(Plugin.Instance.PluginConfiguration.TraktUser,Plugin.Instance.PluginConfiguration.TraktListUserName, Plugin.Instance.PluginConfiguration.TraktListName, new CancellationToken());
            
            Logger.Info($"Count of items on list {listData.Count}");

             var mediaItems =
                LibraryManager.GetItemList(
                        new InternalItemsQuery
                        {
                            IncludeItemTypes = new[] {nameof(Movie)},
                            IsVirtualItem = false,
                            OrderBy = new[]
                            {
                                new ValueTuple<string, SortOrder>(ItemSortBy.SeriesSortName, SortOrder.Ascending),
                                new ValueTuple<string, SortOrder>(ItemSortBy.SortName, SortOrder.Ascending)
                            }
                        })
                    .ToList();
                
            Logger.Info($"Count of items in emby {mediaItems.Count}");
            
            foreach (var movie in mediaItems.OfType<Movie>())
            {
                //Logger.Info($"Movie {movie.Name}");
                
                if (libraryItems.Contains(movie.InternalId))
                {
                    continue;
                }
                
                var foundMovie = Match.FindMatch(movie, listData);

                if (foundMovie != null)
                {
                    //Logger.Info($"Movie {movie.Name} found");

                    var embyMove = LibraryManager.GetItemById(movie.Id);
                    if (embyMove != null)
                    {
                        Logger.Info($"Emby Movie {embyMove} found!");
                           // if(string.IsNullOrEmpty(embyMove.Path)){
                                var newMovie = new ChannelItemInfo
                                {
                                    Name = embyMove.Name,
                                    ImageUrl = embyMove.PrimaryImagePath,
                                    Id = embyMove.Id.ToString(),
                                    Type = ChannelItemType.Media,
                                    ContentType = ChannelMediaContentType.Movie,
                                    MediaType = ChannelMediaType.Video,
                                    IsLiveStream = false,
                                    MediaSources = new List<MediaSourceInfo>
                                    {
                                        new ChannelMediaInfo
                                            {Path = embyMove.Path, Protocol = MediaProtocol.File}.ToMediaSource()
                                    },
                                    OriginalTitle = embyMove.OriginalTitle
                                };
                                newItems.Add(newMovie);
                           // }
                    }
                }
                else
                {
                    Logger.Debug($"Movie {movie.Name} not found");
                }
            }
                
            Logger.Info($"Count of items in List to add {newItems.Count}");
            

            return await Task.FromResult(new ChannelItemResult
            {
                Items = newItems.ToList()
            });
        }

        public bool IsEnabledFor(string userId)
        {
            return true;
        }

        public async Task<ChannelItemResult> GetChannelItems(InternalChannelItemQuery query, CancellationToken cancellationToken)
        {
            return await GetChannelItemsInternal();
        }

        public async Task<DynamicImageResponse> GetChannelImage(ImageType type, CancellationToken cancellationToken)
        {
            switch (type)
            {
                case ImageType.Backdrop:
                    {
                        var path = GetType().Namespace + ".Images." + type.ToString().ToLower() + ".png";

                        return await Task.FromResult(new DynamicImageResponse
                        {
                            Format = ImageFormat.Png,
                            HasImage = true,
                            Stream = GetType().Assembly.GetManifestResourceStream(path)
                        });
                    }
                default:
                    throw new ArgumentException("Unsupported image type: " + type);

            }
        }

        public IEnumerable<ImageType> GetSupportedChannelImages()
        {
            return new List<ImageType> { ImageType.Primary, ImageType.Thumb };
        }

        public string Name => Plugin.Instance.PluginConfiguration.ChannelName;
        public string Description { get; private set; }
        public string HomePageUrl
        {
            get { return ""; }
        }

        public ChannelParentalRating ParentalRating
        {
            get { return ChannelParentalRating.GeneralAudience; }
        }

        public string GetCacheKey(string userId)
        {
            return Guid.NewGuid().ToString("N");
        }

        public Task<IEnumerable<MediaSourceInfo>> GetChannelItemMediaInfo(string id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}