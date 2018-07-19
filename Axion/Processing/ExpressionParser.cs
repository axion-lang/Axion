//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using Axion.Processing.Tokens;
//using Axion.Processing.Tokens.Blocks;

//namespace Axion.Processing {
//   public class EParser {
//      private static TokenCollection inputTokens;

//      public static OperationToken Process(TokenCollection input) {
//         inputTokens = input;

//         // check parenthesis count
//         Token[] inputOperators = input.Where(t => t is OperatorToken).ToArray();
//         var missingParenthesesCount = 0;
//         for (int i = 0; i < inputOperators.Length; i++) {
//            switch (inputOperators[i].Value) {
//               case "(":
//                  missingParenthesesCount++;
//                  break;
//               case ")":
//                  missingParenthesesCount--;
//                  break;
//            }
//         }

//         if (missingParenthesesCount != 0) {
//            throw new ArgumentException("Uneven parenthesis");
//         }

//         var from = 0;
//         return LoadAndCalculate(ref from, input.Last.Value.Value);
//      }

//      public static Token recf(ref int from, string toValue) {
//         while (from < inputTokens.Count && inputTokens[from].Value != toValue) {
//            Token ct = inputTokens[from++];
//            switch (ct) {
//               case NumberToken num: {
//                  if (from < inputTokens.Count || !(inputTokens[from] is OperatorToken op)) {
//                     throw new Exception();
//                  }

//                  return new OperationToken(op, num, recf(ref from, toValue));
//               }
//               case OperatorToken op: {
//                  // prefix unary if previous token is null or operator and operator can be unary
//                  bool prefixUnaryOp = (from == 1 || inputTokens[from - 2] is OperatorToken) &&
//                                       op.Operator.InputSide == InputSide.SomeOne;
//                  // postfix unary if next token is null or operator
//                  // prefix unary if previous token is null or operator and operator can be unary
//                  bool postfixUnaryOp = (from >= inputTokens.Count || inputTokens[from] is OperatorToken) &&
//                                        op.Operator.InputSide == InputSide.SomeOne;
//                  if (!prefixUnaryOp  && from == 1 ||
//                      !postfixUnaryOp && from >= inputTokens.Count) {
//                     throw new Exception();
//                  }

//                  OperatorToken[] nextOperators = inputTokens.Skip(from - 1).OfType<OperatorToken>().ToArray();
//                  if (nextOperators.Length > 0) {
//                     if (op.Operator.IsOpenBrace) {
//                        return new OperationToken(op, num, recf(ref from, op.Operator.GetMatchingBrace()));
//                     }
//                  }

//                  break;
//               }
//            }
//         }
//      }

//      public static OperationToken LoadAndCalculate(ref int i, string toValue) {
//         var cells = new List<Cell>();
//         do {
//            NumberToken value = null;
//            OperatorToken action = null;
//            Token token = inputTokens[i++];
//            switch (token) {
//               case OperatorToken opToken: {
//                  // if extracted item is a function or if the next item is starting with a '('
//                  if (opToken.Value == "(") {
//                     value = (NumberToken) LoadAndCalculate(ref i, ")").LeftOperand;
//                  }

//                  break;
//               }
//               case NumberToken numToken: {
//                  value = numToken;

//                  action = (OperatorToken) inputTokens[i++];
//                  break;
//               }
//               default: {
//                  throw new ArgumentException("Could not parse token: " + token);
//               }
//            }

//            cells.Add(new Cell(value, action));
//         } while (i < inputTokens.Count && inputTokens[i].Value != toValue);

//         if (i < inputTokens.Count && inputTokens[i].Value == toValue) {
//            // This happens when called recursively: move one char forward.
//            i++;
//         }

//         var index = 1;
//         return Merge(cells[0], ref index, cells); /*
//            do { // Main processing cycle of the first part.
//                char ch = data[from++];
//                if (StillCollecting(item.ToString(), ch, to)) { // The char still belongs to the previous operand.
//                    item.Append(ch);
//                    if (from < data.Length && data[from] != to) {
//                        continue;
//                    }
//                }


