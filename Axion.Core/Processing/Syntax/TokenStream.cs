//using System.Collections.Generic;
//using Axion.Core.Processing.Lexical.Tokens;
//
//namespace Axion.Core.Processing.Syntax {
//    public class TokenStream {
//        /// <summary>
//        ///     Reference to processing <see cref="LinkedList{T}" /> of tokens.
//        /// </summary>
//        internal readonly List<Token> Tokens;
//
//        private int index = 0;
//
//        internal Token Token => index < Tokens.Count ? Tokens[index] : null;
//
//        internal Token Peek => index + 1 < Tokens.Count ? Tokens[index + 1] : null;
//        
//        public TokenStream(List<Token> tokens) {
//            this.Tokens = tokens;
//        }
//
//        internal void Move(int pos = 1) {
//            index += pos;
//        }
//
//        public bool PeekIs(Token expected) {
//            return Equals(Peek, expected);
//        }
//
//        public bool MaybeEat(TokenType type) {
//            if (Peek.Type == type) {
//                Move();
//                return true;
//            }
//            return false;
//        }
//
//        public bool Eat(TokenType type) {
//            if (Peek.Type == type) {
//                Move();
//                return true;
//            }
//            ReportSyntaxError(Peek);
//            return false;
//        }
//
//        public bool EatNonEof(TokenType type) {
//            if (Peek.Type == type) {
//                Move();
//                return true;
//            }
//            ReportSyntaxError(Peek, ErrorCodes.SyntaxError, false);
//            return false;
//        }
//    }
//}

