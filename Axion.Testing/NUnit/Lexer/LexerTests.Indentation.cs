using System;
using Axion.Core;
using Axion.Core.Processing;
using Axion.Core.Processing.Source;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Lexer {
    public partial class LexerTests {
        [Test]
        public void IsOK_TabsIndentation() {
            SourceUnit unit = MakeSourceFromCode(
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
                )
            );
            LexIndent(unit);
            Assert.AreEqual(0, unit.Blames.Count);
        }

        [Test]
        public void IsOK_SpacesIndentation() {
            SourceUnit unit = MakeSourceFromCode(
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
                )
            );
            LexIndent(unit);
            Assert.AreEqual(0, unit.Blames.Count);
        }

        [Test]
        public void IsWarns_MixedIndentation() {
            SourceUnit unit = MakeSourceFromCode(
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
                )
            );
            LexIndent(unit);
            // 2 blames for mixed indentation
            Assert.AreEqual(2, unit.Blames.Count);
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
}