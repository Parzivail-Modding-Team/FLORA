using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CommandLine;
using FLORA.FabricJson;
using FLORA.Interactive;
using Parser = CommandLine.Parser;

namespace FLORA
{
    internal class Program
    {
        private static MappingDatabase _mappingDatabase;

        public class Options
        {
            [Option('v', "mappingVersion", Required = false, HelpText = "Target Yarn mapping version. If omitted, will attempt to find the most recent compatible mapping from the mod metadata.")]
            public string MappingVersion { get; set; }

            [Option('t', "tiny", Required = false, HelpText = "Tiny v1 mapping file to use. If omitted, will continue with online version discovery (see mappingVersion).")]
            public string LocalTinyFile { get; set; }

            [Value(0, Required = false, HelpText = "The input sources jar or zip file. If omitted, will run in interactive/REPL mode")]
            public string InputArchive { get; set; }

            [Value(1, Required = false, HelpText = "The output directory of the mapped sources. If omitted, will create a directory based on the input filename")]
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
        private static void DoRemap(Options args)
        {
            if (args.InputArchive == null)
                InteractiveMapper.Run();

            // Quick fail if the input archive doesn't exist
            if (!File.Exists(args.InputArchive))
            {
                Lumberjack.Error($"Input file {args.InputArchive} does not exist!");
                Environment.Exit((int)ExitConditions.InputFileNotFound);
            }

            // Load local mapping database
            Lumberjack.Log("Loading mapping database...");
            _mappingDatabase = new MappingDatabase("mappings.db");

            // Try to pick a mapping version manually or automatically
            YarnVersion mappingVersion = null;

            if (args.LocalTinyFile != null)
            {
                // Try and load mappings from a specific local .tiny file (if provided)

                Lumberjack.Log("Loading mappings from local file...");
                if (!File.Exists(args.LocalTinyFile))
                {
                    Lumberjack.Error($"Tiny v1 file {args.LocalTinyFile} does not exist!");
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
                var gameVersion = GetGameVersion(args.InputArchive);

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
            MapArchive(mappingSet, args.InputArchive, args.OutputDir ?? $"{Path.GetFileNameWithoutExtension(args.InputArchive)}-mapped");
            Lumberjack.PopCategory();

            Lumberjack.Log("Done. Press any key to continue.");
            Console.ReadKey();
            Environment.Exit((int)ExitConditions.Success);
        }

        /// <summary>
        /// Check if the mapping for the regex match is valid
        /// </summary>
        /// <param name="mapping">The class, field or method mapping from the database</param>
        /// <param name="match">The regex match for the keyword</param>
        /// <param name="failedMappings">The list to insert failed mappings into</param>
        /// <returns></returns>
        private static string CheckMatch(Mapping mapping, Capture match, List<string> failedMappings = null)
        {
            if (mapping != null)
            {
                Lumberjack.Log($"{match.Value}\t-> {mapping.GetMappingString()}");
                return mapping.MappedShortName;
            }

            Lumberjack.Warn($"{match.Value}\t-> ??? (No mapping)");
            failedMappings?.Add(match.Value);
            return match.Value;
        }

        /// <summary>
        /// Attempts to extract the game version from the archive
        /// </summary>
        /// <param name="archiveFilename">The name of the archive to extract the version from</param>
        /// <returns>The game version, or null if one could not be found</returns>
        public static string GetGameVersion(string archiveFilename)
        {
            using (var srcZip = ZipFile.OpenRead(archiveFilename))
            {
                var metadataFile = srcZip.GetEntry("fabric.mod.json");
                if (metadataFile == null) return null;

                FabricModJson meta;
                using (var reader = new StreamReader(metadataFile.Open()))
                    meta = reader.ReadToEnd().ParseJson<FabricModJson>();

                if (meta.SchemaVersion == 1)
                    return meta.Depends.Minecraft;
            }

            return null;
        }

        /// <summary>
        /// Iterates over the files in an archive and maps the intermediary names in the java files
        /// </summary>
        /// <param name="mappingSource">The mapping provider</param>
        /// <param name="srcArchive">The archive to iterate through</param>
        /// <param name="destDir">The destination for the mapped files</param>
        private static void MapArchive(IMappingSource mappingSource, string srcArchive, string destDir)
        {
            // Keep the failed mappings to create a report at the end
            var failedMappings = new List<string>();

            using (var srcZip = ZipFile.OpenRead(srcArchive))
            {
                foreach (var srcEntry in srcZip.Entries)
                {
                    // Directories are entries too, skip them
                    if (srcEntry.FullName.EndsWith("/"))
                        continue;

                    var destFile = $"{destDir}/{srcEntry.FullName}";

                    var fileDir = Path.GetDirectoryName(destFile);
                    Debug.Assert(fileDir != null);

                    // Make sure the output directory structure exists
                    Directory.CreateDirectory(fileDir);

                    if (Path.GetExtension(srcEntry.Name) == ".java")
                    {
                        // Read and map java files
                        Lumberjack.Log(srcEntry.FullName);

                        string file;
                        using (var reader = new StreamReader(srcEntry.Open())) file = reader.ReadToEnd();

                        Lumberjack.PushIndent();

                        var mappedContents = MapString(mappingSource, file, failedMappings);
                        File.WriteAllBytes(destFile, Encoding.UTF8.GetBytes(mappedContents));

                        Lumberjack.PopIndent();
                    }
                    else
                    {
                        // Copy all files directly that aren't java files 
                        using (var reader = srcEntry.Open())
                        using (var writer = File.Open(destFile, FileMode.Create))
                            reader.CopyTo(writer);
                    }
                }
            }

            // Report all of the failed mappings
            if (failedMappings.Count > 0)
            {
                Lumberjack.Warn("Failed mappings:");

                Lumberjack.PushIndent();
                Lumberjack.PushCategory("Failed");

                foreach (var failedMapping in failedMappings) Lumberjack.Warn(failedMapping);

                Lumberjack.PopCategory();
                Lumberjack.PopIndent();
            }
        }

        /// <summary>
        /// Maps the input string contents
        /// </summary>
        /// <param name="mappingSource">The mapping provider</param>
        /// <param name="file">The contents to map</param>
        /// <param name="failedMappings">The list of failed mappings to generate a report for</param>
        /// <returns>The mapped string contents</returns>
        public static string MapString(IMappingSource mappingSource, string file, List<string> failedMappings = null)
        {
            Lumberjack.PushCategory("Mapper");

            // Create regexes for matching intermediary names
            var rgxClass = new Regex("class_\\d+", RegexOptions.Compiled);
            var rgxField = new Regex("field_\\d+", RegexOptions.Compiled);
            var rgxMethod = new Regex("method_\\d+", RegexOptions.Compiled);

            // Replace all of the intermediary names
            file = rgxClass.Replace(file, match => CheckMatch(mappingSource.GetClassByInt(match.Value), match, failedMappings));
            file = rgxField.Replace(file, match => CheckMatch(mappingSource.GetFieldByInt(match.Value), match, failedMappings));
            file = rgxMethod.Replace(file,
                match => CheckMatch(mappingSource.GetMethodByInt(match.Value), match, failedMappings));

            Lumberjack.PopCategory();
            return file;
        }
    }
}
