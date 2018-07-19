using System;
using System.Collections.Generic;
using System.IO;
using Axion.Enums;
using Axion.Processing.Tokens;
using Newtonsoft.Json;

namespace Axion.Processing {
   public sealed class SourceCode {
      private readonly string debugFilePath;
      private readonly string[] sourceLines;
      private readonly string sourceFileName;
      public readonly List<Token> SyntaxTree = new List<Token>();
      public readonly Chain<Token> Tokens = new Chain<Token>();

      public SourceCode(string sourceCode) {
         sourceFileName = "testsource.ax";
         debugFilePath = Compiler.AppFolder + "output\\_debugInfo.ax.json";
         sourceLines = sourceCode.Replace("\r", "").Split('\n');
      }

      public SourceCode(FileInfo file) {
         if (!file.Exists) {
            throw new FileNotFoundException("Source file doesn't exists", file.FullName);
         }

         if (file.Extension != ".ax") {
            throw new ArgumentException("Source file must have \".ax\" extension");
         }

         sourceFileName = file.Name;
         debugFilePath = file.FullName + "_debugInfo.json";
         sourceLines = File.ReadAllText(file.FullName).Replace("\r", "").Split('\n');
      }

      public void Compile(ProcessingType processingType) {
         Logger.LogInfo($"## Compiling '{sourceFileName}' ...");
         if (sourceLines.Length == 0) {
            Logger.LogInfo("# Source is empty. Compilation canceled.");
            return;
         }
         Logger.LogInfo("# Tokens list generation...");
         {
            Lexer.Tokenize(sourceLines, Tokens);
            if (processingType == ProcessingType.Tokenize) {
               goto COMPILATION_END;
            }
         }
         Logger.LogInfo("# Abstract Syntax Tree generation...");
         {
            //SyntaxTree = Program.Parser.Process(this);
            if (processingType == ProcessingType.BuildSyntaxTree) {
               goto COMPILATION_END;
            }
         }

         switch (processingType) {
            case ProcessingType.Interpretation: {
               throw new NotImplementedException("Interpretation support is in progress!");
            }
            case ProcessingType.Transpile_C: {
               throw new NotImplementedException("Transpiling to 'C' is not implemented yet.");
            }
            default: {
               throw new NotImplementedException(
                  $"'{Enum.GetName(typeof(ProcessingType), processingType)}' mode not implemented yet.");
            }
         }

         COMPILATION_END:

         if (Compiler.FlagDebug) {
            Logger.LogInfo($"# Saving debugging information to '{debugFilePath}' ...");
            SaveDebugInfoToFile();
         }

         Logger.LogInfo($"\"{sourceFileName}\" compiled.");
      }

      public void SaveDebugInfoToFile() {
         string debugInfo =
            "{" +
            Environment.NewLine +
            "\"tokens\": " +
            JsonConvert.SerializeObject(Tokens, Compiler.JsonSerializerSettings) +
            "," + Environment.NewLine +
            "\"syntaxTree\": " +
            JsonConvert.SerializeObject(SyntaxTree, Compiler.JsonSerializerSettings) +
            Environment.NewLine +
            "}";
         File.WriteAllText(debugFilePath, debugInfo);
      }
   }
}