using System;
using Axion.Core.Processing.Lexical.Tokens;
using CodeConsole;

namespace Axion;

public class ColoredToken : ColoredValue {
    public Token Token { get; }

    public ColoredToken(Token token, ConsoleColor color, bool isWhite = false) :
        base(token.Value + token.EndingWhite, color, isWhite) {
        Token = token;
    }
}
