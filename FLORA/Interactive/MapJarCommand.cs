using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Pastel;

namespace FLORA.Interactive
{
    [InteractiveCommandDesc("mapjar", "mapjar <input filename> [output directory]", "Maps the intermediate names in the given decompiled jar and writes mapped files in the output directory. If output directory is omitted, a directory based on the input filename is used.")]
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
    }
}