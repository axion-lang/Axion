using System;
using Axion.Core.Source;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Lexer {
    public partial class LexerTests {
        [Test]
        public void TestVariousStrings() {
            SourceUnit src = MakeSourceFromCode(
                string.Join(
                    Environment.NewLine,
                    "str1 = \"regular literal\"",
                    "str2 = 'regular literal'",
                    "",
                    "",
                    "str1f = f\"{str1} formatted literal {str2}\"",
                    "str2f = f'{str1} formatted literal {str2}'",
                    "",
                    "",
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
                    "'''",
                    "",
                    "",
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
                    "'''",
                    "",
                    "",
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
            SourceUnit src = MakeSourceFromCode(
                string.Join(
                    Environment.NewLine,
                    "'invalid -> \\m <- escape!'"
                )
            );
            Lex(src);
            Assert.AreEqual(1, src.Blames.Count);
        }

        [Test]
        public void TestFailStringTruncatedUEscape() {
            SourceUnit src = MakeSourceFromCode(
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
            SourceUnit src = MakeSourceFromCode(
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