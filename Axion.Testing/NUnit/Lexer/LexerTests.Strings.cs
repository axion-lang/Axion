using System;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Lexer {
    public partial class LexerTests {
        [Test]
        public void TestRegularStrings() {
            var src = TestUtils.UnitFromCode(
                string.Join(
                    Environment.NewLine,
                    "str1 = \"regular literal\"",
                    "str2 = 'regular literal'"
                )
            );
            Lex(src);
            Assert.AreEqual(0, src.Blames.Count);
        }

        [Test]
        public void TestFormattedStrings() {
            var src = TestUtils.UnitFromCode(
                string.Join(
                    Environment.NewLine,
                    "str1f = f\"{str1} formatted literal {str2}\"",
                    "str2f = f'{str1} formatted literal {str2}'"
                )
            );
            Lex(src);
            Assert.AreEqual(0, src.Blames.Count);
        }

        [Test]
        public void TestMultilineStrings() {
            var src = TestUtils.UnitFromCode(
                string.Join(
                    Environment.NewLine,
                    "str1m = \"\"\"multiline literal\"\"\"",
                    "str2m = '''multiline literal'''",
                    "",
                    "",
                    "str1m_ = \"\"\"",
                    "multiline literal",
                    "\"\"\"",
                    "",
                    "str2m_ = '''",
                    "multiline literal",
                    "'''"
                )
            );
            Lex(src);
            Assert.AreEqual(0, src.Blames.Count);
        }

        [Test]
        public void TestFormattedMultilineStrings() {
            var src = TestUtils.UnitFromCode(
                string.Join(
                    Environment.NewLine,
                    "str1fm = f\"\"\"{str1} formatted multiline literal {str2}\"\"\"",
                    "str2fm = f'''{str1} formatted multiline literal {str2}'''",
                    "",
                    "",
                    "str1fm_ = f\"\"\"",
                    "{str1} formatted multiline literal {str2}",
                    "\"\"\"",
                    "",
                    "str2fm_ = f'''",
                    "{str1} formatted multiline literal {str2}",
                    "'''"
                )
            );
            Lex(src);
            Assert.AreEqual(0, src.Blames.Count);
        }

        [Test]
        public void TestEmptyStrings() {
            var src = TestUtils.UnitFromCode(
                string.Join(
                    Environment.NewLine,
                    "str1e = \"\"",
                    "str2e = ''",
                    "",
                    "",
                    "str1fe = f\"\"",
                    "str2fe = f''",
                    "",
                    "",
                    "str1me = \"\"\"\"\"\"",
                    "str2me = ''''''",
                    "",
                    "",
                    "str1me_ = \"\"\"",
                    "\"\"\"",
                    "str2me_ = '''",
                    "'''"
                )
            );
            Lex(src);
            // 2 warns for redundant prefixes for empty strings
            Assert.AreEqual(2, src.Blames.Count);
        }

        [Test]
        public void TestFailStringInvalidEscape() {
            var src = TestUtils.UnitFromCode("'invalid -> \\m <- escape!'");
            Lex(src);
            Assert.AreEqual(1, src.Blames.Count);
        }

        [Test]
        public void TestFailStringTruncatedUEscape() {
            var src = TestUtils.UnitFromCode(
                string.Join(
                    Environment.NewLine,
                    "'invalid -> \\U5 <- escape!'",
                    "'invalid -> \\u2 <- escape!'"
                )
            );
            Lex(src);
            Assert.AreEqual(2, src.Blames.Count);
        }

        [Test]
        public void TestStringEscSequences() {
            var src = TestUtils.UnitFromCode(
                string.Join(
                    Environment.NewLine,
                    "str1e = \"esc: \\r\\n\\f\\t\\v\"",
                    "str2e = 'esc: \\u2323\\U0010ffff\\x24'"
                )
            );
            Lex(src);
            Assert.AreEqual(0, src.Blames.Count);
        }
    }
}
