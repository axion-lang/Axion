//using System;
//using System.Linq;
//using Axion.Enums;

//namespace Axion.Processing.Tokens.Blocks {
//   public class OperationToken : Token {
//      internal readonly Token LeftOperand;
//      internal readonly OperatorToken OperatorToken;
//      internal readonly Token RightOperand;

//      internal OperationToken(OperatorToken @operator, Token leftOperand, Token rightOperand) {
//         OperatorToken = @operator ?? throw new ArgumentNullException(nameof(@operator));
//         Value = OperatorToken.Value;
//         StartLnPos = leftOperand?.StartLnPos
//                        ?? @operator.StartLnPos;
//         StartClPos = leftOperand?.StartClPos
//                          ?? @operator.StartClPos;
//         if (Specification.BoolOperators.Contains(OperatorToken.Value) || OperatorToken.Value == ".") {
//            ID = TokenID.Identifier;
//         }

//         LeftOperand = leftOperand;
//         RightOperand = rightOperand;
//      }

//      public override string ToCppCode(int tabLevel) {
//         return LeftOperand.ToCppCode(tabLevel) + $" {OperatorToken} " + RightOperand.ToCppCode(0);
//      }
//   }
//}