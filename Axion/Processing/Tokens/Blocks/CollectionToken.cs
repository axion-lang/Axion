//using System;
//using System.Collections.Generic;
//using AxionStandard.Enums;

//namespace AxionStandard.Processing.Tokens.Blocks {
//   internal class CollectionToken : Token {
//      internal readonly CollectionType CollectionType;
//      internal readonly List<Token> ItemsTokens;
//      internal readonly Token ItemType;

//      internal CollectionToken(Token itemType, CollectionType collectionType, List<Token> itemsTokens) {
//         ItemType = itemType ?? throw new ArgumentNullException(nameof(itemType));
//         StartLnPos = ItemType.StartLnPos;
//         StartClPos = ItemType.StartClPos;
//         ID = TokenID.Identifier;
//         CollectionType = collectionType;
//         ItemsTokens = itemsTokens;
//      }
//   }

//   internal enum CollectionType {
//      Array,
//      Matrix,
//      List,
//      Map
//   }
//}