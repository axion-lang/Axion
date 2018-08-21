//using System;
//using Axion.Core;
//using Axion.Processing;
//using NUnit.Framework;

//namespace Axion.Testing.NUnit {
//    [TestFixture]
//    internal class ExpressionsParserTests {
//        /// <summary>
//        ///     UNSTABLE TEST!
//        /// </summary>
//        [Test]
//        public void CalculateConstants(string expr, double expected) {
//            var source = new SourceCode(expr);
//            Compiler.Process(source, SourceProcessingMode.Lex);
//            var tokens = source.Tokens;
//            var result = double.Parse(EParser.Process(tokens).Value);
//            Assert.That(Math.Abs(result - expected) < 0.0000000000000001d);
//        }

//        /// <summary>
//        ///     UNSTABLE TEST!
//        /// </summary>
//        [Test]
//        public void ExpressionsParser_Global() {
//            Assert.DoesNotThrow(
//                () => {
//                    CalculateConstants("1-2",                 1 - 2);
//                    CalculateConstants("(((-5.5)))",          -5.5);
//                    CalculateConstants("(1-(2))",             1 - 2);
//                    CalculateConstants("3+2*6-1",             3 + 2 * 6 - 1);
//                    CalculateConstants("3-2*6-1",             3 - 2 * 6 - 1);
//                    CalculateConstants("1-2-3-(4-(5-(6-7)))", 1 - 2 - 3 - (4 - (5 - (6 - 7))));
//                    //OPP_CalculateConstants("2-3*sin(pi)", 2 - 3 * Math.Sin(Math.PI));
//                    //OPP_CalculateConstants("1-(exp(10*7-sqrt((1+1)*20*10)))", 1 - Math.Exp(10 * 7 - Math.Sqrt((1 + 1) * 20 * 10)));
//                    CalculateConstants("3-(5-6)-(2-(3-(1-2)))", 3 - (5 - 6) - (2 - (3 - (1 - 2))));
//                    CalculateConstants(
//                        "3-(5-6)-(2-(3-(1+2)))+2-(-1+7)*(9-2)/((16-3)-3)+15/2*5",
//                        3 - (5 - 6) - (2 - (3 - (1 + 2))) + 2 - (-1 + 7) * (9 - 2) / (16.0 - 3 - 3.0) +
//                        15 / 2.0 * 5
//                    );
//                    CalculateConstants("(-1+7)*(9-2)",      (-1 + 7) * (9 - 2));
//                    CalculateConstants("((16-3)-3)+15/2*5", 16 - 3 - 3 + 15 / 2.0 * 5);
//                    CalculateConstants("1+15/2*5",          1 + 15 / 2.0 * 5);
//                    CalculateConstants("3-2/6-1",           3 - 2 / 6.0 - 1);
//                    //OPP_CalculateConstants("3*50-3*2^4*3", 3 * 50 - 3 * Math.Pow(2, 4) * 3);
//                    //OPP_CalculateConstants("5-1/2^2-3", 5 - 1 / Math.Pow(2, 2) - 3);
//                    CalculateConstants("(((1/4/2-(8/2/3+5))))", 1 / 4.0 / 2.0 - (8 / 2.0 / 3.0 + 5));
//                    //OPP_CalculateConstants("pow(2,3)", Math.Pow(2, 3));
//                    //OPP_CalculateConstants("abs(3*-50-2*3/4)/3*2", Math.Abs(3.0 * -50 - 2 * 3.0 / 4.0) / 3.0 * 2);
//                }
//            );
//        }
//    }
//}

