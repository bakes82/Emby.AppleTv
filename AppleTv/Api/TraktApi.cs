using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AppleTv.Api.DataContracts;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;

namespace AppleTv.Api
{
    public class TraktApi
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ILogger _logger;
        private readonly IHttpClient _httpClient;

        public TraktApi(IJsonSerializer jsonSerializer, ILogger logger, IHttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonSerializer = jsonSerializer;
            _logger = logger;
        }


        public async Task RefreshUserAuth(TraktUser traktUser, CancellationToken cancellationToken)
        {
            var data = new TraktUserTokenRequest
            {
                client_id = TraktUris.Id,
                client_secret = TraktUris.Secret,
                redirect_uri = "urn:ietf:wg:oauth:2.0:oob"
            };

            if (!string.IsNullOrWhiteSpace(traktUser.PIN))
            {
                data.code = traktUser.PIN;
                data.grant_type = "authorization_code";
            }
            else if (!string.IsNullOrWhiteSpace(traktUser.RefreshToken))
            {
                data.refresh_token = traktUser.RefreshToken;
                data.grant_type = "refresh_token";
            }
            else
            {
                _logger.Error("Tried to reauthenticate with Trakt, but neither PIN nor refreshToken was available");
            }

            TraktUserToken userToken;
            using (var response = await PostToTrakt(TraktUris.Token, data, null, cancellationToken).ConfigureAwait(false))
            {
                userToken = await _jsonSerializer.DeserializeFromStreamAsync<TraktUserToken>(response).ConfigureAwait(false);
            }

            if (userToken != null)
            {
                traktUser.AccessToken = userToken.access_token;
                traktUser.RefreshToken = userToken.refresh_token;
                traktUser.PIN = null;
                traktUser.AccessTokenExpiration = DateTimeOffset.Now.AddMonths(2);
                Plugin.Instance.PluginConfiguration.Pin = null;
                Plugin.Instance.SaveConfiguration();
            }
        }

        private Task<Stream> GetFromTrakt(string url, TraktUser traktUser, CancellationToken cancellationToken)
        {
            return GetFromTrakt(url, cancellationToken, traktUser);
        }

        private async Task<Stream> GetFromTrakt(string url, CancellationToken cancellationToken, TraktUser traktUser)
        {
            var options = GetHttpRequestOptions();
            options.Url = url;
            options.CancellationToken = cancellationToken;

            if (traktUser != null)
            {
                await SetRequestHeaders(options, traktUser, cancellationToken).ConfigureAwait(false);
            }

            await Plugin.Instance.TraktResourcePool.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                return await Retry(async () => await _httpClient.Get(options).ConfigureAwait(false)).ConfigureAwait(false);
            }
            finally
            {
                Plugin.Instance.TraktResourcePool.Release();
            }
        }

        private Task<Stream> PostToTrakt(string url, object data, TraktUser traktUser, CancellationToken cancellationToken)
        {
            return PostToTrakt(url, data, cancellationToken, traktUser);
        }

        /// <summary>
        ///     Posts data to url, authenticating with <see cref="TraktUser"/>.
        /// </summary>
        /// <param name="traktUser">If null, authentication headers not added.</param>
        private async Task<Stream> PostToTrakt(string url, object data, CancellationToken cancellationToken,
            TraktUser traktUser)
        {
            var requestContent = data == null ? string.Empty : _jsonSerializer.SerializeToString(data);
            if (traktUser != null && traktUser.ExtraLogging) _logger.Debug("POST " + requestContent);
            var options = GetHttpRequestOptions();
            options.Url = url;
            options.CancellationToken = cancellationToken;
            options.RequestContent = requestContent.AsMemory();

            if (traktUser != null)
            {
                await SetRequestHeaders(options, traktUser, cancellationToken).ConfigureAwait(false);
            }

            await Plugin.Instance.TraktResourcePool.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                var retryResponse = await Retry(async () => await _httpClient.Post(options).ConfigureAwait(false)).ConfigureAwait(false);
                return retryResponse.Content;
            }
            finally
            {
                Plugin.Instance.TraktResourcePool.Release();
            }
        }

        private async Task<T> Retry<T>(Func<Task<T>> function)
        {
            try
            {
                return await function().ConfigureAwait(false);
            }
            catch { }
            await Task.Delay(500).ConfigureAwait(false);
            try
            {
                return await function().ConfigureAwait(false);
            }
            catch { }
            await Task.Delay(500).ConfigureAwait(false);
            return await function().ConfigureAwait(false);
        }

        private HttpRequestOptions GetHttpRequestOptions()
        {
            var options = new HttpRequestOptions
            {
                RequestContentType = "application/json",
                TimeoutMs = 120000,
                LogErrorResponseBody = false,
                LogRequest = true,
                BufferContent = false,
                EnableHttpCompression = false,
                EnableKeepAlive = false
            };
            options.RequestHeaders.Add("trakt-api-version", "2");
            options.RequestHeaders.Add("trakt-api-key", TraktUris.Id);
            return options;
        }

        private async Task SetRequestHeaders(HttpRequestOptions options, TraktUser traktUser, CancellationToken cancellationToken)
        {

            if (DateTimeOffset.Now > traktUser.AccessTokenExpiration)
            {
                traktUser.AccessToken = "";
            }
            if (string.IsNullOrEmpty(traktUser.AccessToken) || !string.IsNullOrEmpty(traktUser.PIN))
            {
                await RefreshUserAuth(traktUser, cancellationToken).ConfigureAwait(false);
            }
            if (!string.IsNullOrEmpty(traktUser.AccessToken))
            {
                options.RequestHeaders.Add("Authorization", "Bearer " + traktUser.AccessToken);
            }

        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="traktUser"></param>
        /// <returns></returns>
        public async Task<List<TraktListsItem>> GetTraktUserListItems(TraktUser traktUser, string listUser, string listName, CancellationToken cancellationToken)
        {
            var listUrl = string.Format(TraktUris.UserLists, listUser, listName);
            
            using (var response = await GetFromTrakt(listUrl, traktUser, cancellationToken).ConfigureAwait(false))
            {
                return await _jsonSerializer.DeserializeFromStreamAsync<List<TraktListsItem>>(response).ConfigureAwait(false);
            }
        }
    }
}