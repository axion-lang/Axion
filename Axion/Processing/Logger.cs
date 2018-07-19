using System;
using Axion.Enums;

namespace Axion.Processing {
    internal static class Logger {
        internal static void LogWarning(string message,    ErrorOrigin errorOrigin,
                                        int    lnPos = -1, int         clPos = -1) {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"{errorOrigin:G} warning: {message}.");

            if (lnPos != -1 &&
                clPos != -1) {
                Console.WriteLine($"At line {lnPos}, column {clPos}.");
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
        }

        internal static void LogInfo(string message, ConsoleColor color = ConsoleColor.DarkCyan) {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        //internal static string ToBoxedString(string str)
        //{
        //    // │ ─ ╭ ╮ ╯ ╰
        //    // open box
        //    var sb = new StringBuilder("╭");
        //    for (int i = 0; i < str.Length + 2 /* 2 for spaces */; i++)
        //    {
        //        sb.Append("─");
        //    }
        //    sb.AppendLine("╮");

        //    // content
        //    string[] lines = str.Split(Environment.NewLine);
        //    for (int i = 0; i < lines.Length; i++)
        //    {
        //        sb.AppendLine($"│ {lines[i]} │");
        //    }

        //    // close box
        //    sb.AppendLine("╰");
        //    for (int i = 0; i < str.Length + 2 /* 2 for spaces */; i++)
        //    {
        //        sb.Append("─");
        //    }
        //    sb.AppendLine("╯");
        //    return sb.ToString();
        //}
    }
}