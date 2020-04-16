using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace FLORA
{
    public class YarnVersion
    {
        [JsonProperty("gameVersion")]
        public string GameVersion { get; set; }
        
        [JsonProperty("separator")]
        public string Separator { get; set; }
        
        [JsonProperty("build")]
        public int Build { get; set; }
        
        [JsonProperty("maven")]
        public string Maven { get; set; }
        
        [JsonProperty("version")]
        public string Version { get; set; }
        
        [JsonProperty("stable")]
        public bool Stable { get; set; }
    }
}
