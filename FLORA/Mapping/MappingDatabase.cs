using System.Collections.Generic;
using System.IO;
using System.Linq;
using FLORA.Fabric;
using LiteDB;

namespace FLORA.Mapping
{
    class MappingDatabase
    {
        private readonly LiteDatabase _mappingDatabase;
        private LocalMappingSource _localMappingSource;

        public bool IsUsingLocalFile => _localMappingSource != null;

        public MappingDatabase(string filename)
        {
            _mappingDatabase = new LiteDatabase(filename);
        }

        public bool HasMappingSet(YarnVersion requestedVersion)
        {
            if (IsUsingLocalFile)
                return true;

            var mappingTable = _mappingDatabase.GetCollection<YarnVersion>("yarn_versions");
            return mappingTable.Exists(map => map.Version == requestedVersion.Version);
        }

        public IMappingSource GetMappingSet(YarnVersion requestedVersion)
        {
            if (IsUsingLocalFile)
                return _localMappingSource;

            if (!HasMappingSet(requestedVersion))
                return null;

            var mappingTable = _mappingDatabase.GetCollection<YarnVersion>("yarn_versions");
            var mappingKey = mappingTable.FindOne(map => map.Version == requestedVersion.Version);
            var versionKey = mappingKey.TableName;

            return GetDatabaseMapSet(requestedVersion, versionKey);
        }

        private IMappingSource GetDatabaseMapSet(YarnVersion requestedVersion, string tableName)
        {
            return new DatabaseMappingSource(
                requestedVersion,
                _mappingDatabase.GetCollection<Mapping>($"{tableName}_classes"),
                _mappingDatabase.GetCollection<Mapping>($"{tableName}_fields"),
                _mappingDatabase.GetCollection<Mapping>($"{tableName}_methods")
            );
        }

        public IMappingSource CreateMappingSet(YarnVersion requestedVersion, string[] mappingDescriptions)
        {
            var mappingTable = _mappingDatabase.GetCollection<YarnVersion>("yarn_versions");
            requestedVersion.TableName = AlphaOnlyStringEncoder.Encode(requestedVersion.Version);
            mappingTable.Insert(requestedVersion);

            var mappingSet = GetMappingSet(requestedVersion);
            var (classes, fields, methods) = ParseMappings(mappingDescriptions);
            mappingSet.InsertMappings(classes, fields, methods);

            return mappingSet;
        }

        private static (List<Mapping>, List<Mapping>, List<Mapping>) ParseMappings(string[] mappingDescriptions)
        {
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

            return (classes, fields, methods);
        }

        public YarnVersion[] GetYarnVersions()
        {
            var mappingTable = _mappingDatabase.GetCollection<YarnVersion>("yarn_versions");
            return mappingTable.FindAll().ToArray();
        }

        public void UseLocalFile(string filename)
        {
            _localMappingSource = new LocalMappingSource();
            var (classes, fields, methods) = ParseMappings(File.ReadAllLines(filename));
            _localMappingSource.InsertMappings(classes, fields, methods);
        }

        public void ReleaseLocalFile()
        {
            _localMappingSource = null;
        }
    }
}
