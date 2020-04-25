using System.Drawing;
using System.Runtime.Serialization;
using LiteDB;
using Pastel;

namespace FLORA.Fabric
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

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Version} ({Maven}{(Stable ? ", stable" : "")})";
        }

        public string ToColorfulString()
        {
            return $"{Version.Pastel(Color.Orange)} ({Maven}{(Stable ? ", stable".Pastel(Color.ForestGreen) : "")})";
        }
    }
}
