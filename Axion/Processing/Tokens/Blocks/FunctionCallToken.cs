//using System;
//using System.Collections.Generic;
//using AxionStandard.Enums;

//namespace AxionStandard.Processing.Tokens.Blocks {
//   internal class FunctionCallToken : Token {
//      internal readonly List<Token> Arguments;
//      internal readonly Token NameToken;

//      internal FunctionCallToken(Token nameToken, List<Token> argumentTokens) {
//         NameToken = nameToken      ?? throw new ArgumentNullException(nameof(nameToken));
//         Arguments = argumentTokens ?? throw new ArgumentNullException(nameof(argumentTokens));
//         StartLnPos = nameToken.StartLnPos;
//         StartClPos = nameToken.StartClPos;
//         ID = TokenID.Identifier;
//      }
//   }
//}