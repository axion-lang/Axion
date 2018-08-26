using System;
using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing;

namespace Axion.Core.Visual {
    internal class ConsoleCodeEditor {
        private readonly ConsoleSyntaxHighlighter highlighter = new ConsoleSyntaxHighlighter();

        private readonly List<string> lines = new List<string>();

        private string line {
            get => lines[cursorY];
            set {
                lines[cursorY] = value;
                // if line modified - it should be re-rendered
                HighlightLine();
            }
        }

        #region Cursor position

        private int cursorX {
            // check that value is in bounds
            get => Console.CursorLeft > leftBoundSize
                       ? Console.CursorLeft - leftBoundSize
                       : 0;
            set {
                int length = leftBoundSize + value;
                // grow buffer width if cursor out of it.
                if (length >= Console.BufferWidth) {
                    Console.BufferWidth = length + 1;
                }
                Console.CursorLeft = length;
            }
        }

        private int cursorY {
            // check that value is in bounds
            get => Console.CursorTop > topBoundSize
                       ? Console.CursorTop - topBoundSize
                       : 0;
            set => Console.CursorTop = topBoundSize + value;
        }

        #endregion

        #region Error box properties

        private       int    errorBoxX;
        private       int    errorBoxY;
        private const string errorBoxPrefix = "| Error: ";

        #endregion

        private readonly int leftBoundSize;
        private          int topBoundSize;

        private readonly string tabulation = new string(' ', 4);
        private readonly bool   singleLineMode;
        private readonly bool   syntaxHighlighting;
        private readonly string prompt;

        public ConsoleCodeEditor(
            bool   singleLineMode,
            bool   syntaxHighlighting,
            string prompt        = "",
            string firstCodeLine = ""
        ) {
            this.singleLineMode     = singleLineMode;
            this.syntaxHighlighting = syntaxHighlighting;
            this.prompt             = prompt;
            lines.Add(firstCodeLine);
            leftBoundSize = singleLineMode && prompt.Length > 0
                                ? prompt.Length
                                : 8; // TODO fixes in single-line mode
        }

        public string[] BeginSession() {
            ClearLine();

            // render editor header and top frame
            if (!singleLineMode) {
                ConsoleUI.WriteLine();
                ConsoleUI.WriteLine("Press [Esc] twice to exit code editor");

                if (syntaxHighlighting) {
                    ConsoleUI.WriteLine("┌──────" + new string('─', Console.BufferWidth - leftBoundSize));
                    ConsoleUI.Write(errorBoxPrefix);
                    // save size & position of error box
                    errorBoxX = Console.CursorLeft;
                    errorBoxY = Console.CursorTop;
                    // no error occurred
                    ConsoleUI.WriteLine("none");
                    ConsoleUI.WriteLine("├─────┬" + new string('─', Console.BufferWidth - leftBoundSize));
                }
                else {
                    ConsoleUI.WriteLine("┌─────┬" + new string('─', Console.BufferWidth - leftBoundSize));
                }
            }

            // init editor field bounds
            topBoundSize = Console.CursorTop;
            cursorX      = line.Length;

            // highlight first line
            HighlightLine();

            // writing loop
            ConsoleKeyInfo key = Console.ReadKey(true);
            while (true) {
                switch (key.Key) {
                    case ConsoleKey.Escape: {
                        if (singleLineMode) {
                            break;
                        }
                        key = Console.ReadKey(true);
                        if (key.Key == ConsoleKey.Escape) {
                            // [Esc] pressed twice -> exit
                            goto EDITOR_EXIT;
                        }
                        continue;
                    }

                    #region Move cursor with arrows

                    case ConsoleKey.LeftArrow: {
                        MoveCursor(CursorMoveDirection.Left);
                        break;
                    }
                    case ConsoleKey.RightArrow: {
                        MoveCursor(CursorMoveDirection.Right);
                        break;
                    }
                    case ConsoleKey.UpArrow: {
                        MoveCursor(CursorMoveDirection.Up);
                        break;
                    }
                    case ConsoleKey.DownArrow: {
                        MoveCursor(CursorMoveDirection.Down);
                        break;
                    }

                    #endregion

                    case ConsoleKey.Enter: {
                        if (singleLineMode) {
                            goto EDITOR_EXIT;
                        }
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
                    // TODO add something for FN keys
                    case ConsoleKey.F1:
                    case ConsoleKey.F2:
                    case ConsoleKey.F3:
                    case ConsoleKey.F4:
                    case ConsoleKey.F5:
                    case ConsoleKey.F6:
                    case ConsoleKey.F7:
                    case ConsoleKey.F8:
                    case ConsoleKey.F9:
                    case ConsoleKey.F10:
                    case ConsoleKey.F11:
                    case ConsoleKey.F12: {
                        break;
                    }
                    case ConsoleKey.Home: {
                        // to line start
                        cursorX = 0;
                        break;
                    }
                    case ConsoleKey.End: {
                        // to line end
                        cursorX = line.Length;
                        break;
                    }
                    case ConsoleKey.PageUp: {
                        break;
                    }
                    case ConsoleKey.PageDown: {
                        break;
                    }
                    case ConsoleKey.Insert: {
                        // do not process insert key
                        break;
                    }
                    default: {
                        // auto complete quotes to prevent
                        // highlighter from showing an error.
                        if (Spec.StringQuotes.Contains(key.KeyChar) ||
                            key.KeyChar == Spec.CharLiteralQuote) {
                            // double quotes
                            WriteValue(key.KeyChar + key.KeyChar.ToString());
                            break;
                        }
                        // any other char that should be passed in code
                        WriteValue(key.KeyChar);
                        break;
                    }
                }
                key = Console.ReadKey(true);
            }

            EDITOR_EXIT:

            // render bottom frame
            if (!singleLineMode) {
                ConsoleUI.WriteLine("\n└─────┴" + new string('─', Console.BufferWidth - leftBoundSize));
            }

            // remove empty lines
            lines.RemoveAll(ln => ln.Trim().Length == 0);

            // finished work
            return lines.Count == 0
                       ? new[] { "" }
                       : lines.ToArray();
        }

        /// <summary>
        ///     Splits line in cursor position into 2 lines.
        /// </summary>
        private void SplitLine() {
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
                PrintError(ex);
            }
        }

