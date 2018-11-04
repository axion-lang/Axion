using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Axion.Core.Processing;
using Axion.Core.Tokens;

namespace Axion.Core.Visual {
    /// <summary>
    ///     Embedded in console code editor with optional
    ///     syntax highlighting & handy keyboard shortcuts.
    /// </summary>
    internal class ConsoleCodeEditor {
        #region Editor view properties

        /// <summary>
        ///     Width of line number field at left side of code editor.
        /// </summary>
        internal const int LineNumberWidth = 7;

        /// <summary>
        ///     Editor's X position in console.
        /// </summary>
        private readonly int editBoxX;

        /// <summary>
        ///     Editor's Y position in console.
        /// </summary>
        private int editBoxY;

        /// <summary>
        ///     X position of editor's message box.
        ///     (works only with <see cref="syntaxHighlighting"/>
        ///      and not in <see cref="singleLineMode"/>).
        /// </summary>
        private int messageBoxX;

        /// <summary>
        ///     Y position of editor's message box.
        ///     (works only with <see cref="syntaxHighlighting"/>
        ///      and not in <see cref="singleLineMode"/>).
        /// </summary>
        private int messageBoxY;

        #endregion

        #region Editor settings

        /// <summary>
        ///     Tab character in editor.
        /// </summary>
        private readonly string tabulation = new string(' ', 4);

        /// <summary>
        ///     Editor should use only 1 editable line.
        /// </summary>
        private readonly bool singleLineMode;

        /// <summary>
        ///     Editor should highlight user input
        ///     with specified <see cref="highlighter"/>.
        /// </summary>
        private readonly bool syntaxHighlighting;

        /// <summary>
        ///     User prompt used when editor
        ///     launched in single-line mode.
        /// </summary>
        private readonly string prompt;

        #endregion

        #region Line, cursor

        /// <summary>
        ///     Current editing line.
        ///     This property automatically calls
        ///     <see cref="RenderCode"/> when modified.
        ///     You should use <see cref="lines"/>[<see cref="cursorY"/>] when you
        ///     don't want to re-render input instead.
        /// </summary>
        private string line {
            get => lines[cursorY];
            set {
                lines[cursorY] = value;
                // if line modified - it should be re-rendered
                RenderCode();
            }
        }

        /// <summary>
        ///     A wrapper over <see cref="Console.CursorLeft"/>.
        ///     Position is relative to <see cref="editBoxX"/>.
        ///     Automatically grows or reduces <see cref="Console.BufferWidth"/>
        ///     based longest line length.
        /// </summary>
        private int cursorX {
            // check that value is in bounds
            get => Console.CursorLeft - editBoxX;
            set {
                int length = value + editBoxX;
                // resize buffer
                if (length >= Console.BufferWidth) {
                    // grow buffer width if cursor out of it.
                    Console.BufferWidth = length + 1;
                }
                else {
                    // buffer width should be equal to the longest line length.
                    int maxLength = lines.Max(x => x.Length);
                    if (maxLength > Console.WindowWidth) {
                        Console.BufferWidth = maxLength + editBoxX;
                    }
                }
                Console.CursorLeft = length;
            }
        }

        /// <summary>
        ///     A wrapper over <see cref="Console.CursorTop"/>.
        ///     Position is relative to <see cref="editBoxY"/>.
        /// </summary>
        private int cursorY {
            get => Console.CursorTop - editBoxY;
            set => Console.CursorTop = value + editBoxY;
        }

        #endregion

        /// <summary>
        ///     Current console highlighter.
        /// </summary>
        private readonly ConsoleSyntaxHighlighter highlighter;

        private readonly Stopwatch highlightingWatch = new Stopwatch();
        private bool lastTimeRendered = false;

        /// <summary>
        ///     Editor's code lines.
        /// </summary>
        private readonly List<string> lines = new List<string>();

