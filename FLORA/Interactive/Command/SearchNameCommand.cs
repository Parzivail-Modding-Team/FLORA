using FLORA.Mapping;

namespace FLORA.Interactive.Command
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

        /// <inheritdoc />
        public override void PrintAdvancedHelp()
        {
            Lumberjack.Log(GetCommandDescription().Usage);
            Lumberjack.Log("");
            Lumberjack.Log("The search command is used to find the mappings associated with the given named,");
            Lumberjack.Log("intermediary, or official/obfuscated mapping. Searching by official/obfuscated names");
            Lumberjack.Log("is only available for classes, as all classes' fields and methods first obfuscated");
            Lumberjack.Log("name is the same, and there's a good chance any given query will return thousands of");
            Lumberjack.Log("results.");
            Lumberjack.Log("");
            Lumberjack.Log("Examples:");
            Lumberjack.Log("");
            Lumberjack.Info("search CowEntity");
            Lumberjack.Info("search class_1430");
            Lumberjack.Info("search ath");
        }
    }
}