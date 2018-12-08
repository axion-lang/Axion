//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using Axion.Tokens;
//using Axion.Tokens.Ast;

//namespace Axion.Processing {
//    /// <summary>
//    ///     Experimental operator-precedence based syntax tree builder.
//    ///     [UNSTABLE]
//    /// </summary>
//    public class EParser {
//        private static LinkedList<Token>     inputTokens;
//        private static LinkedListNode<Token> node;

//        public static OperationToken Process(LinkedList<Token> input) {
//            inputTokens = input;
//            node        = inputTokens.First;

//            return LoadAndCalculate(input.Last.Value.Value);
//        }

//        internal static OperationToken LoadAndCalculate(string toValue) {
//            var cells = new List<Cell>();
//            do {
//                Token         value;
//                OperatorToken action;
//                switch (node.Value) {
//                    case OperatorToken tk_op: {
//                        action = tk_op;
//                        // TODO ExprParser: mark op as unary case
//                        // for prefix operators
//                        // ++i - 40
//                        if (node.Previous == null ||
//                            node.Previous.Value is OperatorToken prevOp) {
//                        }
//                        // for postfix operators
//                        // i++ - 40
//                        if (node.Next == null ||
//                            node.Next.Value is OperatorToken nextOp) {
//                        }
//                        // if extracted item is a function or if the next item is starting with a '('
//                        if (tk_op.Value == "(") {
//                            node  = node.Next;
//                            value = LoadAndCalculate(")").LeftOperand;
//                        }
//                        else {
//                            node  = node.Next;
//                            value = node.Value;
//                            node  = node.Next;
//                        }
//                        break;
//                    }
//                    case Token tk_val: {
//                        value = tk_val;
//                        if (!(node.Next.Value is OperatorToken)) {
//                            throw new Exception("val follows val.");
//                        }
//                        node   = node.Next;
//                        action = (OperatorToken) node.Value;
//                        node   = node.Next;
//                        break;
//                    }
//                    default: {
//                        throw new ArgumentException("Could not parse token: " + node.Value);
//                    }
//                }
//                cells.Add(new Cell(value, action));
//            } while (node.Next != null && node.Value.Value != toValue);
//            var ind = 1;
//            return Merge(cells[0], ref ind, cells);
//        }

//        //private static OperationToken output = new OperationToken(null, null, null);

//        /*internal static OperationToken recf(string toValue) {
//          while (node.Value.Value != toValue) {
//             node = node.Next;
//             switch (node.Value) {
//                case OperatorToken op: {
//                   // if previous token is null or operator and operator can be unary
//                   bool prefixUnaryOp =
//                      // if token is first (-5 + 2)
//                      (node == inputTokens.First ||
//                       // or if previous token is operator  (2 + -5)
//                       node.Previous.Value is OperatorToken) &&
//                      // and if operator can be unary
//                      op.Properties.InputSide == InputSide.SomeOne;

//                   // if next token is null or operator and operator can be unary
//                   bool postfixUnaryOp =
//                      // if token is last (5 + 2++)
//                      (node == inputTokens.Last ||
//                       // or if previous token is operator  (5++ - 2)
//                       node.Next.Value is OperatorToken) &&
//                      // and if operator can be unary 
//                      op.Properties.InputSide == InputSide.SomeOne;
//                   if (!prefixUnaryOp && node == inputTokens.First) {
//                      throw new Exception("prefix operator at invalid place.");
//                   }
//                   if (!postfixUnaryOp && node == inputTokens.Last) {
//                      throw new Exception("postfix operator at invalid place.");
//                   }

//                   // ++i (- 40)
//                   if (prefixUnaryOp) {
//                      output.OperatorToken = op;
//                      return new OperationToken(op, null, recf(ref from, toValue));
//                   }
//                   // i++ (- 40)
//                   if (postfixUnaryOp) {
//                      return new OperationToken(op, recf(ref from, toValue), null);
//                   }

//                   // if it is expression in parentheses
//                   if (op.Properties.IsOpenBrace) {
//                      return new OperationToken(op, num, recf(ref from, op.Properties.GetMatchingBrace()));
//                   }
//                   break;
//                }
//                // if number or variable or constant.
//                default: {
//                   break;
//                }
//             }
//          }
//       }*/

//        /*private static List<Cell> Split(ParsingScript script, char[] to) {
//          var listToMerge = new List<Cell>(16);

//          if (!script.StillValid() || to.Contains(script.Current)) {
//             listToMerge.Add(Cell.EmptyInstance);
//             script.Forward();
//             return listToMerge;
//          }
//          int arrayIndexDepth = 0;
//          bool inQuotes = false;
//          int negated = 0;

//          do { // Main processing cycle of the first part.
//             // process prefix operators
//             if (inputTokens[i++] is OperatorToken opT && (
//                 opT.Properties.Value == "-" ||
//                 opT.Properties.Value == "--" ||
//                 opT.Properties.Value == "++")) {

//             }
//             string negateSymbol = Utils.IsNotSign(script.Rest);
//             if (negateSymbol != null && !inQuotes) {
//                negated++;
//                script.Forward(negateSymbol.Length);
//                continue;
//             }

//             char ch = script.CurrentAndForward();
//             CheckQuotesIndices(script, ch, ref inQuotes, ref arrayIndexDepth);
//             string action = null;

