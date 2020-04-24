using System.Drawing;
using System.Linq;
using LiteDB;
using Pastel;

namespace FLORA
{
    internal class Mapping
    {
        [BsonField("p")]
        public string ParentOfficialName { get; set; }

        [BsonField("o")]
        public string OfficialName { get; set; }

        [BsonField("i")]
        public string IntermediaryName { get; set; }

        [BsonField("m")]
        public string MappedName { get; set; }
        
        [BsonField("is")]
        public string IntermediaryShortName => IntermediaryName.Split('$').Last();
        
        [BsonField("ms")]
        public string MappedShortName => MappedName.Split('$').Last();

        [BsonIgnore]
        public string GetMappingString()
        {
            var official = OfficialName.Pastel(Color.DarkOrange);
            var intermediary = IntermediaryName.Pastel(Color.DodgerBlue);
            var mapped = MappedName.Pastel(Color.LightGreen);

            return $"{official}/{intermediary}/{mapped}";
        }
    }
}