        /// <summary>
        ///     Creates new console editor instance.
        /// </summary>
        /// <param name="singleLineMode">
        ///     Editor uses only 1 editable line.
        /// </param>
        /// <param name="syntaxHighlighting">
        ///     Enable specified syntax highlighting.
        /// </param>
        /// <param name="prompt">
        ///     A permanent prompt to user to input something. Works only with <paramref name="singleLineMode" />.
        /// </param>
        /// <param name="firstCodeLine">
        ///     Appends specified first line to the editor.
        /// </param>
        public ConsoleCodeEditor(
            bool   singleLineMode,
            bool   syntaxHighlighting,
            string prompt        = "",
            string firstCodeLine = ""
        ) {
            this.syntaxHighlighting = syntaxHighlighting;
            if (syntaxHighlighting) {
                highlighter = new ConsoleSyntaxHighlighter(this);
            }

            this.singleLineMode = singleLineMode;
            if (singleLineMode && prompt.Length > 0) {
                this.prompt = prompt;
                editBoxX    = prompt.Length;
            }
            else {
                editBoxX = LineNumberWidth + 1;
            }

            lines.Add(firstCodeLine);
        }

        /// <summary>
        ///     Starts the editor (draws bounds, highlights code, etc.)
        ///     and returns prepared code lines when user finished editing.
        /// </summary>
        public string[] BeginSession() {
            ConsoleUI.ClearLine();
            if (singleLineMode) {
                Console.Write(prompt);
            }
            else {
                // draw editor header and top frame
                ConsoleUI.WriteLine("Press [Esc] twice to exit code editor");
                if (syntaxHighlighting) {
                    // draw message box frame
                    Console.Write(
                        "┌──────" + new string('─', Console.BufferWidth - editBoxX) + "\n" +
                        "| "
                    );
                    // save position of message box
                    messageBoxX = Console.CursorLeft;
                    messageBoxY = Console.CursorTop;
                    // draw editor frame
                    ConsoleUI.WriteLine(
                        "No errors\n" +
                        "├─────┬" + new string('─', Console.BufferWidth - editBoxX)
                    );
                }
                else {
                    // else draw upper bound
                    Console.WriteLine("┌─────┬" + new string('─', Console.BufferWidth - editBoxX));
                }
            }

            editBoxY = Console.CursorTop;
            cursorX  = line.Length;

            // highlight first line
            RenderCode();

            return ReadLines();
        }

