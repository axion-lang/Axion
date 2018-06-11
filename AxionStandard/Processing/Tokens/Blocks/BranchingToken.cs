//using System.Collections.Generic;
//using Axion.Processing;

//namespace Axion.Tokens
//{
//    internal class BranchingToken : BlockToken
//    {
//        internal readonly Token Condition;
//        internal readonly List<BlockToken> ElseIfs = new List<BlockToken>();
//        internal BlockToken ElseBlock = new BlockToken();

//        internal BranchingToken(Token startToken, Token condition)
//        {
//            LinePosition = startToken.LinePosition;
//            ColumnPosition = startToken.ColumnPosition;
//            Value = startToken.Value;
//            // if some condition doesn't return a boolean
//            if (!(condition is OperationToken) ||
//                condition.ID != TokenID.ID)
//            {
//                throw new ProcessingException("Invalid condition",
//                                              ErrorOrigin.TokenConstructor,
//                                              condition.LinePosition,
//                                              condition.ColumnPosition);
//            }

//            if (Parser.TokenIndex >= Parser.Tokens.Count)
//            {
//                throw new ProcessingException("Condition without actions at end of file",
//                                              ErrorOrigin.TokenConstructor);
//            }

//            // todo check for duplicate conditions: create recursive function through operation token
//            Condition = condition;
//        }

//        public override string ToString(int tabLevel)
//        {
//            var tabs = "";
//            for (int i = 0; i < tabLevel; i++)
//            {
//                tabs += "  ";
//            }

//            string str = $"{tabs}if:\r\n";
//            str += $"{tabs}  condition:\r\n";
//            str += Condition.ToString(tabLevel + 2);
//            str += "\r\n";

//            str += $"{tabs}  then:\r\n";
//            for (int i = 0; i < Children.Count; i++)
//            {
//                str += Children[i].ToString(tabLevel + 2);
//                str += "\r\n";
//            }

//            for (int i = 0; i < ElseIfs.Count; i++)
//            {
//                str += $"{tabs}  else-if-{i + 1}:\r\n";
//                str += ElseIfs[i].ToString(tabLevel + 2);
//                str += "\r\n";
//            }

//            if (ElseBlock.Children.Count == 0)
//            {
//                str += $"{tabs}  else: null";
//            }
//            else
//            {
//                str += $"{tabs}  else:\r\n";
//                str += ElseBlock.ToString(tabLevel + 1);
//            }

//            return str;
//        }
//    }
//}

