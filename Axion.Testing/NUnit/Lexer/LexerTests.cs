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
        public void IndentationLengthComputedCorrectly() {
            // with tabs
            var source1 = new SourceUnit(
                "i = 0\r\n"
                + "while i < 10:\r\n"
                + "\tconsole.print 'OK.'\r\n"
                + "\tj = 0\r\n"
                + "\twhile j < 5:\r\n"
                + "\t\tif i == 3 and j == 2:\r\n"
                + "\t\t\tconsole.print 'Got it!'\r\n"
                + "\t\tj++\r\n"
                + "\ti++\r\n"
                + "",
                OutPath + nameof(IndentationLengthComputedCorrectly) + "_tabs" + TestExtension
            );

            const int indentIndex1 = 10;
            const int indentIndex2 = 26;
            const int indentIndex3 = 37;
            const int indentIndex4 = 43;
            const int indentIndex5 = 47;

            Compiler.Process(
                source1,
                SourceProcessingMode.Lex,
                SourceProcessingOptions.SyntaxAnalysisDebugOutput
                | SourceProcessingOptions.CheckIndentationConsistency
            );

            Assert.AreEqual(0, source1.Blames.Count);

            Assert.IsTrue(
                source1.Tokens.ElementAt(indentIndex1).TokenEquals(new IndentToken("\t", (2, 0)))
            );
            Assert.IsTrue(
                source1.Tokens.ElementAt(indentIndex2).TokenEquals(new IndentToken("\t\t", (5, 0)))
            );
            Assert.IsTrue(
                source1.Tokens.ElementAt(indentIndex3)
                       .TokenEquals(new IndentToken("\t\t\t", (6, 0)))
            );
            Assert.IsTrue(
                source1.Tokens.ElementAt(indentIndex4).TokenEquals(new OutdentToken((7, 0)))
            );
            Assert.IsTrue(
                source1.Tokens.ElementAt(indentIndex5).TokenEquals(new OutdentToken((8, 0)))
            );

            // with spaces
            var source2 = new SourceUnit(
                "i = 0\r\n"
                + "while i < 10:\r\n"
                + "    console.print 'OK.'\r\n"
                + "    j = 0\r\n"
                + "    while j < 5:\r\n"
                + "        if i == 3 and j == 2:\r\n"
                + "            console.print 'Got it!'\r\n"
                + "        j++\r\n"
                + "    i++\r\n"
                + "",
                OutPath + nameof(IndentationLengthComputedCorrectly) + "_spaces" + TestExtension
            );

            Compiler.Process(
                source2,
                SourceProcessingMode.Lex,
                SourceProcessingOptions.SyntaxAnalysisDebugOutput
                | SourceProcessingOptions.CheckIndentationConsistency
            );

            Assert.AreEqual(0, source2.Blames.Count);

            Assert.IsTrue(
                source2.Tokens.ElementAt(indentIndex1)
                       .TokenEquals(new IndentToken(new string(' ', 4), (2, 0)))
            );
            Assert.IsTrue(
                source2.Tokens.ElementAt(indentIndex2)
                       .TokenEquals(new IndentToken(new string(' ', 8), (5, 0)))
            );
            Assert.IsTrue(
                source2.Tokens.ElementAt(indentIndex3)
                       .TokenEquals(new IndentToken(new string(' ', 12), (6, 0)))
            );
            Assert.IsTrue(
                source2.Tokens.ElementAt(indentIndex4).TokenEquals(new OutdentToken((7, 0)))
            );
            Assert.IsTrue(
                source2.Tokens.ElementAt(indentIndex5).TokenEquals(new OutdentToken((8, 0)))
            );

            // mixed
            var source3 = new SourceUnit(
                // use double indent at line 5 to distinct 8-space tab from spaces.
                "i = 0\r\n"
                + // 1
                "while i < 10:\r\n"
                + // 2
                "\tconsole.print 'OK.'\r\n"
                + // 3
                "\tj = 0\r\n"
                + // 4
                "\twhile j < 5:\r\n"
                + // 5
                "            if i == 3 and j == 2:\r\n"
                + // 5
                "            \tconsole.print 'Got it!'\r\n"
                + // 7
                "            j++\r\n"
                + // 8
                "        i++\r\n"
                + // 9
                "",
                OutPath + nameof(IndentationLengthComputedCorrectly) + "_mixed" + TestExtension
            );

            Compiler.Process(
                source3,
                SourceProcessingMode.Lex,
                SourceProcessingOptions.SyntaxAnalysisDebugOutput
                | SourceProcessingOptions.CheckIndentationConsistency
            );

            Assert.AreEqual(1, source3.Blames.Count);

            Assert.IsTrue(
                source3.Tokens.ElementAt(indentIndex1).TokenEquals(new IndentToken("\t", (2, 0)))
            );
            Assert.IsTrue(
                source3.Tokens.ElementAt(indentIndex2)
                       .TokenEquals(new IndentToken(new string(' ', 12), (5, 0)))
            );
            Assert.IsTrue(
                source3.Tokens.ElementAt(indentIndex3)
                       .TokenEquals(new IndentToken(new string(' ', 12) + "\t", (6, 0)))
            );
            Assert.IsTrue(
                source3.Tokens.ElementAt(indentIndex4).TokenEquals(new OutdentToken((7, 0)))
            );
            Assert.IsTrue(
                source3.Tokens.ElementAt(indentIndex5).TokenEquals(new OutdentToken((8, 0)))
            );

            string TokenString(Token tk) {
                return "(" + tk.Value.Replace("\t", "    ") + ")";
            }

            Assert.AreEqual(
                string.Join(";", source1.Tokens.Select(TokenString)),
                string.Join(";", source2.Tokens.Select(TokenString))
            );
            Assert.AreEqual(source2.Tokens.Count, source3.Tokens.Count);
        }

        [Test]
        public void NumbersParsedCorrectly() {
            string[] numbers = {
                "123456",
                "654321",
                "123.456",
                "654.321",
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
                    "123456",
                    new NumberOptions {
                        Number = new StringBuilder("123456")
                    },
                    pos
                ),
                new NumberToken(
                    "654321",
                    new NumberOptions {
                        Number = new StringBuilder("654321")
                    },
                    pos
                ),
                new NumberToken(
                    "123.456",
                    new NumberOptions(10, 32, true) {
                        Number = new StringBuilder("123.456")
                    },
                    pos
                ),
                new NumberToken(
                    "654.321",
                    new NumberOptions(10, 32, true) {
                        Number = new StringBuilder("654.321")
                    },
                    pos
                ),
                new NumberToken(
                    "0x1689ABCDEF",
                    new NumberOptions(16) {
                        Number = new StringBuilder("1689ABCDEF")
                    },
                    pos
                ),
                new NumberToken(
                    "0b10110001",
                    new NumberOptions(2) {
                        Number = new StringBuilder("10110001")
                    },
                    pos
                ),
                new NumberToken(
                    "0o72517242",
                    new NumberOptions(8) {
                        Number = new StringBuilder("72517242")
                    },
                    pos
                ),
                new NumberToken(
                    "4j",
                    new NumberOptions(10, 32, false, true) {
                        Number = new StringBuilder("4")
                    },
                    pos
                ),
                new NumberToken(
                    "12e5",
                    new NumberOptions(
                        10,
                        32,
                        false,
                        false,
                        false,
                        false,
                        true,
                        5
                    ) {
                        Number = new StringBuilder("12")
                    },
                    pos
                )
            };
            foreach (Token token in tokens) {
                token.AppendWhitespace(" ");
            }

            Assert.AreEqual(numbers.Length, tokens.Length);

            for (var i = 0; i < numbers.Length; i++) {
                var source = new SourceUnit(
                    "number = " + numbers[i] + " + 0b10010010",
                    OutPath + nameof(NumbersParsedCorrectly) + i + TestExtension
                );
                Assert.DoesNotThrow(
                    () => Compiler.Process(
                        source,
                        SourceProcessingMode.Lex,
                        SourceProcessingOptions.SyntaxAnalysisDebugOutput
                    )
                );

                var expected = new List<Token> {
                    new IdentifierToken("number", (0, 0)).AppendWhitespace(" "),
                    new SymbolToken("=", (0, 7)).AppendWhitespace(" "),
                    tokens[i],
                    new OperatorToken("+", (0, tokens[i].Span.EndPosition.Column))
                        .AppendWhitespace(" "),
                    new NumberToken(
                        "0b10010010",
                        new NumberOptions(32),
                        (0, tokens[i].Span.EndPosition.Column + 2)
                    ),
                    new EndOfLineToken(startPosition: (0, tokens[i].Span.EndPosition.Column + 12)),
                    new EndOfCodeToken((1, 0))
                };

                for (var k = 0; k < source.Tokens.Count; k++) {
                    if (k == 2
                        && source.Tokens[k] is NumberToken num
                        && expected[k] is NumberToken num2) {
                        Assert.IsTrue(num.Options.TestEquality(num2.Options));
                    }
                    else {
                        Assert.IsTrue(expected[k].TokenEquals(source.Tokens[k]));
                    }
                }
            }
        }

        [Test]
        public void UseBlockValid() {
            var source = new SourceUnit(
                "use (\n"
                + "   System (IO, Linq),\n"
                + "   Axion (Core, Testing),\n"
                + "   Some.Long.Namespace\n"
                + ")",
                OutPath + nameof(UseBlockValid) + TestExtension
            );

            Compiler.Process(
                source,
                SourceProcessingMode.Lex,
                SourceProcessingOptions.SyntaxAnalysisDebugOutput
            );
            Assert.AreEqual(0, source.Blames.Count);

            var expected = new List<Token> {
                // line 1
                new KeywordToken(TokenType.KeywordUse, (0, 0)).AppendWhitespace(" "),
                new SymbolToken("(", (0, 4)),
                new EndOfLineToken("\n   ", (0, 5)),
                // line 2
                new IdentifierToken("System", (1, 3)).AppendWhitespace(" "),
                new SymbolToken("(", (1, 10)),
                new IdentifierToken("IO", (1, 11)),
                new SymbolToken(",", (1, 13)).AppendWhitespace(" "),
                new IdentifierToken("Linq", (1, 15)),
                new SymbolToken(")", (1, 19)),
                new SymbolToken(",", (1, 20)),
                new EndOfLineToken("\n   ", (1, 21)),
                // line 3
                new IdentifierToken("Axion", (2, 3)).AppendWhitespace(" "),
                new SymbolToken("(", (2, 9)),
                new IdentifierToken("Core", (2, 10)),
                new SymbolToken(",", (2, 14)).AppendWhitespace(" "),
                new IdentifierToken("Testing", (2, 16)),
                new SymbolToken(")", (2, 23)),
                new SymbolToken(",", (2, 24)),
                new EndOfLineToken("\n   ", (2, 25)),
                // line 4
                new IdentifierToken("Some", (3, 3)),
                new SymbolToken(".", (3, 7)),
                new IdentifierToken("Long", (3, 8)),
                new SymbolToken(".", (3, 12)),
                new IdentifierToken("Namespace", (3, 13)),
                new EndOfLineToken(startPosition: (3, 22)),
                // line 5
                new SymbolToken(")", (4, 0)),
                new EndOfLineToken(startPosition: (4, 1)),
                new EndOfCodeToken((5, 0))
            };

            for (var i = 0; i < source.Tokens.Count; i++) {
                Assert.IsTrue(
                    expected[i].TokenEquals(source.Tokens[i]),
                    $"expected[{i}].TokenEquals(source.Tokens[{i}])"
                );
            }

            string t1 = JsonConvert.SerializeObject(source.Tokens, Compiler.JsonSerializer);
            string t2 = JsonConvert.SerializeObject(expected, Compiler.JsonSerializer);
            Assert.AreEqual(t1, t2);
        }
    }

    internal static class TokenUtils {
        internal static bool TokenEquals(this Token a, Token b) {
            return a.Is(b.Type)
                   && string.Equals(a.Value, b.Value)
                   && string.Equals(a.Whitespaces, b.Whitespaces);
        }

        internal static bool TestEquality(this NumberOptions t, NumberOptions other) {
            // don't check value equality
            return t.Radix == other.Radix
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