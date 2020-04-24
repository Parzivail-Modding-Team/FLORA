using System;
using System.Linq;
using System.Reflection;

namespace FLORA.Interactive
{
    internal class InteractiveCommand
    {
        protected string Args;

        public InteractiveCommand(string args)
        {
            Args = args;
        }

        public static CommandReference[] GetCommands()
        {
            var classes = Assembly.GetAssembly(typeof(InteractiveMapper)).GetTypes();
            var attributes = classes.Select(type => new CommandReference(type, type.GetCustomAttribute<InteractiveCommandDescAttribute>()));
            return attributes.Where(t => t.Description != null).ToArray();
        }

        public virtual void Run(MappingDatabase mappingDatabase)
        {

        }

        public static void PrintHelp(CommandReference command)
        {
            Lumberjack.Log(command.Description.Usage);
            Lumberjack.Log($"\t{command.Description.Help}");
        }

        public void PrintErrorUsage()
        {
            var commandType = GetType();
            var commands = GetCommands();
            var commandTuple = commands.FirstOrDefault(t => t.CommandType == commandType);

            if (commandTuple == null)
                throw new ArgumentException(nameof(commandType));
            
            Lumberjack.Error($"Invalid arguments! Usage: {commandTuple.Description.Usage}");
        }
    }
}