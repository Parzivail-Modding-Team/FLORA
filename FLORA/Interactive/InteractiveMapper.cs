using System;
using System.Linq;

namespace FLORA.Interactive
{
    internal class InteractiveMapper
    {
        private static MappingDatabase _mappingDatabase;

        private static IMappingSource _mappingSource;

        public static void Run()
        {
            Lumberjack.Log("Running interactive mode");

            // Load local mapping database
            Lumberjack.Log("Loading mapping database...");
            _mappingDatabase = new MappingDatabase("mappings.db");

            InteractiveCommand command;
            do
            {
                command = GetCommand();
                if (command == null)
                    Lumberjack.Error("Unknown command");
                else
                    command.Run(_mappingDatabase);

            } while (!(command is ExitCommand));
        }

        public static IMappingSource GetMappingSource()
        {
            if (_mappingDatabase.IsUsingLocalFile)
                return _mappingDatabase.GetMappingSet(null);
            if (_mappingSource == null) Lumberjack.Error("No mapping source defined! Select one with \"mapsrc\"");
            return _mappingSource;
        }

        public static void SetYarnVersion(YarnVersion version)
        {
            _mappingSource = _mappingDatabase.GetMappingSet(version);
        }

        private static InteractiveCommand GetCommand()
        {
            Console.Write("> ");
            var line = Console.ReadLine();

            if (line == null)
                return null;

            // Get args from input line, respecting arguments with quotes
            var splitLine = line.Trim().Split(' ');
            var commandName = splitLine[0].ToLower();
            var commandArgs = splitLine.Length == 1 ? null : string.Join(" ", splitLine.Skip(1));

            var commands = InteractiveCommand.GetCommands();

            var commandTuple = commands.FirstOrDefault(t => t.Description.Name == commandName);

            if (commandTuple == null)
                return null;

            return (InteractiveCommand)Activator.CreateInstance(commandTuple.CommandType, commandArgs);
        }
    }
}
