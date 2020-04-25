using System.Runtime.ConstrainedExecution;
using FLORA.Mapping;

namespace FLORA.Interactive.Command
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

        /// <inheritdoc />
        public override void PrintAdvancedHelp()
        {
            Lumberjack.Log(GetCommandDescription().Usage);
            Lumberjack.Log("");
            Lumberjack.Log("The children command is useful to determine what methods, fields, and classes are");
            Lumberjack.Log("defined explicitly as children of the given class. The class can be given in either");
            Lumberjack.Log("the mapped form, the intermediate form, or the official/obfuscated form. For example:");
            Lumberjack.Log("");
            Lumberjack.Info("children MatrixStack");
            Lumberjack.Info("children class_4587");
            Lumberjack.Info("children cyl");
            Lumberjack.Log("");
            Lumberjack.Log("are all equivelant. The children listed will only be children that are explicitly");
            Lumberjack.Log("defined in the target class. As such, fields, methods, and subclasses that are");
            Lumberjack.Log("inherited from a superclass will not be shown unless they are overridden.");
        }
    }
}