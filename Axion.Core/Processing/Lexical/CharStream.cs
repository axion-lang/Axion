using System.Collections.Generic;
using System.Diagnostics;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical {
    /// <summary>
    ///     A stream of characters, created from strings.
    ///     Used to read symbols, with ability to peek,
    ///     go backward, return rest of current line of source,
    ///     and other useful functions.
    /// </summary>
    public class CharStream {
        /// <summary>
        ///     Stream source.
        /// </summary>
        internal readonly string Source;

        /// <summary>
        ///     Index of current processing line.
        /// </summary>
        private int lineIdx;

        /// <summary>
        ///     Index of column in current processing line.
        /// </summary>
        private int columnIdx;

        /// <summary>
        ///     Length of previous processed line.
        ///     Used to return [<see cref="columnIdx" />]
        ///     to previous value when moving backward.
        /// </summary>
        private int prevLineLength;

        /// <summary>
        ///     Initializes a new stream with specified lines of source code.
        /// </summary>
        public CharStream(IEnumerable<string> sourceLines) : this(string.Join("\n", sourceLines)) {
        }

        /// <summary>
        ///     Initializes a new stream with specified source.
        /// </summary>
        public CharStream(string source) {
            // add null-terminator to mark end of stream.
            if (!source.EndsWith(Spec.EndOfStream.ToString())) {
                source += Spec.EndOfStream;
            }
            Source = source;
        }

        /// <summary>
        ///     Copies whole [<paramref name="stream" />]
        ///     to this stream instance.
        /// </summary>
        internal CharStream(CharStream stream) {
            Source    = stream.Source;
            CharIdx   = stream.CharIdx;
            lineIdx   = stream.Position.Line;
            columnIdx = stream.Position.Column;
        }

        /// <summary>
        ///     Index of current processing character in source.
        /// </summary>
        internal int CharIdx { get; private set; }

        /// <summary>
        ///     Current processing character in source.
        /// </summary>
        internal char C => Source[CharIdx];

        /// <summary>
        ///     Line and column position of current processing character in source.
        /// </summary>
        internal Position Position => (lineIdx, columnIdx);

        /// <summary>
        ///     Returns character next to current character,
        ///     without moving to it.
        /// </summary>
        internal char Peek => CharIdx + 1 < Source.Length ? Source[CharIdx + 1] : Spec.EndOfStream;

        /// <summary>
        ///     Checks that current character
        ///     is a newline character.
        /// </summary>
        internal bool AtEndOfLine() {
            return Source[CharIdx] == '\r' || Source[CharIdx] == '\n';
        }

        /// <summary>
        ///     Returns rest of current line,
        ///     starting from current char
        ///     to nearest newline character.
        /// </summary>
        internal string GetRestOfLine() {
            string fromCurrentIndex = Source.Substring(CharIdx);
            int    indexOfNewline   = fromCurrentIndex.IndexOf('\n');
            if (indexOfNewline == -1) {
                return fromCurrentIndex;
            }
            return fromCurrentIndex.Substring(0, indexOfNewline + 1);
        }

        /// <summary>
        ///     Returns string of specified [<see cref="length" />]
        ///     starting from character next to current.
        /// </summary>
        internal string PeekPiece(int length) {
            if (CharIdx + 1 + length < Source.Length) {
                return Source.Substring(CharIdx + 1, length);
            }
            return Source.Substring(CharIdx + 1, Source.Length - CharIdx - 1);
        }

        /// <summary>
        ///     Gets next (or previous) character from stream,
        ///     depending on [<see cref="byLength" />] value.
        /// </summary>
        internal void Move(int byLength = 1) {
            Debug.Assert(byLength != 0);
            if (byLength > 0) {
                for (var _ = 0; _ < byLength && Source[CharIdx] != Spec.EndOfStream; _++) {
                    if (Source[CharIdx] == '\n') {
                        lineIdx++;
                        prevLineLength = columnIdx;
                        columnIdx      = 0;
                    }
                    else {
                        columnIdx++;
                    }
                    CharIdx++;
                }
            }
            else {
                for (int _ = byLength; _ < 0; _++) {
                    CharIdx--;
                    if (Source[CharIdx] == '\n') {
                        lineIdx--;
                        columnIdx = prevLineLength;
                    }
                    else {
                        columnIdx--;
                    }
                }
            }
        }
    }
}