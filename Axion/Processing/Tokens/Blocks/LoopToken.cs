//using Axion.Enums;

//namespace Axion.Processing.Tokens.Blocks {
//   internal class LoopToken : BlockToken {
//      internal readonly OperationToken Condition;
//      internal readonly OperationToken Iterator;
//      internal readonly Token IteratorFunction;

//      public LoopToken(Token conditionToken, Token iteratorDeclarationToken = null,
//                       Token iteratorFunctionToken = null) {
//         // TODO add line and column in while loop
//         if (!(conditionToken is OperationToken condition)) {
//            throw new ProcessingException($"Invalid condition in 'while' loop: '{conditionToken}'",
//                                          ErrorOrigin.Parser,
//                                          conditionToken?.StartLnPos   ?? -1,
//                                          conditionToken?.StartClPos ?? -1);
//         }

//         Condition = condition;
//         if (!(iteratorDeclarationToken is OperationToken iteratorDeclaration) ||
//             iteratorDeclaration.OperatorToken.Value != "=") {
//            throw new ProcessingException(
//               $"Invalid iterator declaration in 'while' loop: '{iteratorDeclarationToken}'",
//               ErrorOrigin.Parser,
//               iteratorDeclarationToken?.StartLnPos   ?? -1,
//               iteratorDeclarationToken?.StartClPos ?? -1);
//         }

//         Iterator = iteratorDeclaration;
//         if (!(iteratorFunctionToken is OperationToken iteratorFunction)) {
//            throw new ProcessingException($"Invalid iterator function in 'while' loop: '{iteratorFunctionToken}'",
//                                          ErrorOrigin.Parser,
//                                          iteratorFunctionToken?.StartLnPos   ?? -1,
//                                          iteratorFunctionToken?.StartClPos ?? -1);
//         }

//         IteratorFunction = iteratorFunction;
//      }
//   }
//}