using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using FLORA.Fabric;
using FLORA.Interactive.Command;
using FLORA.Mapping;
using Pastel;

namespace FLORA.Interactive
{
    internal class InteractiveMapper
    {
        private static MappingDatabase _mappingDatabase;
        private static IMappingSource _selectedMappingSource;

        public static void Run()
        {
            // Load local mapping database
            Lumberjack.Log("Loading mapping database...");
            _mappingDatabase = new MappingDatabase("mappings.db");

            var assy = Assembly.GetExecutingAssembly().GetName();
            Lumberjack.Info("Fabric Lightweight Obfuscation Remapping Assistant (FLORA)");
            Lumberjack.Info($"Version {assy.Version} - parzivail");
            Lumberjack.Info($"Source, issues and discussion: {"https://github.com/Parzivail-Modding-Team/FLORA".Pastel(Color.RoyalBlue)}");
            Lumberjack.Info("Interactive Mode - \"help\" for commands, \"exit\" to quit.");

            // Enter read-eval-print loop
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

        public static IMappingSource GetSelectedMappingSource()
        {
            if (_mappingDatabase.IsUsingLocalFile)
                return _mappingDatabase.GetMappingSet(null);
            if (_selectedMappingSource == null) Lumberjack.Error("No mapping source defined! Select one with \"mapsrc\"");
            return _selectedMappingSource;
        }

        public static void SetYarnVersion(YarnVersion version)
        {
            _mappingDatabase.ReleaseLocalFile();
            _selectedMappingSource = _mappingDatabase.GetMappingSet(version);
        }

        private static InteractiveCommand GetCommand()
        {
            Console.Write("> ");
            var line = Console.ReadLine();

            if (line == null)
                return null;

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
