using System.Linq;
using FLORA.Fabric;
using FLORA.Mapping;

namespace FLORA.Interactive.Command
{
    [InteractiveCommandDesc("yarnver", "yarnver [game version]", "Displays all available Yarn mapping versions, optionally only for a specific game version.")]
    internal class YarnVersionsCommand : InteractiveCommand
    {
        /// <inheritdoc />
        public YarnVersionsCommand(string args) : base(args)
        {
        }

        /// <inheritdoc />
        public override void Run(MappingDatabase mappingDatabase)
        {
            var versions = YarnUtil.GetYarnVersions();
            if (Args != null) versions = versions.Where(version => YarnUtil.DoVersionsMatch(version, Args)).ToArray();

            var groups = versions.GroupBy(version => version.GameVersion);

            foreach (var group in groups)
            {
                Lumberjack.Log(group.Key);
                Lumberjack.PushIndent();

                foreach (var yarnVersion in group.OrderByDescending(version => version.Build))
                    Lumberjack.Log(yarnVersion.ToColorfulString());

                Lumberjack.PopIndent();
            }
        }

        /// <inheritdoc />
        public override void PrintAdvancedHelp()
        {
            Lumberjack.Log(GetCommandDescription().Usage);
            Lumberjack.Log("");
            Lumberjack.Log("The yarnver command is used to determine the yarn versions that are applicable to");
            Lumberjack.Log("the given game version. If no game version is provided, then all known yarn versions");
            Lumberjack.Log("are shown, grouped by their applicable game version. Next to the Yarn version string,");
            Lumberjack.Log("the maven ID is provided, and stable versions are denoted.");
            Lumberjack.Log("");
            Lumberjack.Log("Examples:");
            Lumberjack.Log("");
            Lumberjack.Info("yarnver");
            Lumberjack.Log("\tWill search all mappings");
            Lumberjack.Info("yarnver 1.15");
            Lumberjack.Log("\tWill search all mappings for 1.15.x");
            Lumberjack.Info("yarnver 1.15.1");
            Lumberjack.Log("\tWill search all mappings for 1.15.1");
        }
    }
}