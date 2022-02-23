using System.IO;
using FLORA.Mapping;

namespace FLORA.Interactive.Command
{
	[InteractiveCommandDesc("mapfile", "mapfile <filename>", "Maps the intermediate names in the given file.")]
	internal class MapFileCommand : InteractiveCommand
	{
		/// <inheritdoc />
		public MapFileCommand(string args) : base(args)
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

			var mapped = Mapper.MapString(mappingSource, File.ReadAllText(Args));
			File.WriteAllText(Path.Combine(Path.GetDirectoryName(Args), Path.GetFileNameWithoutExtension(Args) + "-mapped" + Path.GetExtension(Args)), mapped);
		}
	}
}