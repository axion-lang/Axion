using System.Collections.Generic;
using System.IO;
using Axion;
using Axion.Processing;
using Axion.Tokens;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Wrapper.Testing.NUnit {
    [TestFixture]
    internal static class LexerTests {
        private static string OutPath =>
            Directory.Exists(__outPath)
                ? __outPath
                : throw new DirectoryNotFoundException("Unit tests output directory not found.");

        private const string __outPath = "C:\\Users\\Fluctus\\Documents\\Code\\CSharp\\Axion\\Wrapper\\Testing\\_out\\";

        private static string InPath =>
            Directory.Exists(__inPath)
                ? __inPath
                : throw new DirectoryNotFoundException("Unit tests input directory not found.");

        private const string __inPath = "C:\\Users\\Fluctus\\Documents\\Code\\CSharp\\Axion\\Wrapper\\Testing\\_in\\";

        private const string testExtension  = ".unittest.ax";
        private const string axionExtension = ".ax";

        [Test]
        public static void NestedMultilineCommentInvalid() {
            Compiler.Options.Debug = true;
            string[] files = Directory.GetFiles(InPath, $"{nameof(NestedMultilineCommentInvalid)}*{axionExtension}");

            // check for error
            for (var i = 1; i < files.Length + 1; i++) {
                var source = new SourceCode(
                    new FileInfo($"{InPath}{nameof(NestedMultilineCommentInvalid)}_{i}{axionExtension}"),
                    $"{OutPath}{nameof(NestedMultilineCommentInvalid)}_{i}{testExtension}"
                );
                Assert.Throws<ProcessingException>(() => Compiler.Process(source, SourceProcessingMode.Lex));
            }
        }

        [Test]
        public static void NestedMultilineCommentValid() {
            Compiler.Options.Debug = true;
            string[] files = Directory.GetFiles(InPath, $"{nameof(NestedMultilineCommentValid)}*{axionExtension}");

            // validate
            for (var i = 1; i < files.Length + 1; i++) {
                var source = new SourceCode(
                    new FileInfo($"{InPath}{nameof(NestedMultilineCommentValid)}_{i}{axionExtension}"),
                    $"{OutPath}{nameof(NestedMultilineCommentValid)}_{i}{testExtension}"
                );
                Assert.DoesNotThrow(() => Compiler.Process(source, SourceProcessingMode.Lex));
            }
        }

        [Test]
        public static void UseBlockValid() {
            Compiler.Options.Debug = true;
            var source = new SourceCode(
                @"
use {
   collections.enumerable,
   dotnet.namespace,
   some.multi.nested.namespace.end
}",
                OutPath + nameof(UseBlockValid) + testExtension
            );

            Assert.DoesNotThrow(() => Compiler.Process(source, SourceProcessingMode.Lex));

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
            expected.AddLast(new Token(TokenType.EndOfFile, (4, 1), Spec.EndFile.ToString()));

            #endregion

            string t1 = JsonConvert.SerializeObject(source.Tokens, Compiler.Options.JsonSerializer);
            string t2 = JsonConvert.SerializeObject(expected,      Compiler.Options.JsonSerializer);
            Assert.AreEqual(t1, t2);
        }

        [Test]
        public static void VariousStuffValid() {
            Compiler.Options.Debug = true;
            var source = new SourceCode(
                new FileInfo(InPath + nameof(VariousStuffValid) + axionExtension),
                OutPath + nameof(VariousStuffValid) + testExtension
            );

            Assert.DoesNotThrow(() => Compiler.Process(source, SourceProcessingMode.Lex));
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