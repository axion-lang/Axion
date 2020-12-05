using System;
using System.IO;
using System.Linq;
using Axion.Core;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;
using Axion.Frontend.Axion;
using NLog;
using NUnit.Framework;

namespace Axion.Testing.NUnit {
    [SetUpFixture]
    public class SpecTests {
        [OneTimeSetUp]
        public void Setup() {
            // Initialize logging
            LogManager.Configuration.Variables["consoleLogLevel"] = "Fatal";
            LogManager.Configuration.Variables["fileLogLevel"]    = "Fatal";
            LogManager.ReconfigExistingLoggers();

            // Clear debugging output
            var dbg = Path.Join(TestUtils.OutPath, "debug");
            if (Directory.Exists(dbg)) {
                foreach (var file in
                    new DirectoryInfo(dbg).EnumerateFiles()) {
                    file.Delete();
                }
            }
            else {
                Directory.CreateDirectory(dbg);
            }

            Compiler.AddTranslator("axion", new Translator());
        }

        /// <summary>
        ///     First barrier preventing silly errors.
        ///     Checks that all keywords are defined in specification.
        /// </summary>
        [Test]
        public static void SpecificationCheck() {
            // check keywords completeness
            var definedKws = Enum.GetNames(typeof(TokenType))
                                 .Where(
                                     name => name.ToUpper()
                                                 .StartsWith("KEYWORD")
                                 );

            foreach (var kw in definedKws) {
                Enum.TryParse(kw, out TokenType type);
                Assert.That(
                    Spec.Keywords.ContainsValue(type),
                    "Keyword '" + kw + "' is not defined in specification."
                );
            }
        }
    }
}
