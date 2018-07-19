//using System;
//using Axion.Enums;

//namespace Axion.Processing.Tokens.Blocks {
//   internal class IndexerToken : Token {
//      internal readonly Token Index;
//      internal readonly Token Subject;

//      internal IndexerToken(Token subject, Token index) {
//         Subject = subject ?? throw new ArgumentNullException(nameof(subject));
//         Index = index     ?? throw new ArgumentNullException(nameof(index));
//         StartLnPos = subject.StartLnPos;
//         StartClPos = subject.StartClPos;
//         ID = TokenID.Identifier;
//      }
//   }
//}