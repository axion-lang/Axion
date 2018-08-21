using System;
using System.Collections.Generic;
using System.Timers;
using Axion.Core.Processing;
using Axion.Core.Visual.ConsoleImpl;

namespace Axion.Core.Visual {
    internal static class ConsoleCodeEditor {
        private static readonly ConsoleSyntaxHighlighter highlighter = new ConsoleSyntaxHighlighter();

        private static readonly List<string> lines = new List<string>();

        private static string line {
            get => lines[cursorY];
            set {
                lines[cursorY] = value;
                // if line modified - it should be re-rendered
                HighlightLine();
            }
        }

        #region Cursor position

        private static int cursorX {
            get => Console.CursorLeft - leftBoundSize;
            set => Console.CursorLeft = value + leftBoundSize;
        }

        private static int cursorY {
            get => Console.CursorTop - startCursorY;
            set => Console.CursorTop = value + startCursorY;
        }

        #endregion

        #region Error box properties

        private static          int    errorBoxX;
        private static          int    errorBoxY;
        private static          int    errorBoxWidth;
        private static readonly string errorBoxPrefix = "| Error: ";

        #endregion

        private static          int startCursorY;
        private static readonly int leftBoundSize = 8;

        private static readonly string tabulation = new string(' ', 4);

        public static IEnumerable<string> BeginSession(string firstCodeLine) {
            lines.Add(firstCodeLine);
            ConsoleUI.ClearLine();

            // print editor header
            ConsoleUI.Write(
                "Press [Esc] twice to exit code editor\n" +
                "┌──────" + new string('─', Console.BufferWidth - leftBoundSize) + "\n" +
                errorBoxPrefix
            );
            // save size & position of error box
            errorBoxX     = Console.CursorLeft;
            errorBoxY     = Console.CursorTop;
            errorBoxWidth = Console.BufferWidth - cursorX;
            // no error occurred
            ConsoleUI.WriteLine(
                "none\n" +
                "├─────┬" + new string('─', Console.BufferWidth - leftBoundSize)
            );

            // init editor field bounds
            startCursorY = Console.CursorTop;
            cursorX      = line.Length;

            // highlight first line
            HighlightLine();

            // writing loop
            ConsoleKeyInfo key = Console.ReadKey(true);
            while (true) {
                switch (key.Key) {
                    case ConsoleKey.Escape: {
                        key = Console.ReadKey(true);
                        if (key.Key == ConsoleKey.Escape) {
                            // [Esc] pressed twice -> exit
                            goto EDITOR_EXIT;
                        }
                        continue;
                    }

                    #region Move cursor with arrows

                    case ConsoleKey.LeftArrow: {
                        MoveCursor(MoveDirection.Left);
                        break;
                    }
                    case ConsoleKey.RightArrow: {
                        MoveCursor(MoveDirection.Right);
                        break;
                    }
                    case ConsoleKey.UpArrow: {
                        MoveCursor(MoveDirection.Up);
                        break;
                    }
                    case ConsoleKey.DownArrow: {
                        MoveCursor(MoveDirection.Down);
                        break;
                    }

                    #endregion

                    case ConsoleKey.Enter: {
                        SplitLine();
                        break;
                    }
                    case ConsoleKey.Backspace: {
                        EraseLeftChar();
                        break;
                    }
                    case ConsoleKey.Delete: {
                        EraseRightChar();
                        break;
                    }
                    case ConsoleKey.Spacebar: {
                        WriteValue(" ");
                        break;
                    }
                    // handle tabulation
                    case ConsoleKey.Tab: {
                        // Shift + Tab: outdent current tab
                        if (key.Modifiers == ConsoleModifiers.Shift) {
                        }
                        // if on the left side of cursor only whitespaces
                        else if (line.Substring(0, cursorX).Trim().Length == 0) {
                            line    = tabulation + line;
                            cursorX = tabulation.Length;
                        }
                        else {
                            WriteValue(" ");
                        }
                        break;
                    }
                    // do not handle insert key
                    case ConsoleKey.Insert: {
                        break;
                    }
                    // any char that should be passed in code
                    default: {
                        WriteValue(key.KeyChar);
                        break;
                    }
                }
                key = Console.ReadKey(true);
            }

            EDITOR_EXIT:

            ConsoleUI.WriteLine("\n└─────┴" + new string('─', Console.BufferWidth - leftBoundSize));

            // remove empty lines
            lines.RemoveAll(ln => ln.Trim().Length == 0);

            // finished work
            return lines;
        }

