using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pastel;

namespace FLORA
{
    class Lumberjack
    {
        private static readonly Stack<string> CategoryStack = new Stack<string>();
        private static int _indentLevel;

        public static void Log(object message) => Log(LogLevel.Trace, message);

        public static void Info(object message) => Log(LogLevel.Info, message);

        public static void Warn(object message) => Log(LogLevel.Warning, message);

        public static void Error(object message) => Log(LogLevel.Error, message);

        private static void Log(LogLevel level, object message)
        {
            var str = $"{"".PadLeft(_indentLevel, '\t')}{GetHeader(level)} {message}";
            var color = GetColor(level);

            Console.WriteLine(color.HasValue ? str.Pastel(color.Value) : str);
        }

        private static string GetHeader(LogLevel level)
        {
            string header;
            switch (level)
            {
                case LogLevel.Trace:
                    header = "@";
                    break;
                case LogLevel.Info:
                    header = "+";
                    break;
                case LogLevel.Warning:
                    header = "!";
                    break;
                case LogLevel.Error:
                    header = "*";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }

            return CategoryStack.Count != 0 ? $"[{header}/{CategoryStack.Peek()}]" : $"[{header}]";
        }

        private static Color? GetColor(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Trace:
                    return null;
                case LogLevel.Info:
                    return Color.ForestGreen;
                case LogLevel.Warning:
                    return Color.Yellow;
                case LogLevel.Error:
                    return Color.Red;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        public static void PushCategory(string category)
        {
            CategoryStack.Push(category);
        }

        public static void PopCategory()
        {
            CategoryStack.Pop();
        }

        public static void PushIndent()
        {
            _indentLevel++;
        }

        public static void PopIndent()
        {
            _indentLevel--;
        }
    }

    enum LogLevel
    {
        Trace,
        Info,
        Warning,
        Error
    }
}
