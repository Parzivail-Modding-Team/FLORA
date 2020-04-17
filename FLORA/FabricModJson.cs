using Newtonsoft.Json;

namespace FLORA
{
    public class FabricModJson
    {
        public class Dependencies
        {
            [JsonProperty("minecraft")]
            public string Minecraft { get; set; }
        }

        [JsonProperty("schemaVersion")]
        public int SchemaVersion { get; set; }

        [JsonProperty("depends")]
        public Dependencies Depends { get; set; }
    }
}
