using System;

namespace FLORA.Interactive
{
    internal class InteractiveCommandDescAttribute : Attribute
    {
        public string Name { get; }
        public string Usage { get; }
        public string QuickHelp { get; }

        public InteractiveCommandDescAttribute(string name, string usage, string quickHelp)
        {
            Name = name;
            Usage = usage;
            QuickHelp = quickHelp;
        }
    }
}