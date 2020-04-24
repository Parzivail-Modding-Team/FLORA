using System;
using System.Collections.Generic;
using System.Drawing;
using Pastel;

namespace FLORA.Interactive
{
    [InteractiveCommandDesc("children", "children <mapping>", "Searches for children of the class given by a mapped, intermediary, or official name.")]
    internal class GetChildrenCommand : InteractiveCommand
    {
        /// <inheritdoc />
        public GetChildrenCommand(string args) : base(args)
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

            var mappings = mappingSource.GetChildren(Args);

            foreach (var mapping in mappings) Lumberjack.Log(mapping.GetMappingString());
        }
    }
}