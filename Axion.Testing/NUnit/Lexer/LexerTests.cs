using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axion.Core;
using Axion.Core.Processing;
using Axion.Core.Processing.Lexical.Tokens;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Lexer {
    public partial class LexerTests {
        [Test]
        public void UseBlockValid() {
            var source = new SourceUnit(
                "use (\n" +
                "   System (IO, Linq),\n" +
                "   Axion (Core, Testing),\n" +
                "   Some.Long.Namespace\n" +
                ")",
                outPath + nameof(UseBlockValid) + testExtension
            );

            source.Process(SourceProcessingMode.Lex, SourceProcessingOptions.SyntaxAnalysisDebugOutput);
            Assert.AreEqual(0, source.Blames.Count);

            var expected = new List<Token> {
                // line 1
                new KeywordToken(TokenType.KeywordUse, (0, 0), " "),
                new SymbolToken((0, 4), "("),
                new EndOfLineToken((0, 5), whitespaces: "   "),
                // line 2
                new IdentifierToken((1, 3), "System", " "),
                new SymbolToken((1, 10), "("),
                new IdentifierToken((1, 11), "IO"),
                new SymbolToken((1, 13), ",", " "),
                new IdentifierToken((1, 15), "Linq"),
                new SymbolToken((1, 19), ")"),
                new SymbolToken((1, 20), ","),
                new EndOfLineToken((1, 21), whitespaces: "   "),
                // line 3
                new IdentifierToken((2, 3), "Axion", " "),
                new SymbolToken((2, 9), "("),
                new IdentifierToken((2, 10), "Core"),
                new SymbolToken((2, 14), ",", " "),
                new IdentifierToken((2, 16), "Testing"),
                new SymbolToken((2, 23), ")"),
                new SymbolToken((2, 24), ","),
                new EndOfLineToken((2, 25), whitespaces: "   "),
                // line 4
                new IdentifierToken((3, 3), "Some"),
                new SymbolToken((3, 7), "."),
                new IdentifierToken((3, 8), "Long"),
                new SymbolToken((3, 12), "."),
                new IdentifierToken((3, 13), "Namespace"),
                new EndOfLineToken((3, 22)),
                // line 5
                new SymbolToken((4, 0), ")"),
                new EndOfLineToken((4, 1)),
                new EndOfStreamToken((5, 0))
            };

            for (var i = 0; i < source.Tokens.Count; i++) {
                Assert.IsTrue(expected[i].TEquals(source.Tokens[i]));
            }

            string t1 = JsonConvert.SerializeObject(source.Tokens, Compiler.JsonSerializer);
            string t2 = JsonConvert.SerializeObject(expected,      Compiler.JsonSerializer);
            Assert.AreEqual(t1, t2);
        }

        [Test]
        public void IndentationLengthComputedCorrectly() {
            // with tabs
            var source1 = new SourceUnit(
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

            Assert.AreEqual(0, source1.Blames.Count);

            Assert.IsTrue(source1.Tokens.ElementAt(indentIndex1).TEquals(new IndentToken((2, 0), "\t")));
            Assert.IsTrue(source1.Tokens.ElementAt(indentIndex2).TEquals(new IndentToken((5, 0), "\t\t")));
            Assert.IsTrue(source1.Tokens.ElementAt(indentIndex3).TEquals(new IndentToken((6, 0), "\t\t\t")));
            Assert.IsTrue(source1.Tokens.ElementAt(indentIndex4).TEquals(new OutdentToken((7, 0))));
            Assert.IsTrue(source1.Tokens.ElementAt(indentIndex5).TEquals(new OutdentToken((8, 0))));

            // with spaces
            var source2 = new SourceUnit(
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

            Assert.AreEqual(0, source2.Blames.Count);

            Assert.IsTrue(source2.Tokens.ElementAt(indentIndex1).TEquals(new IndentToken((2, 0), new string(' ', 4))));
            Assert.IsTrue(source2.Tokens.ElementAt(indentIndex2).TEquals(new IndentToken((5, 0), new string(' ', 8))));
            Assert.IsTrue(source2.Tokens.ElementAt(indentIndex3).TEquals(new IndentToken((6, 0), new string(' ', 12))));
            Assert.IsTrue(source2.Tokens.ElementAt(indentIndex4).TEquals(new OutdentToken((7, 0))));
            Assert.IsTrue(source2.Tokens.ElementAt(indentIndex5).TEquals(new OutdentToken((8, 0))));

            // mixed
            var source3 = new SourceUnit(
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

            Assert.AreEqual(1, source3.Blames.Count);

            Assert.IsTrue(source3.Tokens.ElementAt(indentIndex1).TEquals(new IndentToken((2, 0), "\t")));
            Assert.IsTrue(source3.Tokens.ElementAt(indentIndex2).TEquals(new IndentToken((5, 0), new string(' ', 12))));
            Assert.IsTrue(source3.Tokens.ElementAt(indentIndex3).TEquals(new IndentToken((6, 0), new string(' ', 12) + "\t")));
            Assert.IsTrue(source3.Tokens.ElementAt(indentIndex4).TEquals(new OutdentToken((7, 0))));
            Assert.IsTrue(source3.Tokens.ElementAt(indentIndex5).TEquals(new OutdentToken((8, 0))));

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
                "0x1689ABCDEF",
                "0b10110001",
                "0o72517242",
                "4j",
                "12e5"
            };
            var pos = (0, 9);
            // TODO complete numbers parsing test
            Token[] tokens = {
                new NumberToken(
                    pos, "123456", new NumberOptions { Number = new StringBuilder("123456") }
                ),
                new NumberToken(
                    pos, "654321", new NumberOptions { Number = new StringBuilder("654321") }
                ),
                new NumberToken(
                    pos, "123.456", new NumberOptions(10, 32, true) { Number = new StringBuilder("123.456") }
                ),
                new NumberToken(
                    pos, "654.321", new NumberOptions(10, 32, true) { Number = new StringBuilder("654.321") }
                ),
                new NumberToken(
                    pos, "0x1689ABCDEF", new NumberOptions(16) { Number = new StringBuilder("1689ABCDEF") }
                ),
                new NumberToken(
                    pos, "0b10110001", new NumberOptions(2) { Number = new StringBuilder("10110001") }
                ),
                new NumberToken(
                    pos, "0o72517242", new NumberOptions(8) { Number = new StringBuilder("72517242") }
                ),
                new NumberToken(
                    pos, "4j", new NumberOptions(10, 32, false, true) { Number = new StringBuilder("4") }
                ),
                new NumberToken(
                    pos, "12e5", new NumberOptions(10, 32, false, false, false, false, true, 5) { Number = new StringBuilder("12") }
                )
            };
            foreach (Token token in tokens) {
                token.AppendWhitespace(" ");
            }

            Assert.AreEqual(numbers.Length, tokens.Length);

            for (var i = 0; i < numbers.Length; i++) {
                var source = new SourceUnit(
                    "number = " + numbers[i] + " + 0b10010010",
                    outPath + nameof(NumbersParsedCorrectly) + i + testExtension
                );
                Assert.DoesNotThrow(
                    () => source.Process(
                        SourceProcessingMode.Lex,
                        SourceProcessingOptions.SyntaxAnalysisDebugOutput
                    )
                );

                var expected = new List<Token> {
                    new IdentifierToken((0, 0), "number", " "),
                    new SymbolToken((0, 7), "=", " "),
                    tokens[i],
                    new OperatorToken((0, tokens[i].Span.End.Column), "+", " "),
                    new NumberToken((0, tokens[i].Span.End.Column + 2), "0b10010010", new NumberOptions(32)),
                    new EndOfLineToken((0, tokens[i].Span.End.Column + 12)),
                    new EndOfStreamToken((1, 0))
                };

                for (var k = 0; k < source.Tokens.Count; k++) {
                    if (k == 2 &&
                        source.Tokens[k] is NumberToken num
                     && expected[k] is NumberToken num2) {
                        Assert.IsTrue(num.Options.TestEquality(num2.Options));
                    }
                    else {
                        Assert.IsTrue(expected[k].TEquals(source.Tokens[k]));
                    }
                }
            }
        }
    }

    internal static class TUtils {
        internal static bool TEquals(this Token a, Token b) {
            return a.Type == b.Type
                && string.Equals(a.Value,       b.Value)
                && string.Equals(a.Whitespaces, b.Whitespaces);
        }

        internal static bool TestEquality(this NumberOptions t, NumberOptions other) {
            // don't check value equality
            return t.Number.ToString() == other.Number.ToString()
                && t.Radix == other.Radix
                && t.Bits == other.Bits
                && t.Floating == other.Floating
                && t.Imaginary == other.Imaginary
                && t.Unsigned == other.Unsigned
                && t.Unlimited == other.Unlimited
                && t.HasExponent == other.HasExponent
                && t.Exponent == other.Exponent;
        }
    }
}