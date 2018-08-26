using System.Collections.Generic;
using System.IO;
using Axion.Core;
using Axion.Core.Processing;
using Axion.Core.Tokens;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Axion.Testing.NUnit {
    [TestFixture]
    internal static class LexerTests {
        private static string OutPath {
            get {
                if (!Directory.Exists(__outPath)) {
                    Directory.CreateDirectory(__outPath);
                }
                return __outPath;
            }
        }

        private const string __outPath = "C:\\Users\\Fluctus\\Documents\\Code\\CSharp\\Axion\\Axion\\Testing\\_out\\";

        private static string InPath {
            get {
                if (!Directory.Exists(__inPath)) {
                    Directory.CreateDirectory(__inPath);
                }
                return __inPath;
            }
        }

        private const string __inPath = "C:\\Users\\Fluctus\\Documents\\Code\\CSharp\\Axion\\Axion\\Testing\\_in\\";

        private const string testExtension = ".unittest" + Compiler.SourceFileExtension;

        /// <summary>
        ///     a quick way to clear unit tests debug output.
        /// </summary>
        [Test]
        public static void _ClearDebugDirectory() {
            Assert.DoesNotThrow(
                () => {
                    var dir = new DirectoryInfo(__outPath);
                    foreach (FileInfo file in dir.EnumerateFiles()) {
                        file.Delete();
                    }
                }
            );
        }

        [Test]
        public static void NestedMultilineCommentInvalid() {
            string[] files = Directory.GetFiles(
                InPath, $"{nameof(NestedMultilineCommentInvalid)}*{Compiler.SourceFileExtension}"
            );

            // check for error
            for (var i = 1; i < files.Length + 1; i++) {
                var source = new SourceCode(
                    new FileInfo($"{InPath}{nameof(NestedMultilineCommentInvalid)}_{i}{Compiler.SourceFileExtension}"),
                    $"{OutPath}{nameof(NestedMultilineCommentInvalid)}_{i}{testExtension}"
                );
                source.Process(SourceProcessingMode.Lex, SourceProcessingOptions.SyntaxAnalysisDebugOutput);
                Assert.IsTrue(source.Errors.Count == 1);
            }
        }

        [Test]
        public static void NestedMultilineCommentValid() {
            string[] files = Directory.GetFiles(
                InPath, $"{nameof(NestedMultilineCommentValid)}*{Compiler.SourceFileExtension}"
            );

            // validate
            for (var i = 1; i < files.Length + 1; i++) {
                var source = new SourceCode(
                    new FileInfo($"{InPath}{nameof(NestedMultilineCommentValid)}_{i}{Compiler.SourceFileExtension}"),
                    $"{OutPath}{nameof(NestedMultilineCommentValid)}_{i}{testExtension}"
                );
                source.Process(SourceProcessingMode.Lex, SourceProcessingOptions.SyntaxAnalysisDebugOutput);
                Assert.IsTrue(source.Errors.Count == 0);
            }
        }

        [Test]
        public static void StringsValidation() {
            var source = new SourceCode(
                new FileInfo(InPath + nameof(StringsValidation) + Compiler.SourceFileExtension),
                OutPath + nameof(StringsValidation) + testExtension
            );
            source.Process(SourceProcessingMode.Lex, SourceProcessingOptions.SyntaxAnalysisDebugOutput);
            Assert.IsTrue(source.Errors.Count == 0);
        }

        [Test]
        public static void UseBlockValid() {
            var source = new SourceCode(
                @"use {
   collections.enumerable,
   dotnet.namespace,
   some.multi.nested.namespace.end
}",
                OutPath + nameof(UseBlockValid) + testExtension
            );

            source.Process(SourceProcessingMode.Lex, SourceProcessingOptions.SyntaxAnalysisDebugOutput);
            Assert.IsTrue(source.Errors.Count == 0);

            var expected = new LinkedList<Token>();

            #region Expected tokens list

            // line 1
            expected.AddLast(new Token(TokenType.KeywordUse, (0, 0), "use"));
            expected.AddLast(new OperatorToken("{", (0, 4)));
            expected.AddLast(new Token(TokenType.Newline, (0, 5), Spec.EndLine.ToString()));
            // line 2
            expected.AddLast(new Token(TokenType.Identifier, (1, 3), "collections"));
            expected.AddLast(new OperatorToken(".", (1, 14)));
            expected.AddLast(new Token(TokenType.Identifier, (1, 15), "enumerable"));
            expected.AddLast(new OperatorToken(",", (1, 25)));
            expected.AddLast(new Token(TokenType.Newline, (1, 26), Spec.EndLine.ToString()));
            // line 3
            expected.AddLast(new Token(TokenType.Identifier, (2, 3), "dotnet"));
            expected.AddLast(new OperatorToken(".", (2, 9)));
            expected.AddLast(new Token(TokenType.Identifier, (2, 10), "namespace"));
            expected.AddLast(new OperatorToken(",", (2, 19)));
            expected.AddLast(new Token(TokenType.Newline, (2, 20), Spec.EndLine.ToString()));
            // line 4
            expected.AddLast(new Token(TokenType.Identifier, (3, 3), "some"));
            expected.AddLast(new OperatorToken(".", (3, 7)));
            expected.AddLast(new Token(TokenType.Identifier, (3, 8), "multi"));
            expected.AddLast(new OperatorToken(".", (3, 13)));
            expected.AddLast(new Token(TokenType.Identifier, (3, 14), "nested"));
            expected.AddLast(new OperatorToken(".", (3, 20)));
            expected.AddLast(new Token(TokenType.Identifier, (3, 21), "namespace"));
            expected.AddLast(new OperatorToken(".", (3, 30)));
            expected.AddLast(new Token(TokenType.Identifier, (3, 31), "end"));
            expected.AddLast(new Token(TokenType.Newline,    (3, 34), Spec.EndLine.ToString()));
            // line 5
            expected.AddLast(new OperatorToken("}", (4, 0)));
            expected.AddLast(new Token(TokenType.EndOfStream, (4, 1), Spec.EndStream.ToString()));

            #endregion

            string t1 = JsonConvert.SerializeObject(source.Tokens, Compiler.JsonSerializer);
            string t2 = JsonConvert.SerializeObject(expected,      Compiler.JsonSerializer);
            Assert.AreEqual(t1, t2);
        }

        [Test]
        public static void VariousStuffValid() {
            var source = new SourceCode(
                new FileInfo(InPath + nameof(VariousStuffValid) + Compiler.SourceFileExtension),
                OutPath + nameof(VariousStuffValid) + testExtension
            );

            source.Process(SourceProcessingMode.Lex, SourceProcessingOptions.SyntaxAnalysisDebugOutput);
            Assert.IsTrue(source.Errors.Count == 0);
        }

        //[Test]
        //public static void NumbersParsedCorrectly() {
        //    string[] numbers = {
        //        "123456", "654321",
        //        "123.456", "654.321",
        //        "0x16829641",
        //        "0b10110001",
        //        "0o72517242",
        //        "3 + 4j",
        //        "12e6"
        //    };
        //    // TODO complete numbers parsing unit test
        //    Token[] tokens = {
        //        //new Token(TokenType.Unknown,), 
        //    };

        //    Assert.AreEqual(numbers.Length, tokens.Length);

        //    for (int i = 0; i < numbers.Length; i++) {
        //        var source = new SourceCode("var number = " + numbers[i] + " + 0b10010010");
        //        Assert.DoesNotThrow(() => Compiler.Process(source, SourceProcessingMode.Lex));

        //        var expected = new LinkedList<Token>();

        //        #region Expected tokens list

        //        // line 1
        //        expected.AddLast(new Token(TokenType.Keyword, (0, 0), "var"));
        //        expected.AddLast(new Token(TokenType.Identifier, (1, 3), "number"));
        //        expected.AddLast(new OperatorToken("=", (0, 3)));
        //        expected.AddLast(tokens[i]);
        //        expected.AddLast(new OperatorToken("+", (0, 3)));
        //        // expected.AddLast(new OperatorToken(".", (1, 14)));
        //        expected.AddLast(new Token(TokenType.EndOfFile, (4, 1), Specification.EndFile.ToString()));

        //        #endregion
        //    }
        //}

        //private static string PrepareFileName(string value) {
        //    var name = value.ToCharArray();
        //    var invalid1 = Path.GetInvalidPathChars();
        //    var invalid2 = Path.GetInvalidFileNameChars();
        //    for (int i = 0; i < name.Length; i++) {
        //        // replace all unneeded characters
        //        if (invalid1.Contains(name[i])
        //            || invalid2.Contains(name[i])
        //            || char.IsWhiteSpace(name[i])) {
        //            name[i] = '_';
        //        }
        //    }
        //    return new string(name) + testExtension;
        //}
    }
}