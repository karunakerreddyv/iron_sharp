﻿using System.Threading;
using Newtonsoft.Json;

namespace IronSharp.Core
{
    public class IronClientConfig : IIronSharpConfig, IInspectable
    {
        private IronSharpConfig _sharpConfig;

        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("project_id")]
        public string ProjectId { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("api_version", DefaultValueHandling = DefaultValueHandling.Ignore )]
        public int? ApiVersion { get; set; }

        [JsonProperty("scheme")]
        public string Scheme { get; set; }

        [JsonProperty("port", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? Port { get; set; }

        [JsonProperty("sharp_config")]
        public IronSharpConfig SharpConfig
        {
            get { return LazyInitializer.EnsureInitialized(ref _sharpConfig, CreateDefaultIronSharpConfig); }
            set { _sharpConfig = value; }
        }

        private static IronSharpConfig CreateDefaultIronSharpConfig()
        {
            return new IronSharpConfig
            {
                BackoffFactor = 25
            };
        }
    }
}