        /// <summary>
        ///     Reads and processes user input.
        /// </summary>
        private string[] ReadLines() { // BUG: lines splitted incorrectly
            // writing loop
            ConsoleKeyInfo key = Console.ReadKey(true);
            while (true) {
                switch (key.Key) {
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

                    case ConsoleKey.Escape: {
                        // use [Enter] in single line mode instead.
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
                    case ConsoleKey.Tab: {
                        // Shift + Tab: outdent current tab
                        if (key.Modifiers == ConsoleModifiers.Shift && line.StartsWith(tabulation)) {
                            // PERF: Outdent doesn't need redrawing, but... (maybe use console buffer copy)
                            line = line.Remove(0, tabulation.Length);
                        }
                        // if on the left side of cursor there are only whitespaces
                        else if (string.IsNullOrWhiteSpace(line.Substring(0, cursorX))) {
                            // PERF: doesn't need redrawing as above.
                            line    =  tabulation + line;
                            cursorX += tabulation.Length;
                        }
                        else {
                            WriteValue(" ");
                        }
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
                        if (cursorY - 10 >= 0) {
                            cursorY -= 10;
                        }
                        else {
                            cursorY = 0;
                        }
                        if (cursorX > line.Length) {
                            cursorX = line.Length;
                        }
                        break;
                    }
                    case ConsoleKey.PageDown: {
                        if (cursorY + 10 < lines.Count) {
                            cursorY += 10;
                        }
                        else {
                            cursorY = lines.Count - 1;
                        }
                        if (cursorX > line.Length) {
                            cursorX = line.Length;
                        }
                        break;
                    }
                    case ConsoleKey.Insert: {
                        // do not process insert key
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
                    // any other char that should be passed in code
                    default: {
                        WriteValue(key.KeyChar);
                        break;
                    }
                }
                key = Console.ReadKey(true);
            }

            EDITOR_EXIT:
            // render bottom frame
            if (!singleLineMode) {
                ConsoleUI.WriteLine("\n└─────┴" + new string('─', Console.BufferWidth - editBoxX));
            }

            // remove empty lines
            lines.RemoveAll(ln => ln.Trim().Length == 0);

            // finished work
            return lines.Count == 0 ? new[] { "" } : lines.ToArray();
        }

        #region Source code - dependent functions

        /// <summary>
        ///     Splits line at cursor position into 2 lines.
        /// </summary>
        private void SplitLine() {
            try {
                string tailPiece = line.Substring(cursorX);
                if (tailPiece.Length > 0) {
                    // cut current line
                    lines[cursorY] = lines[cursorY].Substring(0, cursorX);
                }
                // move to next line
                MoveToNextLineStart();
                // add tail to next line
                if (cursorY == lines.Count) {
                    lines.Add(tailPiece);
                }
                else {
                    lines.Insert(cursorY, tailPiece);
                }
                RenderCode();
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
            if (cursorX == 0 &&
                cursorY == 0) {
                return;
            }
            // if cursor in current line
            if (cursorX > 0) {
                // if erasing empty end of line
                if (cursorX == line.Length && line.Length != line.TrimEnd().Length) {
                    lines[cursorY] = lines[cursorY].Substring(0, line.Length - 1);
                }
                else {
                    line = line.Remove(cursorX - 1, 1);
                }
                cursorX--;
            }
            // cursor X == 0
            else {
                string removingLine = line;
                lines.RemoveAt(cursorY);
                ClearLine(true);
                // move cursor to previous line
                cursorY--;
                // append removing line if it is not empty
                if (removingLine.Length > 0) {
                    lines[cursorY] += removingLine;
                    cursorX        =  line.Length - removingLine.Length;
                }
                else {
                    cursorX = line.Length;
                }
                RenderCode();
                // remove duplicated last code line,
                // because we moved all code under cursor up by 1.
                ConsoleUI.WithCurrentPosition(
                    () => {
                        cursorY = lines.Count;
                        ClearLine(true);
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
                cursorX == line.Length + 1) {
                return;
            }
            // cursor doesn't move when removing right character
            ConsoleUI.WithCurrentPosition(
                () => {
                    if (cursorX == line.Length) { // remove current line
                        // append next line to current
                        line += lines[cursorY + 1];
                        // remove next line
                        lines.RemoveAt(cursorY + 1);
                        RenderCode();
                        // remove duplicated last code line,
                        // because we moved all code under cursor up by 1.
                        cursorY = lines.Count;
                        ClearLine();
                    }
                    // if cursor inside the current line
                    else {
                        if (line.Substring(cursorX).TrimEnd() == "") {
                            // don't redraw line when at the right
                            // side of cursor are only whitespaces.
                            lines[cursorY] = lines[cursorY].Substring(0, line.Length - 1);
                        }
                        else {
                            line = line.Remove(cursorX, 1);
                        }
                    }
                }
            );
        }

        /// <summary>
        ///     Writes specified <see cref="value" /> to editor.
        /// </summary>
        private void WriteValue(object value) {
            string str = value.ToString();
            // write value
            if (cursorX == line.Length) {
                line += str;
            }
            else if (cursorX < line.Length) {
                line = line.Insert(cursorX, str);
            }
            else {
                throw new Exception(nameof(ConsoleCodeEditor) + ": cursor X went through end of line.");
            }
            cursorX += str.Length;
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
        ///     _
        /// </summary>
        internal void WriteMultilineValue(Token token, ConsoleColor color) {
            string   code       = token.ToAxionCode();
            string[] tokenLines = code.Split(Spec.Newlines, StringSplitOptions.None);
            if (tokenLines.Length > 1) {
                SetCursor(token.StartColumn, token.StartLine);
                for (var i = 0; i < tokenLines.Length - 1; i++) {
                    ConsoleUI.Write((tokenLines[i], color));
                    cursorY++;
                    cursorX = 0;
                }
                ConsoleUI.Write((tokenLines.Last(), color));
            }
            else {
                ConsoleUI.Write((code, color));
            }
        }

        internal void SetCursor(int x, int y) {
            cursorX = x;
            cursorY = y;
        }

        internal void MoveToNextLineStart() {
            if (cursorY == lines.Count) {
                ClearLine();
                PrintLineNumber(cursorY);
            }
            cursorY++;
            cursorX = 0;
        }

        #endregion

        #region View-dependent functions

        /// <summary>
        ///     In multiline mode, prints line
        ///     number on left side of editor.
        ///     That number not included in code.
        /// </summary>
        internal static void PrintLineNumber(int lineNumber) {
            var view = "|";

            // left align line number
            // |   X |
            if (lineNumber < 10) {
                view += "   ";
            }
            // |  XX |
            else if (lineNumber < 100) {
                view += "  ";
            }
            // | XXX |
            else if (lineNumber < 301) {
                view += " ";
            }
            // file too large to display.
            else {
                throw new FileTooLargeException();
            }
            // append line number and right aligner
            view += lineNumber + " | ";
            ConsoleUI.Write(view);
        }

        /// <summary>
        ///     Clears current line.
        /// </summary>
        private void ClearLine(bool fullClear = false) {
            ConsoleUI.ClearLine();
            if (!fullClear) {
                if (singleLineMode) {
                    ConsoleUI.Write(prompt);
                }
                else {
                    PrintLineNumber(cursorY + 1);
                }
            }
        }

        /// <summary>
        ///     Full rewriting and highlighting of current code line.
        ///     You must reset <see cref="cursorX" /> after invoking that
        ///     function and after changing <see cref="line" /> value.
        /// </summary>
        private void RenderCode() {
            highlightingWatch.Stop();
            ConsoleUI.WithCurrentPosition(
                () => {
                    ClearLine();
                    if (syntaxHighlighting && (highlightingWatch.ElapsedMilliseconds > 200 || !lastTimeRendered)) {
                        if (line.Trim().Length == 0) {
                            // if line empty just print whitespace
                            ConsoleUI.Write(line);
                        }
                        else {
                            EraseMessageBox();
                            // invoke highlighter
                            var errors   = new List<SyntaxError>();
                            var warnings = new List<SyntaxError>();
                            highlighter.Highlight(lines, ref errors, ref warnings);
                            if (errors.Count != 0) {
                                PrintError(errors[0]);
                            }
                            else if (warnings.Count != 0) {
                                PrintWarning(warnings[0]);
                            }
                        }
                        lastTimeRendered = true;
                    }
                    // if line isn't rendered
                    else {
                        ConsoleUI.Write(line);
                        lastTimeRendered = false;
                    }
                }
            );
            highlightingWatch.Restart();
        }

        private void PrintError(Exception error) {
            EraseMessageBox();
            ConsoleUI.WithPosition(
                messageBoxX, messageBoxY, () => {
                    ConsoleUI.Write((error.Message, ConsoleColor.DarkRed));
                    if (error is SyntaxError syntaxError) {
                        ConsoleUI.Write(
                            ($" (line {syntaxError.Token.StartLine + 1}, column {syntaxError.Token.StartColumn + 1})", ConsoleColor.DarkRed)
                        );
                    }
                }
            );
        }

        private void PrintWarning(Exception warning) {
            EraseMessageBox();
            ConsoleUI.WithPosition(
                messageBoxX, messageBoxY, () => {
                    ConsoleUI.Write((warning.Message, ConsoleColor.DarkYellow));
                    if (warning is SyntaxError syntaxWarning) {
                        ConsoleUI.Write(
                            ($" (line {syntaxWarning.Token.StartLine + 1}, column {syntaxWarning.Token.StartColumn + 1})", ConsoleColor.DarkRed)
                        );
                    }
                }
            );
        }

        /// <summary>
        ///     In multiline mode with syntax highlighting,
        ///     erases current displaying message in editor message box.
        /// </summary>
        private void EraseMessageBox() {
            ConsoleUI.WithPosition(
                messageBoxX, messageBoxY, () => {
                    ConsoleUI.ClearLine(2);
                    Console.Write("Code OK.");
                }
            );
        }

        #endregion

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
            public FileTooLargeException() : base("File too large to display. Please use external editor.") {
            }
        }
    }
}