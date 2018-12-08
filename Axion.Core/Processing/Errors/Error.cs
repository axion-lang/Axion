namespace Axion.Core.Processing.Errors {
    public class Error : IBlame {
        public ErrorType Type { get; }

        public (int line, int column) StartPosition { get; }
        public (int line, int column) EndPosition   { get; }

        public Error(ErrorType type, (int line, int column) startPosition, (int line, int column) endPosition) {
            Type          = type;
            StartPosition = startPosition;
            EndPosition   = endPosition;
        }

        public string AsMessage() {
            string enumMemberName = Type.ToString("G");
            string result         = "" + char.ToUpper(enumMemberName[0]);
            enumMemberName = enumMemberName.Remove(0, 1);
            for (var i = 0; i < enumMemberName.Length; i++) {
                char c = enumMemberName[i];
                if (char.IsUpper(c)) {
                    result += " " + char.ToLower(c);
                }
                else {
                    result += c;
                }
            }
            return "Error: " +
                   result +
                   $" (line {StartPosition.line + 1}, column {StartPosition.column + 1})";
        }
    }
}