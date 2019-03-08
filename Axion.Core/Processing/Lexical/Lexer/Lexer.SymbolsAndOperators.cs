using System.Linq;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical.Lexer {
    public partial class Lexer {
        /// <summary>
        ///     Gets a language operator or symbol from next piece of source.
        ///     Returns operator token if next piece of source
        ///     contains operator, declared in specification.
        ///     Symbol token, if next piece of source
        ///     contains symbol, declared in specification.
        ///     Otherwise, returns invalid operator token.
        /// </summary>
        private Token ReadSymbolic() {
            int    longestLength = Spec.SymbolicValues[0].Length;
            string nextCodePiece = c + stream.PeekPiece(longestLength - 1);

            for (int length = nextCodePiece.Length; length > 0; length--) {
                string piece = nextCodePiece.Substring(0, length);
                // grow sequence of symbols
                if (!Spec.SymbolicValues.Contains(piece)) {
                    continue;
                }
                stream.Move(length);
                if (Spec.Operators.ContainsKey(piece)) {
                    return new OperatorToken(tokenStartPosition, piece);
                }
                if (Spec.Symbols.ContainsKey(piece)) {
                    var token = new SymbolToken(tokenStartPosition, piece);
                    if (token.IsOpenBrace) {
                        mismatchingPairs.Add(token);
                    }
                    else if (token.IsCloseBrace) {
                        if (mismatchingPairs.Count == 0) {
                            // got closing without opening
                            mismatchingPairs.Add(token);
                        }
                        else {
                            // got closing & opening
                            mismatchingPairs.RemoveAt(mismatchingPairs.Count - 1);
                        }
                    }
                    return token;
                }
            }
            // operator or symbol not found in specification
            unit.Blame(BlameType.InvalidSymbol, tokenStartPosition, stream.Position);
            return new Token(TokenType.Invalid, tokenStartPosition, tokenValue.ToString());
        }
    }
}