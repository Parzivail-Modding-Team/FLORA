using System.Runtime.Serialization;

namespace FLORA.FabricJson
{
    [DataContract]
    public class FabricModJson
    {
        [DataContract]
        public class Dependencies
        {
            [DataMember(Name = "minecraft")]
            public string Minecraft { get; set; }
        }

        [DataMember(Name = "schemaVersion")]
        public int SchemaVersion { get; set; }

        [DataMember(Name = "depends")]
        public Dependencies Depends { get; set; }
    }
}
