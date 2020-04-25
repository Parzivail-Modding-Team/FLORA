using System.Drawing;
using FLORA.Mapping;
using Pastel;

namespace FLORA.Interactive.Command
{
    [InteractiveCommandDesc("mapstr", "mapstr <string>", "Maps the intermediate names in the given string.")]
    internal class MapStringCommand : InteractiveCommand
    {
        /// <inheritdoc />
        public MapStringCommand(string args) : base(args)
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

            var mappingSource = InteractiveMapper.GetSelectedMappingSource();
            if (mappingSource == null)
                return;

            var mapped = Mapper.MapString(mappingSource, Args);

            Lumberjack.Log("Mapped string:");
            Lumberjack.Log($"\t{mapped}".Pastel(Color.White));
        }
    }
}