using System.Runtime.Serialization;

namespace FLORA
{
    [DataContract]
    public class YarnVersion
    {
        [DataMember(Name = "gameVersion")]
        public string GameVersion { get; set; }
        
        [DataMember(Name = "separator")]
        public string Separator { get; set; }
        
        [DataMember(Name = "build")]
        public int Build { get; set; }
        
        [DataMember(Name = "maven")]
        public string Maven { get; set; }
        
        [DataMember(Name = "version")]
        public string Version { get; set; }
        
        [DataMember(Name = "stable")]
        public bool Stable { get; set; }
    }
}
