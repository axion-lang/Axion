using System.Collections.Generic;
using System.Linq;
using Axion.Core;
using Axion.Core.Processing;
using Axion.Core.Processing.Lexical.Tokens;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Axion.Testing.NUnit {
    public partial class LexerTests {
        [Test]
        public void UseBlockValid() {
            var source = new SourceCode(
                "use {\n" +
                "   collections.enumerable,\n" +
                "   dotnet.namespace,\n" +
                "   some.multi.nested.namespace.end\n" +
                "}",
                outPath + nameof(UseBlockValid) + testExtension
            );

            source.Process(SourceProcessingMode.Lex, SourceProcessingOptions.SyntaxAnalysisDebugOutput);
            Assert.IsTrue(source.Errors.Count == 0);

            var expectedTokens = new LinkedList<Token>();

            #region Expected tokens list

            // line 1
            expectedTokens.AddLast(new KeywordToken(TokenType.KeywordUse, (0, 0), " "));
            expectedTokens.AddLast(new OperatorToken((0, 4), "{"));
            expectedTokens.AddLast(new EndOfLineToken((0, 5), whitespaces: "   "));
            // line 2
            expectedTokens.AddLast(new Token(TokenType.Identifier, (1, 3), "collections"));
            expectedTokens.AddLast(new OperatorToken((1, 14), "."));
            expectedTokens.AddLast(new Token(TokenType.Identifier, (1, 15), "enumerable"));
            expectedTokens.AddLast(new OperatorToken((1, 25), ","));
            expectedTokens.AddLast(new EndOfLineToken((1, 26), whitespaces: "   "));
            // line 3
            expectedTokens.AddLast(new Token(TokenType.Identifier, (2, 3), "dotnet"));
            expectedTokens.AddLast(new OperatorToken((2, 9), "."));
            expectedTokens.AddLast(new Token(TokenType.Identifier, (2, 10), "namespace"));
            expectedTokens.AddLast(new OperatorToken((2, 19), ","));
            expectedTokens.AddLast(new EndOfLineToken((2, 20), whitespaces: "   "));
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
            expectedTokens.AddLast(new EndOfLineToken((3, 34)));
            // line 5
            expectedTokens.AddLast(new OperatorToken((4, 0), "}"));
            expectedTokens.AddLast(new EndOfStreamToken((4, 1)));

            #endregion

            string t1 = JsonConvert.SerializeObject(source.Tokens,  Compiler.JsonSerializer);
            string t2 = JsonConvert.SerializeObject(expectedTokens, Compiler.JsonSerializer);
            Assert.AreEqual(t1, t2);
        }

        [Test]
        public void IndentationLengthComputedCorrectly() {
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
                outPath + nameof(IndentationLengthComputedCorrectly) + "_tabs" + testExtension
            );

            const int indentIndex1 = 10;
            const int indentIndex2 = 26;
            const int indentIndex3 = 37;
            const int indentIndex4 = 43;
            const int indentIndex5 = 47;

            source1.Process(
                SourceProcessingMode.Lex,
                SourceProcessingOptions.SyntaxAnalysisDebugOutput
              | SourceProcessingOptions.CheckIndentationConsistency
            );

            Assert.IsTrue(source1.Errors.Count == 0);
            Assert.IsTrue(source1.Warnings.Count == 0);

            Assert.IsTrue(source1.Tokens.ElementAt(indentIndex1).Equals(new IndentToken((2, 0), "\t")));
            Assert.IsTrue(source1.Tokens.ElementAt(indentIndex2).Equals(new IndentToken((5, 0), "\t\t")));
            Assert.IsTrue(source1.Tokens.ElementAt(indentIndex3).Equals(new IndentToken((6, 0), "\t\t\t")));
            Assert.IsTrue(source1.Tokens.ElementAt(indentIndex4).Equals(new OutdentToken((7, 0))));
            Assert.IsTrue(source1.Tokens.ElementAt(indentIndex5).Equals(new OutdentToken((8, 0))));

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
                outPath + nameof(IndentationLengthComputedCorrectly) + "_spaces" + testExtension
            );

            source2.Process(
                SourceProcessingMode.Lex,
                SourceProcessingOptions.SyntaxAnalysisDebugOutput
              | SourceProcessingOptions.CheckIndentationConsistency
            );

            Assert.IsTrue(source2.Errors.Count == 0);
            Assert.IsTrue(source2.Warnings.Count == 0);

            Assert.IsTrue(source2.Tokens.ElementAt(indentIndex1).Equals(new IndentToken((2, 0), new string(' ', 4))));
            Assert.IsTrue(source2.Tokens.ElementAt(indentIndex2).Equals(new IndentToken((5, 0), new string(' ', 8))));
            Assert.IsTrue(source2.Tokens.ElementAt(indentIndex3).Equals(new IndentToken((6, 0), new string(' ', 12))));
            Assert.IsTrue(source2.Tokens.ElementAt(indentIndex4).Equals(new OutdentToken((7, 0))));
            Assert.IsTrue(source2.Tokens.ElementAt(indentIndex5).Equals(new OutdentToken((8, 0))));

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
                outPath + nameof(IndentationLengthComputedCorrectly) + "_mixed" + testExtension
            );

            source3.Process(
                SourceProcessingMode.Lex,
                SourceProcessingOptions.SyntaxAnalysisDebugOutput
              | SourceProcessingOptions.CheckIndentationConsistency
            );

            Assert.IsTrue(source3.Errors.Count == 0);
            Assert.IsTrue(source3.Warnings.Count == 1);

            Assert.IsTrue(source3.Tokens.ElementAt(indentIndex1).Equals(new IndentToken((2, 0), "\t")));
            Assert.IsTrue(source3.Tokens.ElementAt(indentIndex2).Equals(new IndentToken((5, 0), new string(' ', 12))));
            Assert.IsTrue(source3.Tokens.ElementAt(indentIndex3).Equals(new IndentToken((6, 0), new string(' ', 12) + "\t")));
            Assert.IsTrue(source3.Tokens.ElementAt(indentIndex4).Equals(new OutdentToken((7, 0))));
            Assert.IsTrue(source3.Tokens.ElementAt(indentIndex5).Equals(new OutdentToken((8, 0))));

            string TokenString(Token tk) {
                return "(" + tk.Value.Replace("\t", "    ") + ")";
            }

            Assert.AreEqual(string.Join(";", source1.Tokens.Select(TokenString)), string.Join(";", source2.Tokens.Select(TokenString)));
            Assert.AreEqual(source2.Tokens.Count,                                 source3.Tokens.Count);
        }

        [Test]
        public void NumbersParsedCorrectly() {
            string[] numbers = {
                "123456", "654321",
                "123.456", "654.321",
                "0x16829641",
                "0b10110001",
                "0o72517242"
                //"3 + 4j",
                //"12e"
            };
            var pos = (0, 9);
            // TODO complete numbers parsing test
            Token[] tokens = {
                new NumberToken(pos, "123456",     new NumberOptions(32)),
                new NumberToken(pos, "654321",     new NumberOptions(32)),
                new NumberToken(pos, "123.456",    new NumberOptions(32)),
                new NumberToken(pos, "654.321",    new NumberOptions(32)),
                new NumberToken(pos, "0x16829641", new NumberOptions(32)),
                new NumberToken(pos, "0b10110001", new NumberOptions(32)),
                new NumberToken(pos, "0o72517242", new NumberOptions(32))
                //new NumberToken(pos, "3 + 4j", new NumberOptions(32)),
                //new NumberToken(pos, 12, new NumberOptions()),
            };
            foreach (Token token in tokens) {
                token.AppendWhitespace(" ");
            }

            Assert.AreEqual(numbers.Length, tokens.Length);

            for (var i = 0; i < numbers.Length; i++) {
                var source = new SourceCode(
                    "number = " + numbers[i] + " + 0b10010010",
                    outPath + nameof(NumbersParsedCorrectly) + i + testExtension
                );
                Assert.DoesNotThrow(
                    () => source.Process(
                        SourceProcessingMode.Lex,
                        SourceProcessingOptions.SyntaxAnalysisDebugOutput
                    )
                );

                var expected = new LinkedList<Token>();

                #region Expected tokens list

                // line 1
                expected.AddLast(new Token(TokenType.Identifier, (0, 0), "number", " "));
                expected.AddLast(new OperatorToken((0, 7), "=", " "));
                expected.AddLast(tokens[i]);
                int numLen = tokens[i].EndColumn;
                expected.AddLast(new OperatorToken((0, numLen), "+", " "));
                expected.AddLast(new NumberToken((0, numLen + 2), "0b10010010", new NumberOptions(32)));
                expected.AddLast(new EndOfStreamToken((0, numLen + 12)));

                string t1 = JsonConvert.SerializeObject(source.Tokens, Compiler.JsonSerializer);
                string t2 = JsonConvert.SerializeObject(expected,      Compiler.JsonSerializer);
                Assert.AreEqual(t1, t2);

                #endregion
            }
        }
    }
}