//             bool keepCollecting = inQuotes || arrayIndexDepth > 0 ||
//                                   StillCollecting(item.ToString(), to, script, ref action);
//             if (keepCollecting) {
//                // The char still belongs to the previous operand.
//                item.Append(ch);

//                bool goForMore = script.StillValid() &&
//                                 (inQuotes || arrayIndexDepth > 0 || !to.Contains(script.Current));
//                if (goForMore) {
//                   continue;
//                }
//             }

//             if (SkipOrAppendIfNecessary(item, ch, to)) {
//                continue;
//             }

//             string token = item.ToString();

//             bool ternary = UpdateIfTernary(script, token, ch, ref listToMerge);
//             if (ternary) {
//                return listToMerge;
//             }

//             CheckConsistency(token, listToMerge, script);

//             script.MoveForwardIf(Constants.SPACE);

//             if (action != null && action.Length > 1) {
//                script.Forward(action.Length - 1);
//             }

//             // We are done getting the next token. The getValue() call below may
//             // recursively call loadAndCalculate(). This will happen if extracted
//             // item is a function or if the next item is starting with a START_ARG '('.
//             var func = new ParserFunction(script, token, ch, ref action);
//             Cell current = func.GetValue(script);
//             if (current == null) {
//                current = Cell.EmptyInstance;
//             }
//             current.ParsingToken = token;

//             if (negated > 0 && current.Type == Cell.VarType.NUMBER) {
//                // If there has been a NOT sign, this is a boolean.
//                // Use XOR (true if exactly one of the arguments is true).
//                bool neg = !((negated % 2 == 0) ^ Convert.ToBoolean(current.Value));
//                current = new Cell(Convert.ToDouble(neg));
//                negated = 0;
//             }

//             if (action == null) {
//                action = UpdateAction(script, to);
//             }
//             else {
//                script.MoveForwardIf(action[0]);
//             }

//             char next = script.TryCurrent(); // we've already moved forward
//             bool done = listToMerge.Count == 0 &&
//                         (next == Constants.END_STATEMENT ||
//                          action == Constants.NULL_ACTION && current.Type != Cell.VarType.NUMBER ||
//                          current.IsReturn);
//             if (done) {
//                if (action != null && action != Constants.END_ARG_STR) {
//                   throw new ArgumentException("Action [" +
//                                               action + "] without an argument.");
//                }
//                // If there is no numerical result, we are not in a math expression.
//                listToMerge.Add(current);
//                return listToMerge;
//             }

//             Cell cell = current.Clone();
//             cell.Action = action;

//             bool addIt = UpdateIfBool(script, ref cell, ref listToMerge);
//             if (addIt) {
//                listToMerge.Add(cell);
//             }
//             item.Clear();
//          } while (script.StillValid() &&
//                   (inQuotes || arrayIndexDepth > 0 || !to.Contains(script.Current)));

//          // This happens when called recursively inside of the math expression:
//          script.MoveForwardIf(Constants.END_ARG);

//          return listToMerge;
//       }*/

//        /// <summary>
//        ///     From outside this function is called with mergeOneOnly = false.
//        ///     It also calls itself recursively with mergeOneOnly = true, meaning
//        ///     that it will return after only one merge.
//        /// </summary>
//        private static OperationToken Merge(Cell current, ref int index, IReadOnlyList<Cell> cells,
//                                            bool mergeOneOnly = false) {
//            OperationToken operation = null;
//            while (index < cells.Count) {
//                var next = cells[index++];
//                while (current.OperatorToken.Properties.Precedence < next.OperatorToken.Properties.Precedence
//                    ) // If we cannot merge cells yet, go to the next cell and merge
//                    // next cells first. E.g. if we have 1+2*3, we first merge next
//                    // cells, i.e. 2*3, getting 6, and then we can merge 1+6.
//                {
//                    Merge(next, ref index, cells, true);
//                }

//                // simplifying
//                if (current.Value is ConstToken numT1 && next.Value is ConstToken numT2) {
//                    var num1 = double.Parse(numT1.Value);
//                    var num2 = double.Parse(numT2.Value);
//                    switch (current.OperatorToken.Value) {
//                        case "*":
//                            current.Value.Value = (num1 * num2).ToString(CultureInfo.InvariantCulture);
//                            break;
//                        case "/":
//                            if (num2.Equals(0d)) {
//                                throw new ArgumentException("Division by zero");
//                            }
//                            current.Value.Value = (num1 / num2).ToString(CultureInfo.InvariantCulture);
//                            break;
//                        case "+":
//                            current.Value.Value = (num1 + num2).ToString(CultureInfo.InvariantCulture);
//                            break;
//                        case "-":
//                            current.Value.Value = (num1 - num2).ToString(CultureInfo.InvariantCulture);
//                            break;
//                    }
//                }
//                current.OperatorToken = next.OperatorToken;
//                operation             = new OperationToken(current.OperatorToken, current.Value, operation);
//                if (mergeOneOnly) {
//                    break;
//                }
//            }
//            return operation;
//        }

//        private class Cell {
//            internal Cell(Token value, OperatorToken opToken) {
//                Value         = value;
//                OperatorToken = opToken;
//            }

//            internal Token         Value         { get; }
//            internal OperatorToken OperatorToken { get; set; }
//        }
//    }
//}

