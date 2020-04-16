using System.Linq;
using LiteDB;

namespace FLORA
{
    internal class Mapping
    {
        [BsonField("i")]
        public string IntermediaryName { get; set; }

        [BsonField("m")]
        public string MappedName { get; set; }

        [BsonIgnore]
        public string IntermediaryShortName => IntermediaryName.Split('$').Last();

        [BsonIgnore]
        public string MappedShortName => MappedName.Split('$').Last();
    }
}