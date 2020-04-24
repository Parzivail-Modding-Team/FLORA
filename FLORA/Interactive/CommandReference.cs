using System;

namespace FLORA.Interactive
{
    internal class CommandReference
    {
        public Type CommandType { get; }
        public InteractiveCommandDescAttribute Description { get; }

        public CommandReference(Type commandType, InteractiveCommandDescAttribute description)
        {
            CommandType = commandType;
            Description = description;
        }
    }
}