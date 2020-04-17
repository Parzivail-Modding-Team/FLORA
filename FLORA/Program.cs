using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using CommandLine;
using FLORA.FabricJson;

namespace FLORA
{
    internal class Program
    {
        private static MappingDatabase _mappingDatabase;

        public class Options
        {
            [Option('v', "mappingVersion", Required = false, HelpText = "Target Yarn mapping version. If omitted, will attempt to find the most recent compatible mapping from the mod metadata.")]
            public string MappingVersion { get; set; }

            [Value(0, Required = true, HelpText = "The input sources jar or zip file")]
            public string Input { get; set; }

            [Value(1, Required = false, HelpText = "The output directory of the mapped sources. If omitted, will create a directory based on the input filename")]
            public string Output { get; set; }
        }

        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(DoRemap);
        }

        private static void DoRemap(Options args)
        {
            if (!File.Exists(args.Input))
            {
                Console.WriteLine($"Input file {args.Input} does not exist!");
                Environment.Exit(1);
            }

            Console.WriteLine("Loading mapping database...");
            _mappingDatabase = new MappingDatabase("mappings.db");

            Console.WriteLine("Fetching mapping versions from remote...");
            var yarnVersions = GetYarnVersions();

            if (args.MappingVersion != null && yarnVersions.All(mapping => mapping.Version != args.MappingVersion))
            {
                Console.WriteLine($"Could not find mapping version {args.MappingVersion}!");
                Environment.Exit(2);
            }

            var mappingVersion = args.MappingVersion == null
                ? null
                : yarnVersions.First(mapping => mapping.Version == args.MappingVersion);

            if (mappingVersion == null)
            {
                Console.WriteLine("Retrieving game version from jarfile...");
                var gameVersion = GetGameVersion(args.Input);

                if (gameVersion == null)
                {
                    Console.WriteLine("Could not find fabric.mod.json in archive! Try specifying the Yarn version manually.");
                    Environment.Exit(3);
                }

                Console.WriteLine($"Found game version: {gameVersion}");
                mappingVersion = yarnVersions.First(yarnVersion => DoVersionsMatch(yarnVersion, gameVersion));

                Console.WriteLine($"Latest mapping that matches game version from jarfile is {mappingVersion.Version}");
            }
            else
            {
                var gameVersion = GetGameVersion(args.Input);

                if (gameVersion != null)
                {
                    Console.WriteLine($"Input jarfile has fabric.mod.json, specifying gave version {gameVersion}");
                    if (!DoVersionsMatch(mappingVersion, gameVersion))
                        Console.WriteLine($"Game version {gameVersion} is not compatible with yarn version {mappingVersion.Version}, some mappings might not exist!");
                }
            }

            if (!_mappingDatabase.HasMappingSet(mappingVersion))
            {
                Console.WriteLine("Fetching mappings from remote...");
                var mappings = GetYarnMappings(mappingVersion);

                Console.WriteLine("Updating database...");
                _mappingDatabase.CreateMappingSet(mappingVersion, mappings);
            }
            else
                Console.WriteLine("Local database contains required mappings");

            Console.WriteLine($"Loading {mappingVersion.Version} mappings...");

            var mappingSet = _mappingDatabase.GetMappingSet(mappingVersion);

            var destDir = args.Output ?? $"{Path.GetFileNameWithoutExtension(args.Input)}-mapped";

            MapArchive(mappingSet, args.Input, destDir);

            Console.WriteLine("Done. Press any key to continue.");
            Console.ReadKey();
            Environment.Exit(0);
        }

        private static string CheckMatch(Mapping mapping, Match match)
        {
            if (mapping != null)
            {
                Console.WriteLine($"\t[+] Mapped {match.Value}\t-> {mapping.MappedName}");
                return mapping.MappedShortName;
            }

            Console.WriteLine($"\t[!] No mapping for {match.Value}");
            return match.Value;
        }

        public static string GetGameVersion(string filename)
        {
            using (var srcZip = ZipFile.OpenRead(filename))
            {
                var metadataFile = srcZip.GetEntry("fabric.mod.json");

                if (metadataFile != null)
                {
                    FabricModJson meta;

                    using (var reader = new StreamReader(metadataFile.Open()))
                        meta = reader.ReadToEnd().ParseJson<FabricModJson>();

                    if (meta.SchemaVersion == 1)
                        return meta.Depends.Minecraft;
                }
            }

            return null;
        }

        private static void MapArchive(MappingSet mappingSet, string src, string destDir)
        {
            var rgxClass = new Regex("class_\\d+", RegexOptions.Compiled);
            var rgxField = new Regex("field_\\d+", RegexOptions.Compiled);
            var rgxMethod = new Regex("method_\\d+", RegexOptions.Compiled);

            using (var srcZip = ZipFile.OpenRead(src))
            {
                foreach (var srcEntry in srcZip.Entries)
                {
                    if (srcEntry.FullName.EndsWith("/"))
                        continue;

                    var destFile = $"{destDir}/{srcEntry.FullName}";

                    Directory.CreateDirectory(Path.GetDirectoryName(destFile));

                    if (Path.GetExtension(srcEntry.Name) == ".java")
                    {
                        Console.WriteLine($"[*] {srcEntry.FullName}");

                        string file;
                        using (var reader = new StreamReader(srcEntry.Open()))
                        {
                            file = reader.ReadToEnd();

                            file = rgxClass.Replace(file, match => CheckMatch(mappingSet.GetNamedClass(match.Value), match));
                            file = rgxField.Replace(file, match => CheckMatch(mappingSet.GetNamedField(match.Value), match));
                            file = rgxMethod.Replace(file, match => CheckMatch(mappingSet.GetNamedMethod(match.Value), match));
                        }

                        File.WriteAllBytes(destFile, Encoding.UTF8.GetBytes(file));
                    }
                    else
                    {
                        using (var reader = srcEntry.Open())
                        using (var writer = File.Open(destFile, FileMode.Create))
                            reader.CopyTo(writer);
                    }
                }
            }
        }

        private static bool DoVersionsMatch(YarnVersion yarnVersion, string mcVersion)
        {
            var mappingVersion = yarnVersion.GameVersion;
            return mappingVersion == mcVersion || Regex.IsMatch(mappingVersion, Regex.Escape(mcVersion).Replace("x", "\\d+"));
        }

        public static string[] GetYarnMappings(YarnVersion version)
        {
            var lines = new List<string>();

            var mappingsTarball = $"https://maven.fabricmc.net/net/fabricmc/yarn/{version.Version}/yarn-{version.Version}-tiny.gz";
            using (var client = new WebClient())
            using (var fileStream = new MemoryStream(client.DownloadData(mappingsTarball)))
            using (var gzStream = new GZipStream(fileStream, CompressionMode.Decompress))
            using (var sr = new StreamReader(gzStream))
            {
                sr.ReadLine();

                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            return lines.ToArray();
        }

        private static YarnVersion[] GetYarnVersions()
        {
            using (var wc = new WebClient())
                return wc.DownloadString("https://meta.fabricmc.net/v2/versions/yarn").ParseJson<YarnVersion[]>();
        }
    }
}
