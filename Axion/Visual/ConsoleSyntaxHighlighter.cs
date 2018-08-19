namespace Axion.Visual {
    internal class ConsoleSyntaxHighlighter {
        public void Highlight(string inputCode) {
            ConsoleView.Output.Write(inputCode);
        }
    }
}