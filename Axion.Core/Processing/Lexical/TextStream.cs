using System;
using System.Linq;
using Axion.Specification;

namespace Axion.Core.Processing.Lexical;

/// <summary>
///     Character stream with possibility
///     to peek next char, rest of line,
///     and moving backwards.
/// </summary>
public class TextStream {
    int charIdx = -1;
    int columnIdx;
    int lineIdx;

    string Text { get; }

    /// <summary>
    ///     0-based (Line, Column) position of character in source code.
    /// </summary>
    public Location Location => new(lineIdx, columnIdx);

    /// <summary>
    ///     Checks that next character is line/source terminator.
    /// </summary>
    public bool AtEndOfLine =>
        Spec.Eols.Contains(Peek()) || PeekIs(Spec.EndOfCode);

    public ReadOnlySpan<char> RestOfLine {
        get {
            var textFromCurrent = Text.AsSpan()[(charIdx + 1)..];
            var i = textFromCurrent.IndexOf('\n');
            if (i == -1) {
                return textFromCurrent;
            }

            return textFromCurrent[..(i + 1)];
        }
    }

    /// <summary>
    ///     Current (eaten) character.
    /// </summary>
    public char Char => charIdx < 0 ? Spec.EndOfCode : Text[charIdx];

    public bool IsEmpty => string.IsNullOrWhiteSpace(Text);

    public TextStream(string text) {
        if (!text.EndsWith(Spec.EndOfCode)) {
            text += Spec.EndOfCode;
        }

        Text = text;
    }

    /// <summary>
    ///     Returns next character from stream,
    ///     not eating it.
    /// </summary>
    public char Peek() {
        if (charIdx + 2 >= 0 && charIdx + 2 < Text.Length) {
            return Text[charIdx + 1];
        }

        return Spec.EndOfCode;
    }

    /// <summary>
    ///     Returns next N characters from stream,
    ///     not eating them.
    /// </summary>
    public ReadOnlySpan<char> Peek(int length) {
        var nextIdx = charIdx + 1;
        if (nextIdx + length < Text.Length) {
            return Text.AsSpan().Slice(nextIdx, length);
        }
        Span<char> span = new char[length];
        Text.AsSpan()[
            Math.Min(nextIdx, Text.Length - 1)
                ..
                Math.Min(nextIdx + length + 1, Text.Length)
        ].CopyTo(span);
        return span;
    }

    /// <summary>
    ///     Compares next substring from stream
    ///     with expected strings.
    /// </summary>
    public bool PeekIs(params string[] expected) {
        var peek = Peek(expected[0].Length);
        foreach (var s in expected) {
            if (peek.Length < s.Length) {
                peek = Peek(s.Length);
            }
            if (peek.StartsWith(s)) {
                return true;
            }
        }
        return false;
    }

    public bool PeekIs(params char[] expected) {
        return expected.Contains(Peek());
    }

    /// <summary>
    ///     Consumes next substring from stream,
    ///     checking that it's equal to expected.
    /// </summary>
    public ReadOnlySpan<char> Eat(params string[] expected) {
        if (expected.Length == 0) {
            Move();
            return new[] { Char };
        }

        var peek = Peek(expected[0].Length);
        foreach (var s in expected) {
            if (peek.Length < s.Length) {
                peek = Peek(s.Length);
            }
            if (peek.StartsWith(s)) {
                Move(s.Length);
                return s;
            }
        }

        return default;
    }

    /// <summary>
    ///     Consumes next char from stream,
    ///     checking that it's equal to expected.
    /// </summary>
    public string? Eat(params char[] expected) {
        if (expected.Length == 0) {
            Move();
            return Char.ToString();
        }

        var nxt = Peek();
        if (expected.Contains(nxt)) {
            Move();
            return nxt.ToString();
        }

        return null;
    }

    void Move(int by = 1) {
        while (by > 0 && Peek() != Spec.EndOfCode) {
            if (Peek() == '\n') {
                lineIdx++;
                columnIdx = 0;
            }
            else {
                columnIdx++;
            }

            charIdx++;
            by--;
        }
    }
}
