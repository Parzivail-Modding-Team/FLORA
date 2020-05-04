using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CommandLine;
using FLORA.Fabric;
using FLORA.Interactive;
using FLORA.Mapping;
using Parser = CommandLine.Parser;

namespace FLORA
{
    internal class Program
    {
        private static MappingDatabase _mappingDatabase;

        public static string BaseDirConfig;
        public static string BaseDirOutput;

        public class Options
        {
            [Option('v', "mappingVersion", Required = false, HelpText = "Target Yarn mapping version. If omitted, will attempt to find the most recent compatible mapping from the mod metadata.")]
            public string MappingVersion { get; set; }

            [Option('t', "tiny", Required = false, HelpText = "Tiny v1 mapping file to use. If omitted, will continue with online version discovery (see mappingVersion).")]
            public string LocalTinyFile { get; set; }

            [Option('c', "config-dir", Required = false, HelpText = "The directory the configuration files and mapping database will be stored in. If omitted, the current directory will be used.", Default = "./")]
            public string ConfigDirBase { get; set; }

            [Option('o', "output-dir", Required = false, HelpText = "The directory the configuration files and mapping database will be stored in. If omitted, the current directory will be used.", Default = "./")]
            public string OutputDirBase { get; set; }

            [Value(0, Required = false, HelpText = "The input sources jar or zip file. If omitted, will run in interactive/REPL mode.")]
            public string InputArchive { get; set; }

            [Value(1, Required = false, HelpText = "The output directory of the mapped sources. If omitted, will create a directory based on the input filename.")]
            public string OutputDir { get; set; }
        }

        private static void Main(string[] args)
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;

            Parser.Default.ParseArguments<Options>(args).WithParsed(DoRemap);
        }

