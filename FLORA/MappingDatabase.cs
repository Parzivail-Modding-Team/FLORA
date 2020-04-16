using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using LiteDB;

namespace FLORA
{
    class MappingDatabase
    {
        private LiteDatabase _mappingDatabase;

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

            return GetMappingSet(versionKey);
        }

        private MappingSet GetMappingSet(string tableName)
        {
            var mappingSet = new MappingSet
            {
                Classes = _mappingDatabase.GetCollection<Mapping>($"{tableName}_classes"),
                Fields = _mappingDatabase.GetCollection<Mapping>($"{tableName}_fields"),
                Methods = _mappingDatabase.GetCollection<Mapping>($"{tableName}_methods")
            };

            mappingSet.Classes.EnsureIndex(mapping => mapping.IntermediaryName);
            mappingSet.Fields.EnsureIndex(mapping => mapping.IntermediaryName);
            mappingSet.Methods.EnsureIndex(mapping => mapping.IntermediaryName);

            return mappingSet;
        }

        public MappingSet CreateMappingSet(YarnVersion yarnVersion, string[] mappings)
        {
            var mappingTable = _mappingDatabase.GetCollection<Mappings>("mappings");

            var versionKey = AlphaOnlyStringEncoder.Encode(yarnVersion.Version);

            mappingTable.Insert(new Mappings
            {
                YarnVersion = yarnVersion.Version,
                TableName = versionKey
            });

            var mappingSet = GetMappingSet(versionKey);

            foreach (var mapping in mappings)
            {
                var columns = mapping.Split('\t');

                switch (columns[0])
                {
                    case "CLASS":
                        mappingSet.Classes.Insert(CreateMapping(columns));
                        break;
                    case "FIELD":
                        mappingSet.Fields.Insert(CreateMapping(columns));
                        break;
                    case "METHOD":
                        mappingSet.Methods.Insert(CreateMapping(columns));
                        break;
                }
            }

            return mappingSet;
        }

        private static Mapping CreateMapping(string[] columns) => new Mapping { IntermediaryName = columns[2], MappedName = columns[3] };
    }
}