        /// <summary>
        ///     Erases character on the left side of cursor.
        /// </summary>
        private void EraseLeftChar() {
            // if on first line start
            if (cursorY == 0 &&
                cursorX == 0) {
                return;
            }
            // if cursor in current line
            if (cursorX > 0) {
                line = line.Remove(cursorX - 1, 1);
                cursorX--;
            }
            // remove current line
            else {
                var prevLine = "";
                if (line.Length > 0) {
                    prevLine = line;
                }
                ClearLine();
                lines.RemoveAt(cursorY);
                // move cursor to previous line
                cursorY--;
                // append previous line if it is not empty
                line += prevLine;

                cursorX = line.Length - prevLine.Length;

                ConsoleUI.DoAfterCursor(
                    () => {
                        // re-render rest lines
                        cursorY++;
                        for (; cursorY < lines.Count; cursorY++) {
                            HighlightLine();
                        }
                        // we're on last line (it is duplicated)
                        ClearLine();
                    }
                );
            }
        }

        /// <summary>
        ///     Erases character on the right side of cursor.
        /// </summary>
        private void EraseRightChar() {
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
                ConsoleUI.DoAfterCursor(
                    () => {
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
                        ClearLine();
                    }
                );
            }
        }

        /// <summary>
        ///     Writes specified <see cref="value" /> to editor.
        /// </summary>
        private void WriteValue(object value) {
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

        private void MoveCursor(CursorMoveDirection direction, int count = 1) {
            switch (direction) {
                case CursorMoveDirection.Left: {
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
                    else if (!singleLineMode) {
                        cursorY--;
                        cursorX = line.Length; // no - 1
                    }
                    break;
                }
                case CursorMoveDirection.Right: {
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
                    else if (!singleLineMode) {
                        cursorY++;
                        cursorX = 0;
                    }
                    break;
                }
                case CursorMoveDirection.Up: {
                    // if on first line
                    if (cursorY == 0 ||
                        singleLineMode) {
                        return;
                    }
                    cursorY--;
                    // if cursor moves at empty space upside
                    if (cursorX >= line.Length) {
                        cursorX = line.Length;
                    }
                    break;
                }
                case CursorMoveDirection.Down: {
                    // if on last line
                    if (cursorY == lines.Count - 1 ||
                        singleLineMode) {
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
        ///     Clears current line.
        /// </summary>
        private void ClearLine() {
            ConsoleUI.ClearLine();
            ConsoleUI.Write(prompt);
        }

        /// <summary>
        ///     Full rewriting and highlighting of current code line.
        ///     You must reset <see cref="cursorX" /> after invoking that
        ///     function and after changing <see cref="line" /> value.
        /// </summary>
        private void HighlightLine() {
            ConsoleUI.DoAfterCursor(
                () => {
                    ClearLine();
                    if (!singleLineMode && syntaxHighlighting) {
                        EraseError();
                        PrintLineNumber();
                        if (line.Trim().Length == 0) {
                            // if line empty just print whitespace
                            ConsoleUI.Write(line);
                        }
                        else {
                            // invoke highlighter
                            highlighter.Highlight(line, out List<SourceProcessingException> errors);
                            if (errors.Count != 0) {
                                PrintError(errors[0]);
                            }
                        }
                    }
                    else {
                        ConsoleUI.Write(line);
                    }
                }
            );
        }

        /// <summary>
        ///     In multiline mode, prints line
        ///     number on left side of editor.
        ///     That number not included in code.
        /// </summary>
        private void PrintLineNumber() {
            if (!singleLineMode) {
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
        }

        /// <summary>
        ///     In multiline mode with syntax highlighting,
        ///     erases current
        /// </summary>
        private void EraseError() {
            ConsoleUI.DoAfterCursor(
                () => {
                    ClearLine();
                    ConsoleUI.Write(errorBoxPrefix);
                }
            );
        }

        private void PrintError(Exception errorMessage) {
            ConsoleUI.DoWithPosition(
                errorBoxX, errorBoxY, () => {
                    EraseError();
                    ConsoleUI.Write((errorMessage.Message, ConsoleColor.DarkRed));
                }
            );
        }

        private enum CursorMoveDirection {
            Left,
            Right,
            Up,
            Down
        }

        /// <summary>
        ///     Occurs when lines in editor exceeds maximal allowed value.
        /// </summary>
        private class FileTooLargeException : Exception {
            public FileTooLargeException()
                : base("File too large to display. Please use external editor.") {
            }
        }
    }
}