        /// <summary>
        /// Remaps the given jar with the specified mappings and output directory
        /// </summary>
        /// <param name="args">The command line flags for the conversion</param>
        /// <remarks>
        /// Most of the logic here is finding and loading mappings. The general flow is as
        /// follows: If you supplied a Tiny mapping file, use that. Otherwise, download the mapping
        /// metadata from the Yarn site. If you provided a yarn version, make sure it's valid and then
        /// use that one. If you didn't, try to pick the mapping based on the game version listed in
        /// the dependencies of the fabric.mod.json in the archive. If it doesn't exist, fail hard.
        ///
        /// With the mapping version in hand, see if we already cached the mappings in the database,
        /// and download them if we hadn't.
        /// </remarks>
        private static void DoRemap(Options args)
        {
            BaseDirConfig = args.ConfigDirBase;
            BaseDirOutput = args.OutputDirBase;

            if (args.InputArchive == null)
                InteractiveMapper.Run();

            // Quick fail if the input archive doesn't exist
            if (!File.Exists(args.InputArchive))
            {
                Lumberjack.Error($"Input file \"{args.InputArchive}\" does not exist!");
                Environment.Exit((int)ExitConditions.InputFileNotFound);
            }

            // Load local mapping database
            Lumberjack.Log("Loading mapping database...");
            _mappingDatabase = new MappingDatabase(Path.Combine(BaseDirConfig, "mappings.db"));

            // Try to pick a mapping version manually or automatically
            YarnVersion mappingVersion = null;

            if (args.LocalTinyFile != null)
            {
                // Try and load mappings from a specific local .tiny file (if provided)

                Lumberjack.Log("Loading mappings from local file...");
                if (!File.Exists(args.LocalTinyFile))
                {
                    Lumberjack.Error($"Tiny v1 file \"{args.LocalTinyFile}\" does not exist!");
                    Environment.Exit((int)ExitConditions.TinyFileNotFound);
                }

                mappingVersion = new YarnVersion
                {
                    Version = args.LocalTinyFile
                };
                _mappingDatabase.UseLocalFile(args.LocalTinyFile);
            }
            else
            {
                // Download all of the mapping versions from the Yarn API

                Lumberjack.Log("Fetching mapping versions from remote...");
                var yarnVersions = YarnUtil.GetYarnVersions();

                if (yarnVersions == null)
                {
                    // If we couldn't get them, try to use the ones we've already cached

                    Lumberjack.Warn("Working in offline mode. Only local Yarn mappings will be available.");
                    yarnVersions = _mappingDatabase.GetYarnVersions();
                }

                if (args.MappingVersion != null)
                {
                    if (yarnVersions.All(mapping => mapping.Version != args.MappingVersion))
                    {
                        // If a specific mapping version was specified that doesn't exist in the version listing, fail

                        Lumberjack.Error($"Could not find mapping version {args.MappingVersion}!");
                        Environment.Exit((int)ExitConditions.MappingVersionNotFound);
                    }

                    mappingVersion = yarnVersions.First(mapping => mapping.Version == args.MappingVersion);
                }

                Lumberjack.Log("Retrieving game version from jarfile...");
                var gameVersion = JarUtils.GetGameVersion(args.InputArchive);

                if (mappingVersion == null)
                {
                    // If no mapping version was specified in any form, select one based off of the game target version

                    if (gameVersion == null)
                    {
                        // If there's no Minecraft version dependency in the fabric.mod.json, fail

                        Lumberjack.Error(
                            "Could not find fabric.mod.json in archive! Try specifying the Yarn version manually.");
                        Environment.Exit((int)ExitConditions.FabricModJsonNotFound);
                    }

                    Lumberjack.Log($"Found game version: {gameVersion}");

                    // Get the latest mapping version compatible with the jar game version
                    mappingVersion =
                        yarnVersions.FirstOrDefault(yarnVersion => YarnUtil.DoVersionsMatch(yarnVersion, gameVersion));

                    if (mappingVersion == null)
                    {
                        // If no mapping was compatible, fail
                        // This should only really happen if we're in offline mode and only able to use locally cached mappings

                        Lumberjack.Error(
                            $"Could not automatically find a mapping compatible with game version {gameVersion}");
                        Environment.Exit((int)ExitConditions.MappingVersionNotFound);
                    }

                    Lumberjack.Log(
                        $"Latest mapping that matches game version from jarfile is {mappingVersion.Version}");
                }
                else if (gameVersion != null)
                {
                    // If a specific Yarn version was specified, check the game version anyway and spit out an error if they're probably not compatible

                    // We continue anyway because we assume if they're specifying mapping versions manually they probably know they're not compatible
                    // and have a reason to need that version

                    Lumberjack.Log($"Input jarfile has fabric.mod.json, specifying gave version {gameVersion}");
                    if (!YarnUtil.DoVersionsMatch(mappingVersion, gameVersion))
                        Lumberjack.Warn(
                            $"Game version {gameVersion} is not compatible with yarn version {mappingVersion.Version}, some mappings might not exist!");
                }

                if (!_mappingDatabase.HasMappingSet(mappingVersion))
                {
                    // If we don't have those mappings cached, download them and cache them

                    Lumberjack.Log("Fetching mappings from remote...");
                    var mappings = YarnUtil.GetYarnMappings(mappingVersion);

                    if (mappings != null)
                    {
                        Lumberjack.Log("Updating database...");
                        _mappingDatabase.CreateMappingSet(mappingVersion, mappings);
                    }
                    else
                    {
                        Lumberjack.Error($"Failed to load requested mappings {mappingVersion.Version}");
                        Environment.Exit((int)ExitConditions.MappingVersionNotFound);
                    }
                }
                else
                    Lumberjack.Log("Local database contains required mappings.");
            }

            // Load mappings and map the archive into the output directory
            Lumberjack.Log($"Loading {mappingVersion.Version} mappings...");

            var mappingSet = _mappingDatabase.GetMappingSet(mappingVersion);

            Lumberjack.PushCategory("Jar");
            Mapper.MapArchive(mappingSet, args.InputArchive, args.OutputDir ?? $"{Path.GetFileNameWithoutExtension(args.InputArchive)}-mapped");
            Lumberjack.PopCategory();

            Lumberjack.Log("Done. Press any key to continue.");
            Console.ReadKey();
            Environment.Exit((int)ExitConditions.Success);
        }
    }
}
