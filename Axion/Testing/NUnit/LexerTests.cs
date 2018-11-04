using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        ///     A quick way to clear unit tests debug output.
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
        public static void IndentationLengthComputedCorrectly() {
            // with tabs
            var source1 = new SourceCode(
                "i = 0\r\n" +
                "while i < 10:\r\n" +
                "\tconsole.print 'OK.'\r\n" +
                "\tj = 0\r\n" +
                "\twhile j < 5:\r\n" +
                "\t\tif i == 3 and j == 2:\r\n" +
                "\t\t\tconsole.print 'Got it!'\r\n" +
                "\t\tj++\r\n" +
                "\ti++\r\n" +
                "",
                OutPath + nameof(IndentationLengthComputedCorrectly) + "_tabs" + testExtension
            );

            const int indentIndex1 = 10;
            const int indentIndex2 = 26;
            const int indentIndex3 = 37;
            const int indentIndex4 = 43;
            const int indentIndex5 = 47;

            source1.Process(
                SourceProcessingMode.Lex,
                SourceProcessingOptions.SyntaxAnalysisDebugOutput |
                SourceProcessingOptions.CheckIndentationConsistency
            );

            Assert.IsTrue(source1.Errors.Count == 0);
            Assert.IsTrue(source1.Warnings.Count == 0);

            Assert.IsTrue(source1.Tokens.ElementAt(indentIndex1).Equals(new Token(TokenType.Indent,  (2, 0), "\t")));
            Assert.IsTrue(source1.Tokens.ElementAt(indentIndex2).Equals(new Token(TokenType.Indent,  (5, 0), "\t\t")));
            Assert.IsTrue(source1.Tokens.ElementAt(indentIndex3).Equals(new Token(TokenType.Indent,  (6, 0), "\t\t\t")));
            Assert.IsTrue(source1.Tokens.ElementAt(indentIndex4).Equals(new Token(TokenType.Outdent, (7, 0), "\t\t")));
            Assert.IsTrue(source1.Tokens.ElementAt(indentIndex5).Equals(new Token(TokenType.Outdent, (8, 0), "\t")));

            // with spaces
            var source2 = new SourceCode(
                "i = 0\r\n" +
                "while i < 10:\r\n" +
                "    console.print 'OK.'\r\n" +
                "    j = 0\r\n" +
                "    while j < 5:\r\n" +
                "        if i == 3 and j == 2:\r\n" +
                "            console.print 'Got it!'\r\n" +
                "        j++\r\n" +
                "    i++\r\n" +
                "",
                OutPath + nameof(IndentationLengthComputedCorrectly) + "_spaces" + testExtension
            );

            source2.Process(
                SourceProcessingMode.Lex,
                SourceProcessingOptions.SyntaxAnalysisDebugOutput |
                SourceProcessingOptions.CheckIndentationConsistency
            );

            Assert.IsTrue(source2.Errors.Count == 0);
            Assert.IsTrue(source2.Warnings.Count == 0);

            Assert.IsTrue(source2.Tokens.ElementAt(indentIndex1).Equals(new Token(TokenType.Indent,  (2, 0), new string(' ', 4))));
            Assert.IsTrue(source2.Tokens.ElementAt(indentIndex2).Equals(new Token(TokenType.Indent,  (5, 0), new string(' ', 8))));
            Assert.IsTrue(source2.Tokens.ElementAt(indentIndex3).Equals(new Token(TokenType.Indent,  (6, 0), new string(' ', 12))));
            Assert.IsTrue(source2.Tokens.ElementAt(indentIndex4).Equals(new Token(TokenType.Outdent, (7, 0), new string(' ', 8))));
            Assert.IsTrue(source2.Tokens.ElementAt(indentIndex5).Equals(new Token(TokenType.Outdent, (8, 0), new string(' ', 4))));

            // mixed
            var source3 = new SourceCode(
                // use double indent at line 5 to distinct 8-space tab from spaces.
                "i = 0\r\n" +                                 // 1
                "while i < 10:\r\n" +                         // 2
                "\tconsole.print 'OK.'\r\n" +                 // 3
                "\tj = 0\r\n" +                               // 4
                "\twhile j < 5:\r\n" +                        // 5
                "            if i == 3 and j == 2:\r\n" +     // 5
                "            \tconsole.print 'Got it!'\r\n" + // 7
                "            j++\r\n" +                       // 8
                "        i++\r\n" +                           // 9
                "",
                OutPath + nameof(IndentationLengthComputedCorrectly) + "_mixed" + testExtension
            );

            source3.Process(
                SourceProcessingMode.Lex,
                SourceProcessingOptions.SyntaxAnalysisDebugOutput |
                SourceProcessingOptions.CheckIndentationConsistency
            );

            Assert.IsTrue(source3.Errors.Count == 0);
            Assert.IsTrue(source3.Warnings.Count == 1);

            Assert.IsTrue(source3.Tokens.ElementAt(indentIndex1).Equals(new Token(TokenType.Indent,  (2, 0), "\t")));
            Assert.IsTrue(source3.Tokens.ElementAt(indentIndex2).Equals(new Token(TokenType.Indent,  (5, 0), new string(' ', 12))));
            Assert.IsTrue(source3.Tokens.ElementAt(indentIndex3).Equals(new Token(TokenType.Indent,  (6, 0), new string(' ', 12) + "\t")));
            Assert.IsTrue(source3.Tokens.ElementAt(indentIndex4).Equals(new Token(TokenType.Outdent, (7, 0), new string(' ', 12))));
            Assert.IsTrue(source3.Tokens.ElementAt(indentIndex5).Equals(new Token(TokenType.Outdent, (8, 0), new string(' ', 8))));

            string TokenString(Token tk) {
                return "(" + tk.Value.Replace("\t", "    ") + ")";
            }

            Assert.AreEqual(string.Join(";", source1.Tokens.Select(TokenString)), string.Join(";", source2.Tokens.Select(TokenString)));
            Assert.AreEqual(source2.Tokens.Count,                                 source3.Tokens.Count);
        }

        [Test]
        public static void NestedMultilineCommentInvalid() {
            string[] files = Directory.GetFiles(InPath, $"{nameof(NestedMultilineCommentInvalid)}*{Compiler.SourceFileExtension}");

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
            string[] files = Directory.GetFiles(InPath, $"{nameof(NestedMultilineCommentValid)}*{Compiler.SourceFileExtension}");

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
                "use {\r\n" +
                "   collections.enumerable,\r\n" +
                "   dotnet.namespace,\r\n" +
                "   some.multi.nested.namespace.end\r\n" +
                "}",
                OutPath + nameof(UseBlockValid) + testExtension
            );

            source.Process(SourceProcessingMode.Lex, SourceProcessingOptions.SyntaxAnalysisDebugOutput);
            Assert.IsTrue(source.Errors.Count == 0);

            var expectedTokens = new LinkedList<Token>();

            #region Expected tokens list

            // line 1
            expectedTokens.AddLast(new Token(TokenType.KeywordUse, (0, 0), "use"));
            expectedTokens.AddLast(new OperatorToken((0, 4), "{"));
            expectedTokens.AddLast(new Token(TokenType.Newline, (0, 5), Spec.EndOfLine.ToString()));
            // line 2
            expectedTokens.AddLast(new Token(TokenType.Identifier, (1, 3), "collections"));
            expectedTokens.AddLast(new OperatorToken((1, 14), "."));
            expectedTokens.AddLast(new Token(TokenType.Identifier, (1, 15), "enumerable"));
            expectedTokens.AddLast(new OperatorToken((1, 25), ","));
            expectedTokens.AddLast(new Token(TokenType.Newline, (1, 26), Spec.EndOfLine.ToString()));
            // line 3
            expectedTokens.AddLast(new Token(TokenType.Identifier, (2, 3), "dotnet"));
            expectedTokens.AddLast(new OperatorToken((2, 9), "."));
            expectedTokens.AddLast(new Token(TokenType.Identifier, (2, 10), "namespace"));
            expectedTokens.AddLast(new OperatorToken((2, 19), ","));
            expectedTokens.AddLast(new Token(TokenType.Newline, (2, 20), Spec.EndOfLine.ToString()));
            // line 4
            expectedTokens.AddLast(new Token(TokenType.Identifier, (3, 3), "some"));
            expectedTokens.AddLast(new OperatorToken((3, 7), "."));
            expectedTokens.AddLast(new Token(TokenType.Identifier, (3, 8), "multi"));
            expectedTokens.AddLast(new OperatorToken((3, 13), "."));
            expectedTokens.AddLast(new Token(TokenType.Identifier, (3, 14), "nested"));
            expectedTokens.AddLast(new OperatorToken((3, 20), "."));
            expectedTokens.AddLast(new Token(TokenType.Identifier, (3, 21), "namespace"));
            expectedTokens.AddLast(new OperatorToken((3, 30), "."));
            expectedTokens.AddLast(new Token(TokenType.Identifier, (3, 31), "end"));
            expectedTokens.AddLast(new Token(TokenType.Newline,    (3, 34), Spec.EndOfLine.ToString()));
            // line 5
            expectedTokens.AddLast(new OperatorToken((4, 0), "}"));
            expectedTokens.AddLast(new Token(TokenType.EndOfStream, (4, 1), Spec.EndOfStream.ToString()));

            #endregion

            string t1 = JsonConvert.SerializeObject(source.Tokens,  Compiler.JsonSerializer);
            string t2 = JsonConvert.SerializeObject(expectedTokens, Compiler.JsonSerializer);
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