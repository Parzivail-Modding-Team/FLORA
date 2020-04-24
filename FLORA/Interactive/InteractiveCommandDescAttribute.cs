using System;

namespace FLORA.Interactive
{
    internal class InteractiveCommandDescAttribute : Attribute
    {
        public string Name { get; }
        public string Usage { get; }
        public string Help { get; }

        public InteractiveCommandDescAttribute(string name, string usage, string help)
        {
            Name = name;
            Usage = usage;
            Help = help;
        }
    }
}