        /// <summary>
        ///     Splits line in cursor position into 2 lines.
        /// </summary>
        private static void SplitLine() {
            try {
                string tailPiece = line.Substring(cursorX);
                // cut current line
                line = line.Substring(0, cursorX);
                // move to next line
                cursorY++;
                // if on last line
                if (cursorY == lines.Count) {
                    lines.Add(tailPiece);
                    HighlightLine();
                }
                else {
                    lines.Insert(cursorY, tailPiece);
                    // save cursor position
                    int savedY = cursorY;
                    // re-render rest lines
                    for (; cursorY < lines.Count; cursorY++) {
                        HighlightLine();
                    }
                    // restore cursor position
                    cursorY = savedY;
                }
                cursorX = 0;
            }
            catch (FileTooLargeException ex) {
                int savedX = cursorX;
                int savedY = cursorY;
                ex.Render();
                cursorX = savedX;
                cursorY = savedY;
            }
        }

        /// <summary>
        ///     Erases character on the left side of cursor.
        /// </summary>
        private static void EraseLeftChar() {
            // if on first line start
            if (cursorY == 0 &&
                cursorX <= 0) {
                return;
            }
            // if cursor in current line
            if (cursorX > 0) {
                int savedCursorX = cursorX;
                line    = line.Remove(cursorX - 1, 1);
                cursorX = savedCursorX - 1;
            }
            // remove current line
            else {
                ConsoleUI.ClearLine();
                var prevLine = "";
                if (line.Length > 0) {
                    prevLine = line;
                }
                lines.RemoveAt(cursorY);
                // move cursor to previous line
                cursorY--;
                // append previous line if it is not empty
                line += prevLine;

                cursorX = line.Length - prevLine.Length;

                // save cursor position
                int savedY = cursorY;
                int savedX = cursorX;

                // re-render rest lines
                cursorY++;
                for (; cursorY < lines.Count; cursorY++) {
                    HighlightLine();
                }
                // we're on last line (it is duplicated)
                ConsoleUI.ClearLine();
                // restore cursor position
                cursorY = savedY;
                cursorX = savedX;
            }
        }

        /// <summary>
        ///     Erases character on the right side of cursor.
        /// </summary>
        private static void EraseRightChar() {
            // if on last line end
            if (cursorY == lines.Count - 1 &&
                cursorX >= line.Length) {
                return;
            }
            // if cursor in current line
            if (cursorX < line.Length) {
                int savedX = cursorX;
                line    = line.Remove(cursorX, 1);
                cursorX = savedX;
            }
            // remove current line
            else {
                // save cursor position
                int savedY = cursorY;
                int savedX = cursorX;
                // append next line to current
                line += lines[cursorY + 1];
                // clear next line
                cursorY++;
                lines.RemoveAt(cursorY);
                // re-render rest lines
                for (; cursorY < lines.Count; cursorY++) {
                    HighlightLine();
                }
                // we're on last line (it is duplicated)
                ConsoleUI.ClearLine();
                // restore cursor position
                cursorY = savedY;
                cursorX = savedX;
            }
        }

        /// <summary>
        ///     Writes specified value to editor
        ///     and optionally re-highlights current line.
        /// </summary>
        /// <param name="value"></param>
        private static void WriteValue(object value) {
            string str = value.ToString();
            // write value
            if (cursorX >= line.Length) {
                line    += str;
                cursorX =  line.Length;
            }
            else {
                line    =  line.Insert(cursorX, str);
                cursorX += str.Length;
            }
        }

