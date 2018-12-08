using System.Collections.Generic;
using System.Diagnostics;

namespace Axion.Core.Processing.Lexical {
    public class CharStream {
        /// <summary>
        ///     Stream source.
        /// </summary>
        internal readonly string Source;

        /// <summary>
        ///     Index of current processing character in source.
        /// </summary>
        internal int CharIdx { get; private set; }

        /// <summary>
        ///     Current processing character in source.
        /// </summary>
        internal char C => Source[CharIdx];

        /// <summary>
        ///     Index of current processing line.
        /// </summary>
        private int lineIdx;

        /// <summary>
        ///     Index of column in current processing line.
        /// </summary>
        private int columnIdx;

        /// <summary>
        ///     Line and column position of current processing character in source.
        /// </summary>
        internal (int line, int column) Position => (lineIdx, columnIdx);

        /// <summary>
        ///     Length of previous processed line.
        ///     Used to return <see cref="columnIdx" />
        ///     to previous value when moving backward.
        /// </summary>
        private int prevLineLength;

        /// <summary>
        ///     Initializes a new stream with specified lines of source code.
        /// </summary>
        /// <param name="sourceLines"></param>
        public CharStream(IEnumerable<string> sourceLines)
            : this(string.Join("\n", sourceLines)) {
        }

        /// <summary>
        ///     Initializes a new stream with specified source.
        /// </summary>
        /// <param name="source"></param>
        public CharStream(string source) {
            // add null-terminator to mark end of stream.
            if (!source.EndsWith(Spec.EndOfStream.ToString())) {
                source += Spec.EndOfStream;
            }
            Source = source;
        }

        /// <summary>
        ///     Copies <see cref="stream" />
        ///     to current stream.
        /// </summary>
        /// <param name="stream"></param>
        internal CharStream(CharStream stream) {
            Source    = stream.Source;
            CharIdx   = stream.CharIdx;
            lineIdx   = stream.Position.line;
            columnIdx = stream.Position.column;
        }

        /// <summary>
        ///     Checks that current character
        ///     is a newline character.
        /// </summary>
        internal bool AtEndOfLine() {
            return Source[CharIdx] == '\r'
                || Source[CharIdx] == '\n';
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
            return Source.Substring(CharIdx, indexOfNewline);
        }

        /// <summary>
        ///     Returns character next to current character,
        ///     without moving to it.
        /// </summary>
        internal char Peek => CharIdx + 1 < Source.Length ? Source[CharIdx + 1] : Spec.EndOfStream;

        /// <summary>
        ///     Returns string of specified <see cref="length" />
        ///     starting from character next to current.
        /// </summary>
        /// <param name="length"></param>
        internal string PeekPiece(int length) {
            if (CharIdx + 1 + length < Source.Length) {
                return Source.Substring(CharIdx + 1, length);
            }
            return Source.Substring(CharIdx + 1, Source.Length - CharIdx - 1);
        }

        /// <summary>
        ///     Gets next (or previous) character from stream,
        ///     depending on <see cref="byLength" /> value.
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