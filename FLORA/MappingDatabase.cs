using System.Collections.Generic;
using System.Linq;
using LiteDB;

namespace FLORA
{
    class MappingDatabase
    {
        private readonly LiteDatabase _mappingDatabase;

        public MappingDatabase(string filename)
        {
            _mappingDatabase = new LiteDatabase(filename);
        }

        public bool HasMappingSet(YarnVersion yarnVersion)
        {
            var mappingTable = _mappingDatabase.GetCollection<Mappings>("mappings");
            return mappingTable.Exists(map => map.YarnVersion == yarnVersion.Version);
        }

        public MappingSet GetMappingSet(YarnVersion yarnVersion)
        {
            if (!HasMappingSet(yarnVersion))
                return null;

            var mappingTable = _mappingDatabase.GetCollection<Mappings>("mappings");
            var mappingKey = mappingTable.FindOne(map => map.YarnVersion == yarnVersion.Version);
            var versionKey = mappingKey.TableName;

            return GetMappingSet(yarnVersion, versionKey);
        }

        private MappingSet GetMappingSet(YarnVersion yarnVersion, string tableName)
        {
            return new MappingSet(
                yarnVersion,
                _mappingDatabase.GetCollection<Mapping>($"{tableName}_classes"),
                _mappingDatabase.GetCollection<Mapping>($"{tableName}_fields"),
                _mappingDatabase.GetCollection<Mapping>($"{tableName}_methods")
            );
        }

        public MappingSet CreateMappingSet(YarnVersion yarnVersion, string[] mappingDescriptions)
        {
            var mappingTable = _mappingDatabase.GetCollection<Mappings>("mappings");

            var versionKey = AlphaOnlyStringEncoder.Encode(yarnVersion.Version);

            mappingTable.Insert(new Mappings
            {
                YarnVersion = yarnVersion.Version,
                TableName = versionKey
            });

            var mappingSet = GetMappingSet(yarnVersion, versionKey);

            var classes = new List<Mapping>();
            var fields = new List<Mapping>();
            var methods = new List<Mapping>();
            
            foreach (var mapping in mappingDescriptions)
            {
                var columns = mapping.Split('\t');

                switch (columns[0])
                {
                    case "CLASS":
                        // CLASS <tab> officialName <tab> intermediaryName <tab> mappedName
                        classes.Add(new Mapping
                        {
                            OfficialName = columns[1],
                            IntermediaryName = columns[2].Split('/').Last(),
                            MappedName = columns[3].Split('/').Last()
                        });
                        break;
                    case "FIELD":
                        // FIELD <tab> officialNameOfParent <tab> typeSignature <tab> officialName <tab> intermediaryName <tab> mappedName
                        fields.Add(new Mapping
                        {
                            ParentOfficialName = columns[1],
                            OfficialName = columns[3],
                            IntermediaryName = columns[4].Split('/').Last(),
                            MappedName = columns[5].Split('/').Last()
                        });
                        break;
                    case "METHOD":
                        // METHOD <tab> officialNameOfParent <tab> methodSignature <tab> officialName <tab> intermediaryName <tab> mappedName
                        methods.Add(new Mapping
                        {
                            ParentOfficialName = columns[1],
                            OfficialName = columns[3],
                            IntermediaryName = columns[4].Split('/').Last(),
                            MappedName = columns[5].Split('/').Last()
                        });
                        break;
                }
            }

            mappingSet.InsertMappings(classes, fields, methods);

            return mappingSet;
        }
    }
}
