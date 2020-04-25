using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using FLORA.Mapping;

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

        public static void PrintHelp(InteractiveCommandDescAttribute commandDescription)
        {
            Lumberjack.Info(commandDescription.Usage);
            Lumberjack.Log($"\t{commandDescription.QuickHelp}");
        }

        public virtual void PrintAdvancedHelp()
        {
            PrintHelp(GetCommandDescription());
        }

        protected InteractiveCommandDescAttribute GetCommandDescription()
        {
            return GetType().GetCustomAttribute<InteractiveCommandDescAttribute>();
        }

        public virtual void PrintErrorUsage()
        {
            var commandDescription = GetCommandDescription();
            Lumberjack.Error($"Invalid arguments! Usage: {commandDescription.Usage}");
        }

        protected string[] GetUnquotedArgs()
        {
            if (Args == null)
                return Array.Empty<string>();
            return Regex.Matches(Args, "\"(?<arg>[^\"]+)\"|(?<arg>\\S+)").Cast<Match>()
                .Select(match => match.Groups["arg"].Value).ToArray();
        }
    }
}