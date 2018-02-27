using System;
using System.IO;
using System.Linq;
using Axion.Tokens;

namespace Axion
{
	internal static class Program
	{
		private static string ScriptFileName;
		private static string[] ScriptLines;
		public const string AnyKeyToClose = "Press any key to close app.";

		private static void Main(string[] args)
		{
			if (args.Length < 1 || !File.Exists(args[0]))
			{
				LogError("Input file doesn't exists", true);
			}
			ScriptFileName = args[0];
			ScriptLines = File.ReadAllLines(ScriptFileName);
			LogInfo("Tokens list generation...");
			Tokenize();
			LogInfo("Syntax tree generation...");
			Parse();
			Console.WriteLine("Ready. " + AnyKeyToClose);
			Console.ReadKey();
		}

		private static void Tokenize()
		{
			Lexer.Tokenize(ScriptLines);
			var fileName = ScriptFileName + ".lex";
			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}
			File.WriteAllText(fileName, string.Join(",\r\n", Lexer.Tokens.Select(token => token.ToString(0))));
		}

		private static void Parse()
		{
			var parser = new Parser(Lexer.Tokens, TokenType.Newline);
			parser.Parse();
			parser.SaveFile(ScriptFileName + ".tree");
		}

		public static void LogError(string message, bool critical = false, int lineIndex = -1, int charIndex = -1)
		{
			Console.ForegroundColor = critical ? ConsoleColor.Red : ConsoleColor.DarkYellow;
			var prefix = critical ? "ERROR" : "WARNING";
			Console.Write($"{prefix}: {message}.");
			Console.ForegroundColor = ConsoleColor.White;
			if (lineIndex != -1 && charIndex != -1)
			{
				Console.WriteLine($"\r\nAt line {lineIndex + 1}, column {charIndex + 1}.\r\n");
			}
			if (critical)
			{
				Console.WriteLine("\r\nPress any key to close app.");
				Console.ReadKey();
				Environment.Exit(0);
			}
		}

		public static void LogInfo(string message)
		{
			Console.ForegroundColor = ConsoleColor.DarkCyan;
			Console.WriteLine($"STATUS: {message}.");
			Console.ForegroundColor = ConsoleColor.White;
		}
	}
}
