using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Specification;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic;

[DebuggerDisplay("{TokenIdx}: '{Token.Value}', then '{Peek.Value}'.")]
public class TokenStream : IList<Token> {
    readonly List<Token> tokens = new();
    public int TokenIdx { get; private set; } = -1;

    public Token Token =>
        TokenIdx > -1 && TokenIdx < tokens.Count
            ? tokens[TokenIdx]
            : tokens[^1];

    public Token ExactPeek => TokenIdx + 1 < tokens.Count
        ? tokens[TokenIdx + 1]
        : tokens[^1];

    public Token Peek {
        get {
            SkipTrivial();
            return ExactPeek;
        }
    }

    public int Count => tokens.Count;

    public bool IsReadOnly => false;

    public IEnumerator<Token> GetEnumerator() {
        return tokens.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public void Add(Token item) {
        tokens.Add(item);
    }

    public void Clear() {
        tokens.Clear();
    }

    public bool Contains(Token item) {
        return tokens.Contains(item);
    }

    public void CopyTo(Token[] array, int arrayIndex) {
        tokens.CopyTo(array, arrayIndex);
    }

    public bool Remove(Token item) {
        return tokens.Remove(item);
    }

    public int IndexOf(Token item) {
        return tokens.IndexOf(item);
    }

    public void Insert(int index, Token item) {
        tokens.Insert(index, item);
    }

    public void RemoveAt(int index) {
        tokens.RemoveAt(index);
    }

    public Token this[int index] {
        get => tokens[index];
        set => tokens[index] = value;
    }

    public bool PeekIs(params TokenType[] expected) {
        return PeekByIs(1, expected);
    }

    public bool PeekByIs(int peekBy, params TokenType[] expected) {
        SkipTrivial(expected);
        if (TokenIdx + peekBy < tokens.Count) {
            var peekN = tokens[TokenIdx + peekBy];
            return expected.Any(tt => peekN.Type == tt);
        }

        // if went out of bounds then only end can match
        return expected.Contains(End);
    }

    public Token EatAny(int pos = 1) {
        if (TokenIdx + pos >= 0 && TokenIdx + pos < tokens.Count) {
            TokenIdx += pos;
        }

        return tokens[TokenIdx];
    }

    /// <summary>
    ///     Skips and returns the next token.
    ///     Reports error, if the next token type
    ///     is not the same as passed in parameter.
    /// </summary>
    public Token Eat(params TokenType[] types) {
        SkipTrivial(types);
        EatAny();
        if (!Token.Is(types)) {
            LanguageReport.To(BlameType.InvalidSyntax, Token);
        }

        return Token;
    }

    /// <summary>
    ///     Skips token of specified type,
    ///     returns: was token skipped or not.
    /// </summary>
    public bool MaybeEat(params TokenType[] types) {
        SkipTrivial(types);
        for (var i = 0; i < types.Length; i++) {
            if (ExactPeek.Type == types[i]) {
                EatAny();
                return true;
            }
        }

        return false;
    }

    public bool MaybeEat(string value) {
        SkipTrivial();
        if (ExactPeek.Content != value) {
            return false;
        }

        EatAny();
        return true;
    }

    public void MoveAbsolute(int tokenIndex) {
        TokenIdx = tokenIndex;
    }

    void SkipTrivial(params TokenType[] wantedTypes) {
        var skipNewlines = !wantedTypes.Contains(Newline);
        while (ExactPeek.Type == Comment
            || ExactPeek.Type == Newline && skipNewlines) {
            EatAny();
        }
    }
}
