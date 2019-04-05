using System.Linq;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical {
    public partial class Lexer {
        private Token ReadMarkOrOperator() {
            int    longestLength = Spec.SortedSymbolicValues[0].Length;
            string nextCodePiece = PeekPiece(longestLength);
            var    value         = "";
            Token  result        = null;
            for (int length = nextCodePiece.Length; length > 0; length--) {
                value = nextCodePiece.Substring(0, length);
                // grow sequence of characters
                if (!Spec.SortedSymbolicValues.Contains(value)) {
                    continue;
                }

                if (Spec.Operators.ContainsKey(value)) {
                    result = new OperatorToken(value, tokenStartPosition);
                    break;
                }

                if (Spec.Symbols.ContainsKey(value)) {
                    result = new MarkToken(value, tokenStartPosition);
                    if (result.Type.IsOpenBracket()) {
                        mismatchingPairs.Add(result);
                    }
                    else if (result.Type.IsCloseBracket()) {
                        if (mismatchingPairs.Count == 0) {
                            mismatchingPairs.Add(result);
                        }
                        else {
                            mismatchingPairs.RemoveAt(mismatchingPairs.Count - 1);
                        }
                    }

                    break;
                }
            }

            Move(value.Length);
            if (result == null) {
                // not found in specification
                unit.Blame(BlameType.InvalidCharacter, tokenStartPosition, Position);
                return new Token(TokenType.Invalid, value, tokenStartPosition);
            }

            return result;
        }
    }
}