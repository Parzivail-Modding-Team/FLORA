using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FLORA.Fabric;

namespace FLORA
{
    class JarUtils
    {
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
    }
}
