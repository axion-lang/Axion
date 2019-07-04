using System.Collections.Generic;
using Axion.Core.Processing;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Source;
using Axion.Core.Specification;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Lexer {
    public partial class LexerTests {
        [Test]
        public void IsOK_ValidNumbers() {
            string[] numbers = {
                "123456",
                "654321",
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
                    new NumberOptions("123456"),
                    pos
                ),
                new NumberToken(
                    "654321",
                    new NumberOptions("654321"),
                    pos
                ),
                new NumberToken(
                    "123.456",
                    new NumberOptions("123.456", 10, 32, true),
                    pos
                ),
                new NumberToken(
                    "654.321",
                    new NumberOptions("654.321", 10, 32, true),
                    pos
                ),
                new NumberToken(
                    "0x1689ABCDEF",
                    new NumberOptions("1689ABCDEF", 16),
                    pos
                ),
                new NumberToken(
                    "0x1689_ABC_DEF",
                    new NumberOptions("1689ABCDEF", 16, 64),
                    pos
                ),
                new NumberToken(
                    "0b10110001",
                    new NumberOptions("10110001", 2),
                    pos
                ),
                new NumberToken(
                    "0b1011_0001",
                    new NumberOptions("10110001", 2, 64),
                    pos
                ),
                new NumberToken(
                    "0o72517242",
                    new NumberOptions("72517242", 8),
                    pos
                ),
                new NumberToken(
                    "0o72_517_242",
                    new NumberOptions("72517242", 8, 64),
                    pos
                ),
                new NumberToken(
                    "4j",
                    new NumberOptions("4", 10, 32, false, true),
                    pos
                ),
                new NumberToken(
                    "12e5",
                    new NumberOptions(
                        "12e5",
                        10,
                        32,
                        false,
                        false,
                        false,
                        false,
                        true,
                        5
                    ),
                    pos
                )
            };
            foreach (Token token in tokens) {
                token.AppendWhitespace(" ");
            }

            Assert.AreEqual(numbers.Length, tokens.Length);

            for (var i = 0; i < numbers.Length; i++) {
                var source = new SourceUnit(
                    "number = " + numbers[i],
                    OutPath + nameof(IsOK_ValidNumbers) + i + TestExtension
                );
                Assert.DoesNotThrow(() => Lex(source));

                var expected = new List<Token> {
                    new WordToken("number", (0, 0)).AppendWhitespace(" "),
                    new OperatorToken("=", (0, 7)).AppendWhitespace(" "),
                    tokens[i],
                    new Token(TokenType.End, (0, tokens[i].Span.End.Column))
                };

                for (var k = 0; k < source.Tokens.Count; k++) {
                    if (k == 2
                        && source.Tokens[k] is NumberToken num
                        && expected[k] is NumberToken num2) {
                        Assert.That(
                            num.Options == num2.Options,
                            $"{num.Options}\n{num2.Options}"
                        );
                    }
                    else {
                        Assert.That(
                            expected[k] == source.Tokens[k],
                            $"{expected[k]}\n{source.Tokens[k]}"
                        );
                    }
                }
            }
        }
    }
}