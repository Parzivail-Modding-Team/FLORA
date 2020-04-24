using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Pastel;

namespace FLORA.Interactive
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
    }
}