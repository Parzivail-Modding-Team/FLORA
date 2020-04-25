using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text.RegularExpressions;

namespace FLORA.Fabric
{
    internal static class YarnUtil
    {
        public static string[] GetYarnMappings(YarnVersion version)
        {
            try
            {
                var lines = new List<string>();

                var mappingsTarball =
                    $"https://maven.fabricmc.net/net/fabricmc/yarn/{version.Version}/yarn-{version.Version}-tiny.gz";
                using (var client = new WebClient())
                using (var fileStream = new MemoryStream(client.DownloadData(mappingsTarball)))
                using (var gzStream = new GZipStream(fileStream, CompressionMode.Decompress))
                using (var sr = new StreamReader(gzStream))
                {
                    sr.ReadLine();

                    string line;
                    while ((line = sr.ReadLine()) != null) lines.Add(line);
                }

                return lines.ToArray();
            }
            catch (WebException e)
            {
                Lumberjack.Error($"Could not retrieve remote Yarn mappings: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                Lumberjack.Error($"Could not load remote Yarn mappings: {e.Message}! Please report this to the developer.");
                return null;
            }
        }

        public static YarnVersion[] GetYarnVersions()
        {
            try
            {
                using (var wc = new WebClient())
                    return wc.DownloadString("https://meta.fabricmc.net/v2/versions/yarn").ParseJson<YarnVersion[]>();
            }
            catch (WebException e)
            {
                Lumberjack.Error($"Could not retrieve remote Yarn versions: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                Lumberjack.Error($"Could not load remote Yarn versions: {e.Message}! Please report this to the developer.");
                return null;
            }
        }

        public static bool DoVersionsMatch(YarnVersion yarnVersion, string gameVersion)
        {
            var mappingVersion = yarnVersion.GameVersion;
            return mappingVersion == gameVersion || Regex.IsMatch(mappingVersion, Regex.Escape(gameVersion).Replace("x", "\\d+"));
        }
    }
}