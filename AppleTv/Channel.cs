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
        
        private TraktApi TraktApi { get; }

        public Channel(IHttpClient httpClient, ILogManager logManager, IJsonSerializer jsonSerializer, ILibraryManager lib, TraktApi traktApi)
        {
            HttpClient     = httpClient;
            Logger         = logManager.GetLogger(GetType().Name);
            JsonSerializer = jsonSerializer;
            LibraryManager = lib;
            TraktApi = traktApi;
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
            var config = Plugin.Instance.Configuration;
            var channel = LibraryManager.GetItemList(new InternalItemsQuery()
            {
                Name = config.ChannelName
            });
            // ReSharper disable once ComplexConditionExpression
            var ids = LibraryManager.GetInternalItemIds(new InternalItemsQuery()
            {
                IncludeItemTypes = new[] {"Movie"},
                MinPremiereDate =  DateTimeOffset.Parse("1/1/"+DateTime.Now.Year),
            });

            var libraryItems = LibraryManager.GetInternalItemIds(new InternalItemsQuery()
            {
                ParentIds = new[] {channel[0].InternalId}
            }).ToList();

            if (!string.IsNullOrEmpty(Plugin.Instance.PluginConfiguration.Pin))
            {
                Plugin.Instance.PluginConfiguration.TraktUser.PIN = Plugin.Instance.PluginConfiguration.Pin;
            }
            
            var listData = await TraktApi.GetTraktUserListItems(Plugin.Instance.PluginConfiguration.TraktUser,Plugin.Instance.PluginConfiguration.TraktListUserName, Plugin.Instance.PluginConfiguration.TraktListName, new CancellationToken());
            
            Logger.Info($"Count of items on list {listData.Count}");

            // ReSharper disable once ComplexConditionExpression
            var sortedList = ids.Where(i => !libraryItems.Exists(id => id == i)).ToList();

            var items = sortedList.Select(id => LibraryManager.GetItemById(id))
                .Select(movie => new ChannelItemInfo
                {
                    Name          = movie.Name,
                    ImageUrl      = movie.PrimaryImagePath,
                    Id            = movie.Id.ToString(),
                    Type          = ChannelItemType.Media,
                    ContentType   = ChannelMediaContentType.Movie,
                    MediaType     = ChannelMediaType.Video,
                    IsLiveStream  = false,
                    MediaSources  = new List<MediaSourceInfo> {new ChannelMediaInfo {Path = movie.Path, Protocol = MediaProtocol.File}.ToMediaSource()},
                    OriginalTitle = movie.OriginalTitle
                });

            return await Task.FromResult(new ChannelItemResult
            {
                Items = items.ToList()
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
            return new List<ImageType>() { ImageType.Primary, ImageType.Thumb };
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

    /*public class Channel: IChannel, IHasCacheKey, IRequiresMediaInfoCallback
    {
        public Channel(IHttpClient httpClient, ILogManager logManager, IJsonSerializer jsonSerializer,
            ILibraryManager lib, TraktApi traktApi)
        {
            HttpClient = httpClient;
            Logger = logManager.GetLogger(GetType().Name);
            JsonSerializer = jsonSerializer;
            LibraryManager = lib;
            TraktApi = traktApi;
            Logger.Info("In CTOR");
        }
        
        private IHttpClient HttpClient { get; }
        private ILogger Logger { get; }
        private IJsonSerializer JsonSerializer { get; }
        private ILibraryManager LibraryManager { get; }
        private TraktApi TraktApi { get; }

        public string DataVersion => "12";

        public string HomePageUrl => "";

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
                SupportsSortOrderToggle = true
            };
        }

        public async Task<ChannelItemResult> GetChannelItems(InternalChannelItemQuery query,
            CancellationToken cancellationToken)
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
            return new List<ImageType> {ImageType.Primary, ImageType.Thumb};
        }

        public string Name => Plugin.Instance.PluginConfiguration.ChannelName;
        public string Description { get; private set; }

        public ChannelParentalRating ParentalRating => ChannelParentalRating.GeneralAudience;

        public string GetCacheKey(string userId)
        {
            return Guid.NewGuid().ToString("N");
        }

        public Task<IEnumerable<MediaSourceInfo>> GetChannelItemMediaInfo(string id,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task<ChannelItemResult> GetChannelItemsInternal()
        {
            var newItems = new List<ChannelItemInfo>();
            /*return await Task.FromResult(new ChannelItemResult
            {
                Items = newItems.ToList()
            });#1#
            var channel = LibraryManager.GetItemList(new InternalItemsQuery
            {
                Name = Name
            });
            
            var libraryItems = LibraryManager.GetInternalItemIds(new InternalItemsQuery
            {
                ParentIds = new[] {channel[0].InternalId}
            }).ToList();


            //var listData = client.Users.GetCustomListItemsAsync("movistapp", "horror").Result;
            var listData = await TraktApi.GetTraktUserListItems(Plugin.Instance.PluginConfiguration.TraktListUserName, Plugin.Instance.PluginConfiguration.TraktListName, new CancellationToken());
            
            Logger.Info($"Count of items on list {listData.Count}");
            //_logger.Info(_jsonSerializer.SerializeToString(data));

            var mediaItems =
                LibraryManager.GetItemList(
                        new InternalItemsQuery()
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
                        Logger.Info($"Emby Movie {embyMove}");
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
                    //Logger.Debug($"Movie {movie.Name} not found");
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
    }*/
}