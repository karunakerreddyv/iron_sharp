﻿using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using IronSharp.Core;

namespace IronSharp.IronCache
{
    public class CacheClient
    {
        private readonly string _cacheName;
        private readonly IronCacheRestClient _client;

        public CacheClient(IronCacheRestClient client, string cacheName)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }
            if (string.IsNullOrEmpty(cacheName))
            {
                throw new ArgumentNullException("cacheName");
            }
            Contract.EndContractBlock();

            _client = client;
            _cacheName = cacheName;
        }

        public IValueSerializer ValueSerializer
        {
            get { return _client.Config.SharpConfig.ValueSerializer; }
        }

        /// <summary>
        ///     Delete all items in a cache. This cannot be undone.
        /// </summary>
        /// <remarks>
        ///     http://dev.iron.io/cache/reference/api/#clear_a_cache
        /// </remarks>
        public async Task<bool> Clear()
        {
            return await RestClient.Post<ResponseMsg>(_client.Config, string.Format("{0}/clear", CacheNameEndPoint())).HasExpectedMessage("Deleted.");
        }

        public async Task<bool> Delete(string key)
        {
            return await RestClient.Delete<ResponseMsg>(_client.Config, CacheItemEndPoint(key)).HasExpectedMessage("Deleted.");
        }

        /// <summary>
        ///     This call retrieves an item from the cache. The item will not be deleted.
        /// </summary>
        /// <param name="key"> The key the item is stored under in the cache. </param>
        /// <remarks>
        ///     http://dev.iron.io/cache/reference/api/#get_an_item_from_a_cache
        /// </remarks>
        public async Task<CacheItem> Get(string key)
        {
            RestResponse<CacheItem> response = await RestClient.Get<CacheItem>(_client.Config, CacheItemEndPoint(key));

            if (response.CanReadResult())
            {
                response.Result.Client = this;
            }

            return response;
        }

        /// <summary>
        ///     This call retrieves an item from the cache. The item will not be deleted.
        /// </summary>
        /// <param name="key"> The key the item is stored under in the cache. </param>
        /// <remarks>
        ///     http://dev.iron.io/cache/reference/api/#get_an_item_from_a_cache
        /// </remarks>
        public async Task<T> Get<T>(string key)
        {
            CacheItem item = await Get(key);

            if (IsDefaultValue(item))
            {
                return default(T);
            }

            return item.ReadValueAs<T>();
        }


        /// <summary>
        /// Gets the item from the cache, or initializes the cache item's value using the specififed <paramref name="valueFactory"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The cache item's key.</param>
        /// <param name="valueFactory">The method invoked to create the value</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public async Task<T> GetOrAdd<T>(string key, Func<T> valueFactory, CacheItemOptions options = null)
        {
            T item = await Get<T>(key);

            if (Equals(item, default(T)))
            {
                item = valueFactory();

                await Put(key, item, options);

                return item;
            }

            return item;
        }

        public async Task<T> GetOrAdd<T>(string key, Func<Task<T>> valueFactory, CacheItemOptions options = null)
        {
            T item = await Get<T>(key);

            if (Equals(item, default(T)))
            {
                item = await valueFactory();

                await Put(key, item, options);

                return item;
            }

            return item;
        }

        public async Task<CacheItem> GetOrAdd(string key, Func<CacheItem> valueFactory)
        {
            CacheItem item = await Get(key);

            if (IsDefaultValue(item))
            {
                item = valueFactory();
                await Put(key, item);
            }

            item.Client = this;

            return item;
        }

        public async Task<CacheItem> GetOrAdd(string key, Func<Task<CacheItem>> valueFactory)
        {
            CacheItem item = await Get(key);

            if (IsDefaultValue(item))
            {
                item = await valueFactory();
                await Put(key, item);
            }

            item.Client = this;

            return item;
        }

        /// <summary>
        ///     This call increments the numeric value of an item in the cache. The amount must be a number and attempting to
        ///     increment non-numeric values results in an error.
        ///     Negative amounts may be passed to decrement the value.
        ///     The increment is atomic, so concurrent increments will all be observed.
        /// </summary>
        /// <param name="key"> The key of the item to increment </param>
        /// <param name="amount"> The amount to increment the value, as an integer. If negative, the value will be decremented. </param>
        /// <remarks>
        ///     http://dev.iron.io/cache/reference/api/#increment_an_items_value
        /// </remarks>
        public async Task<CacheIncrementResult> Increment(string key, int amount = 1)
        {
            return await RestClient.Post<CacheIncrementResult>(_client.Config, string.Format("{0}/increment", CacheItemEndPoint(key)), new {amount});
        }

        /// <summary>
        ///     This call gets general information about a cache.
        /// </summary>
        /// <remarks>
        ///     http://dev.iron.io/cache/reference/api/#get_info_about_a_cache
        /// </remarks>
        public async Task<CacheInfo> Info()
        {
            return await RestClient.Get<CacheInfo>(_client.Config, CacheNameEndPoint());
        }

        public async Task<bool> Put(string key, object value, CacheItemOptions options = null)
        {
            return await Put(key, ValueSerializer.Generate(value), options);
        }

        public async Task<bool> Put(string key, int value, CacheItemOptions options = null)
        {
            return await Put(key, new CacheItem(value, options));
        }

        public async Task<bool> Put(string key, string value, CacheItemOptions options = null)
        {
            return await Put(key, new CacheItem(value, options));
        }

        /// <summary>
        ///     This call puts an item into a cache.
        /// </summary>
        /// <param name="key"> The key to store the item under in the cache. </param>
        /// <param name="item"> The item’s data </param>
        /// <remarks>
        ///     http://dev.iron.io/cache/reference/api/#put_an_item_into_a_cache
        /// </remarks>
        public async Task<bool> Put(string key, CacheItem item)
        {
            return await RestClient.Put<ResponseMsg>(_client.Config, CacheItemEndPoint(key), item).HasExpectedMessage("Stored.");
        }

        private static bool IsDefaultValue(CacheItem item)
        {
            return item == null || item.Value == null || string.IsNullOrEmpty(Convert.ToString(item.Value));
        }

        private string CacheItemEndPoint(string key)
        {
            return string.Format("{0}/items/{1}", CacheNameEndPoint(), key);
        }

        private string CacheNameEndPoint()
        {
            return string.Format("{0}/{1}", _client.EndPoint, _cacheName);
        }
    }
}