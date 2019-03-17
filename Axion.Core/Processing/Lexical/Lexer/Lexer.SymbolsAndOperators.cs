using System.Linq;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using static Axion.Core.Specification.Spec;

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
            int    longestLength = SortedSymbolicValues[0].Length;
            string nextCodePiece = c + stream.PeekPiece(longestLength - 1);

            for (int length = nextCodePiece.Length; length > 0; length--) {
                string piece = nextCodePiece.Substring(0, length);
                // grow sequence of symbols
                if (!SortedSymbolicValues.Contains(piece)) {
                    continue;
                }

                stream.Move(length);
                if (Operators.ContainsKey(piece)) {
                    return new OperatorToken(piece, tokenStartPosition);
                }

                if (Symbols.Forward.ContainsKey(piece)) {
                    var token = new SymbolToken(piece, tokenStartPosition);
                    if (token.IsOpenBrace) {
                        mismatchingPairs.Add(token);
                    }
                    else if (token.IsCloseBrace) {
                        if (mismatchingPairs.Count == 0) {
                            mismatchingPairs.Add(token);
                        }
                        else {
                            mismatchingPairs.RemoveAt(mismatchingPairs.Count - 1);
                        }
                    }

                    return token;
                }
            }

            // operator or symbol not found in specification
            unit.Blame(BlameType.InvalidSymbol, tokenStartPosition, stream.Position);
            return new Token(TokenType.Invalid, tokenValue.ToString(), tokenStartPosition);
        }
    }
}