using System;
using System.Collections.Generic;

namespace Axion.Visual {
    internal class ConsoleCodeEditor {
        private static readonly ConsoleOutput output = ConsoleView.Output;
        private readonly        List<string>  lines  = new List<string>();

        private string line {
            get => lines[cursorY];
            set {
                lines[cursorY] = value;
                // if line modified - it should be re-rendered
                RenderLine();
            }
        }

        private int cursorX {
            get => Console.CursorLeft - leftBoundSize;
            set => Console.CursorLeft = value + leftBoundSize;
        }

        private int cursorY {
            get => Console.CursorTop - startCursorY;
            set => Console.CursorTop = value + startCursorY;
        }

        private int startCursorY;

        private const    int    leftBoundSize = 8;
        private readonly string tabulation    = new string(' ', 4);

        internal ConsoleCodeEditor(string firstCodeLine) {
            lines.Add(firstCodeLine); // [0]
        }

        public ConsoleSyntaxHighlighter Highlighter { get; } = new ConsoleSyntaxHighlighter();

        public IEnumerable<string> BeginSession() {
            // print editor header
            output.ClearLine();
            output.WriteLines(
                "Press [Esc] twice to exit code editor",
                "┌─────┬" + new string('─', Console.BufferWidth - leftBoundSize)
            );

            // init bounds
            startCursorY = Console.CursorTop;

            // rewrite first line
            RenderLine();

            // main writing loop
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
                    // handle backspace
                    case ConsoleKey.Backspace: {
                        EraseLeftChar();
                        break;
                    }
                    // handle delete
                    case ConsoleKey.Delete: {
                        EraseRightChar();
                        break;
                    }
                    case ConsoleKey.Enter: {
                        SplitLine();
                        break;
                    }
                    // handle tabulation
                    case ConsoleKey.Tab: {
                        // Shift + Tab: outdent current tab
                        if (key.Modifiers == ConsoleModifiers.Shift) {
                        }
                        else {
                            line += tabulation;
                        }
                        break;
                    }
                    // handle arrow moves
                    // <-
                    case ConsoleKey.LeftArrow: {
                        MoveCursor(MoveDirection.Left);
                        break;
                    }
                    // ->
                    case ConsoleKey.RightArrow: {
                        MoveCursor(MoveDirection.Right);
                        break;
                    }
                    // /\
                    case ConsoleKey.UpArrow: {
                        MoveCursor(MoveDirection.Up);
                        break;
                    }
                    // \/
                    case ConsoleKey.DownArrow: {
                        MoveCursor(MoveDirection.Down);
                        break;
                    }
                    // any char that should be passed in code
                    default: {
                        line += key.KeyChar;
                        break;
                    }
                }
                key = Console.ReadKey(true);
            }

            EDITOR_EXIT:
            // remove empty lines
            lines.RemoveAll(ln => ln.Trim() == "");
            // finished work
            return lines;
        }

        private void SplitLine() {
            string tailPiece = line.Substring(cursorX);
            // cut current line
            line = line.Substring(0, cursorX);
            // move to next line
            cursorY++;
            // if on last line
            if (cursorY == lines.Count) {
                lines.Add(tailPiece);
                RenderLine();
                cursorX = 0;
            }
            else {
                lines.Insert(cursorY, tailPiece);
                // save cursor position
                int savedY = cursorY;
                // re-render rest lines
                for (; cursorY < lines.Count; cursorY++) {
                    RenderLine();
                }
                // restore cursor position
                cursorY = savedY;
                cursorX = 0;
            }
        }

        private void EraseLeftChar() {
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
                output.ClearLine();
                string prevLine = "";
                if (line.Length > 0) {
                    prevLine = line;
                }
                lines.RemoveAt(cursorY);
                // move cursor to previous line
                cursorY--;
                // append previous line if it is not empty
                line += prevLine;

                cursorX = line.Length;

                // save cursor position
                int savedY = cursorY;
                int savedX = cursorX;

                // re-render rest lines
                cursorY++;
                for (; cursorY < lines.Count; cursorY++) {
                    RenderLine();
                }
                // we're on last line (it is duplicated)
                output.ClearLine();
                // restore cursor position
                cursorY = savedY;
                cursorX = savedX;
            }
        }

        private void EraseRightChar() {
            // if on last line end
            if (cursorY == lines.Count - 1 &&
                cursorX > line.Length) {
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
                    RenderLine();
                }
                // we're on last line (it is duplicated)
                output.ClearLine();
                // restore cursor position
                cursorY = savedY;
                cursorX = savedX;
            }
        }

        private enum MoveDirection {
            Left,
            Right,
            Up,
            Down
        }

        private void MoveCursor(MoveDirection direction, int count = 1) {
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
                        int savedCursorX = cursorX;
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
                        int savedCursorX = cursorX;
                        cursorX = line.Length;
                    }
                    break;
                }
            }
        }

        #region Rendering

        /// <summary>
        ///     Full rewriting and highlighting of current code line.
        /// </summary>
        private void RenderLine() {
            output.ClearLine();
            PrintLineNumber();
            Highlighter.Highlight(line);
        }

        private void PrintLineNumber() {
            string view = "|";
            // left align if line number
            if (cursorY < 9) {
                view += "   ";
            }
            else if (cursorY < 99) {
                view += "  ";
            }
            else if (cursorY < 300) {
                view += " ";
            }
            // file too large to display.
            else {
                view += " <File too large to display. Please use external editor.>";
                output.Write(view);
                return;
            }
            // append line number and right aligner
            view += (cursorY + 1) + " | ";
            output.Write(view);
        }

        #endregion
    }
}