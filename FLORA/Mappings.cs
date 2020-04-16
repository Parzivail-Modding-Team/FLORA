using LiteDB;

namespace FLORA
{
    internal class Mappings
    {
        [BsonField("y")]
        public string YarnVersion { get; set; }

        [BsonField("t")]
        public string TableName { get; set; }
    }
}