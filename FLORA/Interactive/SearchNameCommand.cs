using System;
using System.Collections.Generic;
using System.Drawing;
using Pastel;

namespace FLORA.Interactive
{
    [InteractiveCommandDesc("search", "search <mapping>", "Searches for the given mapped, intermediary, or official (classes only) name.")]
    internal class SearchNameCommand : InteractiveCommand
    {
        /// <inheritdoc />
        public SearchNameCommand(string args) : base(args)
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

            var mappings = mappingSource.Search(Args);

            foreach (var mapping in mappings)
            {
                if (mapping.ParentOfficialName != null)
                {
                    var parentMapping = mappingSource.GetClassByObf(mapping.ParentOfficialName);
                    Lumberjack.Log($"{mapping.GetMappingString()} (child of {parentMapping.GetMappingString()})");
                }
                else
                    Lumberjack.Log($"{mapping.GetMappingString()}");
            }
        }
    }
}