using System;
using System.IO;
using System.Linq;

namespace FLORA.Interactive
{
    [InteractiveCommandDesc("mapsrc", "mapsrc <filename or yarn version string>", "Selects a mapping source.")]
    internal class MapSourceCommand : InteractiveCommand
    {
        /// <inheritdoc />
        public MapSourceCommand(string args) : base(args)
        {
        }

        /// <inheritdoc />
        public override void Run(MappingDatabase mappingDatabase)
        {
            if (Args == null)
            {
                PrintErrorUsage();
                return;
            }

            if (Args.EndsWith(".tiny"))
            {
                if (!File.Exists(Args))
                {
                    Lumberjack.Error($"Mapping file \"{Args}\" could not be found!");
                    return;
                }
                
                mappingDatabase.UseLocalFile(Args);

                Lumberjack.Log($"Using mappings from \"{Args}\"");
            }
            else
                LoadRemoteMappings(mappingDatabase, Args);
        }

        private static void LoadRemoteMappings(MappingDatabase mappingDatabase, string version)
        {
            Lumberjack.Log("Fetching mapping versions from remote...");
            var yarnVersions = YarnUtil.GetYarnVersions();

            if (yarnVersions == null)
            {
                // If we couldn't get them, try to use the ones we've already cached

                Lumberjack.Warn("Working in offline mode. Only local Yarn mappings will be available.");
                yarnVersions = mappingDatabase.GetYarnVersions();
            }

            if (yarnVersions.All(mapping => mapping.Version != version))
            {
                // If a specific mapping version was specified that doesn't exist in the version listing, fail

                Lumberjack.Error($"Could not find mapping version {version}!");
                return;
            }

            var mappingVersion = yarnVersions.First(mapping => mapping.Version == version);

            if (!mappingDatabase.HasMappingSet(mappingVersion))
            {
                // If we don't have those mappings cached, download them and cache them

                Lumberjack.Log("Fetching mappings from remote...");
                var mappings = YarnUtil.GetYarnMappings(mappingVersion);

                if (mappings != null)
                {
                    Lumberjack.Log("Updating database...");
                    mappingDatabase.CreateMappingSet(mappingVersion, mappings);
                }
                else
                {
                    Lumberjack.Error($"Failed to load requested mappings {mappingVersion.Version}");
                    Environment.Exit((int)ExitConditions.MappingVersionNotFound);
                }
            }
            else
                Lumberjack.Log("Local database contains required mappings.");

            InteractiveMapper.SetYarnVersion(mappingVersion);
            
            Lumberjack.Log($"Using mappings from yarn {version}");
        }
    }
}