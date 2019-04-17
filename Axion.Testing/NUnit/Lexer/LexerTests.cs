using System;
using System.Collections.Generic;
using System.Text;
using Axion.Core;
using Axion.Core.Processing;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Lexer {
    public partial class LexerTests {
        [Test]
        public void IndentationLengthComputedCorrectly() {
            // with tabs
            var unit = new SourceUnit(
                string.Join(
                    Environment.NewLine,
                    "i = 0",
                    "while i < 10:",
                    "\tj = 0",
                    "\twhile j < 5:",
                    "\t\tif i == 3 and j == 2:",
                    "\t\t\tConsole.print('Got it!')",
                    "\t\tj++",
                    "\ti++"
                ),
                OutPath + nameof(IndentationLengthComputedCorrectly) + "_tabs" + TestExtension
            );

            LexIndent(unit);

            Assert.AreEqual(0, unit.Blames.Count);

            // with spaces
            unit = new SourceUnit(
                string.Join(
                    Environment.NewLine,
                    "i = 0",
                    "while i < 10:",
                    "    j = 0",
                    "    while j < 5:",
                    "        if i == 3 and j == 2:",
                    "            Console.print('Got it!')",
                    "        j++",
                    "    i++"
                ),
                OutPath + nameof(IndentationLengthComputedCorrectly) + "_spaces" + TestExtension
            );

            LexIndent(unit);

            Assert.AreEqual(0, unit.Blames.Count);

            // mixed
            unit = new SourceUnit(
                string.Join(
                    Environment.NewLine,
                    "i = 0",
                    "while i < 10:",
                    "\tj = 0",
                    "    while j < 5:",
                    "\t\tif i == 3 and j == 2:",
                    "            Console.print('Got it!')",
                    "\t\tj++",
                    "\ti++"
                ),
                OutPath + nameof(IndentationLengthComputedCorrectly) + "_mixed" + TestExtension
            );

            LexIndent(unit);

            Assert.AreEqual(2, unit.Blames.Count);
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
                Assert.DoesNotThrow(() => Lex(source));

                var expected = new List<Token> {
                    new WordToken("number", (0, 0)).AppendWhitespace(" "),
                    new OperatorToken("=", (0, 7)).AppendWhitespace(" "),
                    tokens[i],
                    new OperatorToken("+", (0, tokens[i].Span.EndPosition.Column))
                        .AppendWhitespace(" "),
                    new NumberToken(
                        "0b10010010",
                        new NumberOptions(32),
                        (0, tokens[i].Span.EndPosition.Column + 2)
                    ),
                    new Token(TokenType.End, (0, tokens[i].Span.EndPosition.Column + 12))
                };

                for (var k = 0; k < source.Tokens.Count; k++) {
                    if (k == 2
                        && source.Tokens[k] is NumberToken num
                        && expected[k] is NumberToken num2) {
                        Assert.IsTrue(num.Options.TestEquality(num2.Options));
                    }
                    else {
                        Assert.That(
                            expected[k].TokenEquals(source.Tokens[k]),
                            $"{expected[k]}\n{source.Tokens[k]}"
                        );
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

            Lex(source);
            Assert.AreEqual(0, source.Blames.Count);

            var expected = new List<Token> {
                // line 1
                new WordToken(TokenType.KeywordUse, (0, 0)).AppendWhitespace(" "),
                new SymbolToken("(", (0, 4)),
                new NewlineToken("\n   ", (0, 5)),
                // line 2
                new WordToken("System", (1, 3)).AppendWhitespace(" "),
                new SymbolToken("(", (1, 10)),
                new WordToken("IO", (1, 11)),
                new SymbolToken(",", (1, 13)).AppendWhitespace(" "),
                new WordToken("Linq", (1, 15)),
                new SymbolToken(")", (1, 19)),
                new SymbolToken(",", (1, 20)),
                new NewlineToken("\n   ", (1, 21)),
                // line 3
                new WordToken("Axion", (2, 3)).AppendWhitespace(" "),
                new SymbolToken("(", (2, 9)),
                new WordToken("Core", (2, 10)),
                new SymbolToken(",", (2, 14)).AppendWhitespace(" "),
                new WordToken("Testing", (2, 16)),
                new SymbolToken(")", (2, 23)),
                new SymbolToken(",", (2, 24)),
                new NewlineToken("\n   ", (2, 25)),
                // line 4
                new WordToken("Some", (3, 3)),
                new SymbolToken(".", (3, 7)),
                new WordToken("Long", (3, 8)),
                new SymbolToken(".", (3, 12)),
                new WordToken("Namespace", (3, 13)),
                new NewlineToken(startPosition: (3, 22)),
                // line 5
                new SymbolToken(")", (4, 0)),
                new Token(TokenType.End, (4, 1))
            };

            for (var i = 0; i < source.Tokens.Count; i++) {
                Assert.IsTrue(
                    expected[i].TokenEquals(source.Tokens[i]),
                    $"{expected[i]}\n{source.Tokens[i]}"
                );
            }

            string t1 = JsonConvert.SerializeObject(source.Tokens, Compiler.JsonSerializer);
            string t2 = JsonConvert.SerializeObject(expected, Compiler.JsonSerializer);
            Assert.AreEqual(t1, t2);
        }

        private static void Lex(SourceUnit source) {
            Compiler.Process(
                source,
                SourceProcessingMode.Lex,
                SourceProcessingOptions.SyntaxAnalysisDebugOutput
            );
        }

        private static void LexIndent(SourceUnit source) {
            Compiler.Process(
                source,
                SourceProcessingMode.Lex,
                SourceProcessingOptions.SyntaxAnalysisDebugOutput
                | SourceProcessingOptions.CheckIndentationConsistency
            );
        }
    }

    internal static class TokenUtils {
        internal static bool TokenEquals(this Token a, Token b) {
            return a.Is(b.Type)
                   && string.Equals(a.Value, b.Value)
                   && string.Equals(a.EndWhitespaces, b.EndWhitespaces);
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