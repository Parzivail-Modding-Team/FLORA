using System;
using System.Linq;
using FLORA.Mapping;

namespace FLORA.Interactive.Command
{
    [InteractiveCommandDesc("help", "help [command]", "Provides quick reference for all commands or advanved usage for one command.")]
    internal class HelpCommand : InteractiveCommand
    {
        /// <inheritdoc />
        public HelpCommand(string args) : base(args)
        {
        }

        /// <inheritdoc />
        public override void Run(MappingDatabase mappingDatabase)
        {
            var commands = GetCommands();

            if (Args != null)
            {
                var matchingCommand = commands.FirstOrDefault(reference => reference.Description.Name == Args);
                if (matchingCommand == null)
                {
                    PrintErrorUsage();
                    Lumberjack.Error($"Cannot find help for \"{Args}\"");
                    return;
                }

                var command = (InteractiveCommand)Activator.CreateInstance(matchingCommand.CommandType, string.Empty);
                command.PrintAdvancedHelp();
            }
            else
                foreach (var command in commands.OrderBy(reference => reference.Description.Name))
                    PrintHelp(command.Description);
        }
    }
}