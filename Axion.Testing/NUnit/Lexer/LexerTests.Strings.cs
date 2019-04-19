using System;
using Axion.Core.Processing;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Lexer {
    public partial class LexerTests {
        [Test]
        public void IsOK_VariousStrings() {
            SourceUnit unit = MakeSourceFromCode(
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
            Lex(unit);
            // 2 warns for redundant prefixes for empty strings
            Assert.AreEqual(2, unit.Blames.Count);
        }

        [Test]
        public void IsFail_StringInvalidEscape() {
            SourceUnit unit = MakeSourceFromCode(
                string.Join(
                    Environment.NewLine,
                    "'invalid -> \\m <- escape!'"
                )
            );
            Lex(unit);
            Assert.AreEqual(1, unit.Blames.Count);
        }

        [Test]
        public void IsFail_StringTruncatedUEscape() {
            SourceUnit unit = MakeSourceFromCode(
                string.Join(
                    Environment.NewLine,
                    "'invalid -> \\U5 <- escape!'",
                    "'invalid -> \\u2 <- escape!'"
                )
            );
            Lex(unit);
            Assert.AreEqual(2, unit.Blames.Count);
        }

        [Test]
        public void IsOK_StringEscSequences() {
            SourceUnit unit = MakeSourceFromCode(
                string.Join(
                    Environment.NewLine,
                    "str1e = \"esc: \\r\\n\\f\\t\\v\"",
                    "str2e = 'esc: \\u2323\\U00123456\\x24'"
                )
            );
            Lex(unit);
            Assert.AreEqual(0, unit.Blames.Count);
        }
    }
}