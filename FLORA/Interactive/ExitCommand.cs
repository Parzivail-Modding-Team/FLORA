namespace FLORA.Interactive
{
    [InteractiveCommandDesc("exit", "exit", "Exits interactive mode")]
    internal class ExitCommand : InteractiveCommand
    {
        /// <inheritdoc />
        public ExitCommand(string args) : base(args)
        {
        }
    }
}