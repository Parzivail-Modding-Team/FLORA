using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace FLORA.Mapping
{
    class Mapper
    {
        /// <summary>
        /// Iterates over the files in an archive and maps the intermediary names in the java files
        /// </summary>
        /// <param name="mappingSource">The mapping provider</param>
        /// <param name="srcArchive">The archive to iterate through</param>
        /// <param name="destDir">The destination for the mapped files</param>
        public static void MapArchive(IMappingSource mappingSource, string srcArchive, string destDir)
        {
            // Keep the failed mappings to create a report at the end
            var failedMappings = new List<string>();

            destDir = Path.Combine(Program.BaseDirOutput, destDir);

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
                        File.WriteAllBytes(destFile, Encoding.UTF8.GetBytes((string) mappedContents));

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
            else
                Lumberjack.Log("No failed mappings");
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
    }
}
