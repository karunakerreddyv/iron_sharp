﻿using System.Collections.Generic;
using System.Threading;
using IronSharp.Core;

namespace IronSharp.IronMQ
{
    public class Client
    {
        private readonly IronClientConfig _config;

        private Client(IronClientConfig config)
        {
            _config = LazyInitializer.EnsureInitialized(ref config);

            if (string.IsNullOrEmpty(Config.Host))
            {
                Config.Host = CloudHosts.DEFAULT;
            }

            if (config.Version == default (int))
            {
                config.Version = 1;
            }
        }

        public IronClientConfig Config
        {
            get { return _config; }
        }

        public string EndPoint
        {
            get { return "/projects/{Project ID}/queues"; }
        }

        public static Client @New(string projectId = null, string token = null, string host = null)
        {
            return New(new IronClientConfig
            {
                Host = host,
                ProjectId = projectId,
                Token = token
            });
        }

        public static Client @New(IronClientConfig config = null)
        {
            return new Client(config);
        }

        public QueueClient Queue(string name)
        {
            return new QueueClient(this, name);
        }

        /// <summary>
        /// Get a list of all queues in a project. 
        /// By default, 30 queues are listed at a time. 
        /// To see more, use the page parameter or the per_page parameter. 
        /// Up to 100 queues may be listed on a single page.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public IEnumerable<QueueInfo> Queues(PagingFilter filter = null)
        {
            return RestClient.Get<IEnumerable<QueueInfo>>(_config, EndPoint, filter).Result;
        }
    }
}