using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using LiteDB;
using Newtonsoft.Json;

namespace FLORA
{
    class Program
    {
        private static MappingDatabase _mappingDatabase;

        static void Main(string[] args)
        {
            Console.WriteLine("Loading mapping database...");
            _mappingDatabase = new MappingDatabase("mappings.db");

            Console.WriteLine("Fetching remote versions...");
            var yarnVersions = GetYarnVersions();
            var version = yarnVersions[0];

            Console.WriteLine($"Latest mapping is {version.Version}");

            if (!_mappingDatabase.HasMappingSet(version))
            {
                Console.WriteLine("Fetching remote mappings...");
                var mappings = GetYarnMappings(version);

                Console.WriteLine("Updating database...");
                _mappingDatabase.CreateMappingSet(version, mappings);
            }
            
            Console.WriteLine($"Loading {version.Version} mappings...");

            var mappingConversion = _mappingDatabase.GetMappingSet(version);

            Console.WriteLine("Done.");
            Console.ReadKey();
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
                return JsonConvert.DeserializeObject<YarnVersion[]>(wc.DownloadString("https://meta.fabricmc.net/v2/versions/yarn"));
        }
    }
}
