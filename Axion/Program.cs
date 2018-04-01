using System;
using System.IO;
using System.Linq;
using Axion.Processing;

namespace Axion
{
	internal static class Program
	{
		public const string AnyKeyToClose = "Press any key to close app.";
		private static string ScriptFileName;
		public static readonly ScriptFile OutFile = new ScriptFile();

		private static void Main(string[] args)
		{
			if (args.Length < 1 ||
				!File.Exists(args[0]))
			{
				LogError("Input file doesn't exists", ErrorOrigin.Input);
				return;
			}

			ScriptFileName = args[0];

			LogInfo("Tokens list generation...");
			Lexer.Tokenize(File.ReadAllText(ScriptFileName).ToCharArray());
			LogInfo($"Saving tokens list to: {ScriptFileName}.lex ...");
			Save("lex");

			LogInfo("Syntax tree generation...");
			Parser.Parse();
			LogInfo($"Saving tree to: {ScriptFileName}.tree ...");
			Save("tree");

			LogInfo("\r\nReady!");
			Console.WriteLine();
			Console.WriteLine("Imported libraries:");
			Console.WriteLine(string.Join(",\r\n", OutFile.Imports.Select(t => t.ToString())));

			Console.WriteLine(AnyKeyToClose);
			Console.ReadKey();
		}

		private static void Save(string postfix)
		{
			var fileName = $"{ScriptFileName}.{postfix}";
			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}

			File.WriteAllText(fileName,
							  string.Join(",\r\n",
										  OutFile.Tokens.Select(token => token.ToString())));
		}

		public static void LogError(string message,
									ErrorOrigin errorOrigin,
									int lineIndex = -1,
									int charIndex = -1)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"{errorOrigin:G} error: {message}.");

			if (lineIndex != -1 &&
				charIndex != -1)
			{
				Console.WriteLine($"At line {lineIndex}, column {charIndex}.");
			}
			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.White;

			LogInfo($"Saving traceback to: {ScriptFileName}.traceback ...");
			Save("traceback");
			Console.Write(AnyKeyToClose);
			Console.ReadKey();
			Environment.Exit(0);
		}

		public static void LogWarning(string message,
									  ErrorOrigin errorOrigin,
									  int lineIndex = -1,
									  int charIndex = -1)
		{
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			Console.WriteLine($"{errorOrigin:G} warning: {message}.");

			if (lineIndex != -1 &&
				charIndex != -1)
			{
				Console.WriteLine($"At line {lineIndex + 1}, column {charIndex + 1}.");
			}
			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.White;
		}

		public static void LogInfo(string message)
		{
			Console.ForegroundColor = ConsoleColor.DarkCyan;
			Console.WriteLine(message);
			Console.ForegroundColor = ConsoleColor.White;
		}
	}
}