using System;
using System.IO;
using AxionStandard.Enums;
using AxionStandard.Processing;
using Newtonsoft.Json;

namespace AxionStandard {
   public static class Compiler {
      internal const string AnyKeyToClose = "Press any key to close app.";
      internal const string Version = "0.3.0.0";

      internal static readonly string AppFolder = AppDomain.CurrentDomain.BaseDirectory + "\\";

      public static SourceCode ProcessingSource;
      //internal static readonly Parser Parser = new Parser();

      internal static readonly string[] ConsoleArgumentPrefixes = { "/", "-", "--", "---" };
      internal static bool FlagDebug;
      internal static bool FlagScript;
      internal static string FileNameOrScript;

      public static JsonSerializerSettings JsonSerializerSettings { get; } = new JsonSerializerSettings {
         Formatting = Formatting.Indented
      };

      public static void EntryPoint(string[] args) {
         Console.WriteLine($"Axion programming language compiler v. {Version}");
         Console.WriteLine($"Launched at '{AppFolder}'");
         Console.WriteLine("Type 'help' or '?' to get arguments support.");
         Console.WriteLine();

         var ffProcessing = ProcessingType.Compilation;
         for (int i = 0; i < args.Length; i++) {
            // removing argument prefixes
            for (int j = 0; j < ConsoleArgumentPrefixes.Length; j++) {
               string prefix = ConsoleArgumentPrefixes[j];
               if (args[i].StartsWith(prefix)) {
                  args[i] = args[i].Remove(0, prefix.Length);
               }
            }

            if (i == args.Length - 1) {
               FileNameOrScript = args[i];
               break;
            }

            switch (args[i]) {
               case "help":
               case "?": {
                  Console.WriteLine();
                  Console.WriteLine("path to file should be the last argument.");
                  Console.WriteLine("======================Input modes=====================");
                  Console.WriteLine("(no argument)        Process input file.");
                  Console.WriteLine("-s  -script          Process input script.");
                  Console.WriteLine("====================Compiler modes====================");
                  Console.WriteLine("-t  -tokenize        Create tokens list from input script.");
                  Console.WriteLine("-p  -parse           Create tokens from input script and abstract syntax tree (AST) from tokens.");
                  Console.WriteLine("-i  -interpret       Create tokens, AST, then interpret input script.");
                  Console.WriteLine("-d  -debug           Save JSON debug information to file.");
                  break;
               }
               case "s": {
                  FlagScript = true;
                  break;
               }
               case "t": {
                  ffProcessing = ProcessingType.Tokenize;
                  break;
               }
               case "p": {
                  ffProcessing = ProcessingType.BuildSyntaxTree;
                  break;
               }
               case "i": {
                  ffProcessing = ProcessingType.Interpretation;
                  break;
               }
               case "d": {
                  FlagDebug = true;
                  break;
               }
               default: {
                  Console.WriteLine("Invalid argument. type 'help' or '?' to get support about launch arguments.");
                  break;
               }
            }
         }

         if (FlagDebug) {
            if (!Directory.Exists(AppFolder + "output")) {
               Directory.CreateDirectory(AppFolder + "output");
            }
         }

         if (FlagScript) {
            ProcessingSource = new SourceCode(FileNameOrScript);
         }
         else {
            var fileInfo = new FileInfo(FileNameOrScript);
            if (!fileInfo.Exists) {
               throw new FileNotFoundException("Input file doesn't exists.");
            }

            var sourceFile = new SourceCode(fileInfo);
            ProcessingSource = sourceFile;
            sourceFile.Compile(ffProcessing);
         }

         Logger.LogInfo("Ready.");
         Console.WriteLine(AnyKeyToClose);
         Console.Read();
      }
   }
}