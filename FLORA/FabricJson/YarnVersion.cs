using System.Runtime.Serialization;
using LiteDB;

namespace FLORA
{
    [DataContract]
    public class YarnVersion
    {
        [BsonField("gameVersion")]
        [DataMember(Name = "gameVersion")]
        public string GameVersion { get; set; }
        
        [BsonField("separator")]
        [DataMember(Name = "separator")]
        public string Separator { get; set; }
        
        [BsonField("build")]
        [DataMember(Name = "build")]
        public int Build { get; set; }
        
        [BsonField("maven")]
        [DataMember(Name = "maven")]
        public string Maven { get; set; }
        
        [BsonField("version")]
        [DataMember(Name = "version")]
        public string Version { get; set; }
        
        [BsonField("stable")]
        [DataMember(Name = "stable")]
        public bool Stable { get; set; }

        [BsonField("table")]
        [IgnoreDataMember]
        public string TableName { get; set; }
    }
}
