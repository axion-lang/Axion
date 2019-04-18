using System.Collections.Generic;
using System.Text;
using Axion.Core.Processing;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Lexer {
    public partial class LexerTests {
        [Test]
        public void IsOK_ValidNumbers() {
            string[] numbers = {
                "123456",
                "000042",
                "123.456",
                "654.321",
                "0x1689ABCDEF",
                "0x1689_ABC_DEFi64",
                "0b10110001",
                "0b1011_0001i64",
                "0o72517242",
                "0o72_517_242i64",
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
                    "0x1689_ABC_DEF",
                    new NumberOptions(16, 64) {
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
                    "0b1011_0001",
                    new NumberOptions(2, 64) {
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
                    "0o72_517_242",
                    new NumberOptions(8, 64) {
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
                    OutPath + nameof(IsOK_ValidNumbers) + i + TestExtension
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
    }
}