//                double value;
//                if (item.Length == 0 && ch == '(') {
//                    // expression in parentheses
//                    value = LoadAndCalculate(')');
//                }

//                // try to parse this as a number.
//                else if (!double.TryParse(item.ToString(), out value)) {
//                    throw new ArgumentException("Could not parse token [" + item + "]");
//                }

//                char action = ValidAction(ch)
//                    ? ch
//                    : UpdateAction(data, ref from, ch, to);

//                cells.Add(new Cell(value, action));
//                item.Clear();
//            } while (from < data.Length && data[from] != to);

//            if (from < data.Length &&
//                (data[from] == '\n' || data[from] == to)) { // This happens when called recursively: move one char forward.
//                from++;
//            }*/
//      }
//      /*
//               private static bool StillCollecting(string item, char ch, char to) {
//                   // Stop collecting if either got ')' or to char, e.g. ','.
//                   char stopCollecting = to == ')' || to == '\n' ? ')' : to;
//                   return item.Length == 0 && (ch == '-' || ch == ')') ||
//                          !(ValidAction(ch) || ch == '(' || ch == stopCollecting);
//               }
 
//               private static OperatorToken UpdateAction(string item, ref int from, char ch, char to) {
//                   if (from >= item.Length || item[from] == ')' || item[from] == to) {
//                       return ')';
//                   }
 
//                   int index = from;
//                   char res = ch;
//                   while (!ValidAction(res) && index < item.Length) {
//                       // Look for the next character in string until a valid action is found.
//                       res = item[index++];
//                   }
 
//                   if (ValidAction(res)) {
//                       @from = index;
//                   }
//                   else {
//                       if (index > @from) {
//                           @from = index - 1;
//                       }
//                       else {
//                           @from = @from;
//                       }
//                   }
 
//                   return res;
//               }*/

//      /// <summary>
//      ///    From outside this function is called with mergeOneOnly = false.
//      ///    It also calls itself recursively with mergeOneOnly = true, meaning
//      ///    that it will return after only one merge.
//      /// </summary>
//      private static OperationToken Merge(Cell current, ref int index, List<Cell> listToMerge,
//                                          bool mergeOneOnly = false) {
//         OperationToken operation = null;
//         while (index < listToMerge.Count) {
//            Cell next = listToMerge[index++];

//            while (current.OperatorToken.Operator.Precedence < next.OperatorToken.Operator.Precedence) {
//               // If we cannot merge cells yet, go to the next cell and merge
//               // next cells first. E.g. if we have 1+2*3, we first merge next
//               // cells, i.e. 2*3, getting 6, and then we can merge 1+6.
//               Merge(next, ref index, listToMerge, true);
//            }

//            // simplifying
//            if (current.Value is NumberToken numT1 && next.Value is NumberToken numT2) {
//               double num1 = double.Parse(numT1.Value);
//               double num2 = double.Parse(numT2.Value);
//               switch (current.OperatorToken.Value) {
//                  case "*":
//                     current.Value.Value = (num1 * num2).ToString(CultureInfo.InvariantCulture);
//                     break;
//                  case "/":
//                     if (num2.Equals(0d)) {
//                        throw new ArgumentException("Division by zero");
//                     }

//                     current.Value.Value = (num1 / num2).ToString(CultureInfo.InvariantCulture);
//                     break;
//                  case "+":
//                     current.Value.Value = (num1 + num2).ToString(CultureInfo.InvariantCulture);
//                     break;
//                  case "-":
//                     current.Value.Value = (num1 - num2).ToString(CultureInfo.InvariantCulture);
//                     break;
//               }
//            }

//            current.OperatorToken = next.OperatorToken;
//            operation = new OperationToken(current.OperatorToken, current.Value, operation);
//            if (mergeOneOnly) {
//               break;
//            }
//         }

//         return operation;
//      }

//      private class Cell {
//         internal Cell(Token value, OperatorToken opToken) {
//            Value = value;
//            OperatorToken = opToken;
//         }

//         internal Token Value { get; }
//         internal OperatorToken OperatorToken { get; set; }
//      }
//   }
//}