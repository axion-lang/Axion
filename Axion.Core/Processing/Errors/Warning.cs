namespace Axion.Core.Processing.Errors {
    public class Warning : IBlame {
        public WarningType Type { get; }

        public (int line, int column) StartPosition { get; }
        public (int line, int column) EndPosition   { get; }

        public Warning(WarningType type, (int line, int column) startPosition, (int line, int column) endPosition) {
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
            return "Warning: " +
                   result +
                   $" (line {StartPosition.line + 1}, column {StartPosition.column + 1})";
        }
    }
}