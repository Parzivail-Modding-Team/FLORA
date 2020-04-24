using System;
using System.Linq;

namespace FLORA.Interactive
{
    [InteractiveCommandDesc("help", "help [command]", "Provides help on one or all commands")]
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

                PrintHelp(matchingCommand);
            }
            else
                foreach (var command in commands)
                    PrintHelp(command);
        }
    }
}