        private enum MoveDirection {
            Left,
            Right,
            Up,
            Down
        }

        private static void MoveCursor(MoveDirection direction, int count = 1) {
            switch (direction) {
                case MoveDirection.Left: {
                    // if reach first line start
                    if (cursorY == 0 &&
                        cursorX - count < 0) {
                        return;
                    }
                    // if fits in current line
                    if (cursorX - count >= 0) {
                        cursorX -= count;
                    }
                    // move line up
                    else {
                        cursorY--;
                        cursorX = line.Length; // no - 1
                    }
                    break;
                }
                case MoveDirection.Right: {
                    // if reach last line end
                    if (cursorY == lines.Count - 1 &&
                        cursorX + count > line.Length) {
                        return;
                    }
                    // if fits in current line
                    if (cursorX + count <= line.Length) {
                        cursorX += count;
                    }
                    // move line down
                    else {
                        cursorY++;
                        cursorX = 0;
                    }
                    break;
                }
                case MoveDirection.Up: {
                    // if on first line
                    if (cursorY == 0) {
                        return;
                    }
                    cursorY--;
                    // if cursor moves at empty space upside
                    if (cursorX >= line.Length) {
                        cursorX = line.Length;
                    }
                    break;
                }
                case MoveDirection.Down: {
                    // if on first line
                    if (cursorY == lines.Count - 1) {
                        return;
                    }
                    cursorY++;
                    // if cursor moves at empty space downside
                    if (cursorX >= line.Length) {
                        cursorX = line.Length;
                    }
                    break;
                }
            }
        }

        /// <summary>
        ///     Full rewriting and highlighting of current code line.
        ///     You must reset <see cref="cursorX" /> after invoking that
        ///     function and after changing <see cref="line" /> value.
        /// </summary>
        private static void HighlightLine() {
            ConsoleUI.ClearLine();
            PrintLineNumber();
            if (line.Trim().Length == 0) {
                // if line empty just print whitespace
                ConsoleUI.Write(line);
            }
            else {
                // invoke highlighter
                highlighter.Highlight(line, out List<ProcessingException> errors);
                if (errors.Count != 0) {
                    PrintException(errors[0]);
                }
            }
        }

        /// <summary>
        ///     Prints line number on left side of editor.
        ///     That number not included in code.
        /// </summary>
        private static void PrintLineNumber() {
            var view = "|";

            // left align line number
            // |   X |
            if (cursorY < 9) {
                view += "   ";
            }
            // |  XX |
            else if (cursorY < 99) {
                view += "  ";
            }
            // | XXX |
            else if (cursorY < 300) {
                view += " ";
            }
            // file too large to display.
            else {
                throw new FileTooLargeException();
            }
            // append line number and right aligner
            view += cursorY + 1 + " | ";
            ConsoleUI.Write(view);
        }

        private static void PrintException(ProcessingException errorMessage) {
            // save cursor position
            int savedX = Console.CursorLeft;
            int savedY = Console.CursorTop;

            // write message to error box
            Console.CursorLeft = errorBoxX;
            Console.CursorTop  = errorBoxY;

            ConsoleUI.ClearLine();
            ConsoleUI.Write(errorBoxPrefix + errorMessage.Message);

            // restore cursor position
            cursorX            = savedX;
            Console.CursorLeft = savedX;
            Console.CursorTop  = savedY;
        }

        /// <summary>
        ///     Occurs when lines in editor exceeds maximal allowed value.
        /// </summary>
        private class FileTooLargeException : Exception {
            public FileTooLargeException()
                : base("| <: File too large to display. Please use external editor. :>") {
            }

            public void Render() {
                ConsoleUI.Write(Message);
                var showTimer = new Timer(5000);
                showTimer.Elapsed += delegate {
                    ConsoleUI.ClearLine();
                    showTimer.Stop();
                    showTimer.Dispose();
                };
                showTimer.Start();
            }
        }
    }
}