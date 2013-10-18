﻿using System;
using IronSharp.Core;
using Newtonsoft.Json;

namespace IronSharp.IronCache
{
    public class CacheClient
    {
        private readonly string _cacheName;
        private readonly Client _client;

        public CacheClient(Client client, string cacheName)
        {
            _client = client;
            _cacheName = cacheName;
        }

        /// <summary>
        /// Delete all items in a cache. This cannot be undone.
        /// </summary>
        /// <remarks>
        /// http://dev.iron.io/cache/reference/api/#clear_a_cache
        /// </remarks>
        public bool Clear()
        {
            return RestClient.Post<ResponseMsg>(_client.Config, string.Format("{0}/clear", CacheNameEndPoint())).HasExpectedMessage("Deleted.");
        }

        public bool Delete(string key)
        {
            return RestClient.Delete<ResponseMsg>(_client.Config, CacheItemEndPoint(key)).HasExpectedMessage("Deleted.");
        }

        /// <summary>
        /// This call retrieves an item from the cache. The item will not be deleted.
        /// </summary>
        /// <param name="key"> The key the item is stored under in the cache. </param>
        /// <remarks>
        /// http://dev.iron.io/cache/reference/api/#get_an_item_from_a_cache
        /// </remarks>
        public CacheItem Get(string key)
        {
            return RestClient.Get<CacheItem>(_client.Config, CacheItemEndPoint(key));
        }

        public T Get<T>(string key, JsonSerializerSettings settings = null)
        {
            CacheItem item = Get(key);

            if (item == null || string.IsNullOrEmpty(item.Value))
            {
                return default(T);
            }

            return item.ReadValueAs<T>(settings);
        }

        public T GetOrAdd<T>(string key, Func<T> valueFactory, CacheItemOptions options = null, JsonSerializerSettings settings = null)
        {
            var item = Get<T>(key, settings);

            if (Equals(item, default(T)))
            {
                item = valueFactory();
                Put(key, item, options, settings);
            }

            return item;
        }

        public CacheItem GetOrAdd(string key, Func<CacheItem> valueFactory)
        {
            CacheItem item = Get(key);

            if (item == null || string.IsNullOrEmpty(item.Value))
            {
                item = valueFactory();
                Put(key, item);
            }

            return item;
        }

        /// <summary>
        /// This call increments the numeric value of an item in the cache. The amount must be a number and attempting to increment non-numeric values results in an error.
        /// Negative amounts may be passed to decrement the value.
        /// The increment is atomic, so concurrent increments will all be observed.
        /// </summary>
        /// <param name="key"> The key of the item to increment </param>
        /// <param name="amount"> The amount to increment the value, as an integer. If negative, the value will be decremented. </param>
        /// <remarks>
        /// http://dev.iron.io/cache/reference/api/#increment_an_items_value
        /// </remarks>
        public CacheIncrementResult Increment(string key, int amount)
        {
            return RestClient.Post<CacheIncrementResult>(_client.Config, string.Format("{0}/increment", CacheItemEndPoint(key)), new {amount});
        }

        /// <summary>
        /// This call gets general information about a cache.
        /// </summary>
        /// <remarks>
        /// http://dev.iron.io/cache/reference/api/#get_info_about_a_cache
        /// </remarks>
        public CacheInfo Info()
        {
            return RestClient.Get<CacheInfo>(_client.Config, CacheNameEndPoint());
        }

        public bool Put(string key, object value, CacheItemOptions options = null, JsonSerializerSettings settings = null)
        {
            return Put(key, new CacheItem(value, options, settings));
        }

        public bool Put(string key, string value, CacheItemOptions options = null)
        {
            return Put(key, new CacheItem(value, options));
        }

        /// <summary>
        /// This call puts an item into a cache.
        /// </summary>
        /// <param name="key"> The key to store the item under in the cache. </param>
        /// <param name="item"> The item’s data </param>
        /// <remarks>
        /// http://dev.iron.io/cache/reference/api/#put_an_item_into_a_cache
        /// </remarks>
        public bool Put(string key, CacheItem item)
        {
            return RestClient.Put<ResponseMsg>(_client.Config, CacheItemEndPoint(key), item).HasExpectedMessage("Stored.");
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