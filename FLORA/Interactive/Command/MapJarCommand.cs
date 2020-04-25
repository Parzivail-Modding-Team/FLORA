using System.IO;
using FLORA.Mapping;

namespace FLORA.Interactive.Command
{
    [InteractiveCommandDesc("mapjar", "mapjar <input filename> [output directory]", "Maps the intermediate names in the given decompiled jar and writes mapped files in the output directory.")]
    internal class MapJarCommand : InteractiveCommand
    {
        /// <inheritdoc />
        public MapJarCommand(string args) : base(args)
        {
        }

        /// <inheritdoc />
        public override void Run(MappingDatabase mappingDatabase)
        {
            var args = GetUnquotedArgs();

            if (args.Length < 1 || args.Length > 2)
            {
                PrintErrorUsage();
                return;
            }

            var input = args[0];
            var output = args.Length == 1 ? $"{Path.GetFileNameWithoutExtension(input)}-mapped" : args[1];

            if (!File.Exists(input))
            {
                Lumberjack.Error($"Input file \"{input}\" does not exist!");
                return;
            }

            var mappingSource = InteractiveMapper.GetSelectedMappingSource();
            if (mappingSource == null)
                return;

            Mapper.MapArchive(mappingSource, input, output);
        }

        /// <inheritdoc />
        public override void PrintAdvancedHelp()
        {
            Lumberjack.Log(GetCommandDescription().Usage);
            Lumberjack.Log("");
            Lumberjack.Log("The mapjar command is used to convert the intermediary mappings in all of the Java");
            Lumberjack.Log("files in a given jar/zip archive to their named counterparts. The input file must be");
            Lumberjack.Log("a \"source\" jar or a decompiled zip (as produced by jd-gui, for example), where the");
            Lumberjack.Log("contents are Java files and not class files. All of the contents of the jar will be");
            Lumberjack.Log("copied to the output directory, even if they were not modified by the mapping");
            Lumberjack.Log("procedure. If no output directory is specified, a directory based on the input filename");
            Lumberjack.Log("is used.");
        }
    }
}