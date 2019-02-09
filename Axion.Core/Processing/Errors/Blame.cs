using System.Text;

namespace Axion.Core.Processing.Errors {
    public class Blame {
        public readonly string        Message;
        public readonly BlameSeverity Severity;
        public readonly Span          Span;

        public Blame(BlameType type, BlameSeverity severity, Position start, Position end) {
            Message  = TypeToMessage(type);
            Severity = severity;
            Span     = new Span(start, end);
        }

        public Blame(string message, BlameSeverity severity, Span span) {
            Message  = message;
            Severity = severity;
            Span     = span;
        }

        public string AsMessage() {
            return
                $"{Severity}: {Message} (line {Span.StartPosition.Line + 1}, column {Span.StartPosition.Column + 1})";
        }

        private static string TypeToMessage(BlameType type) {
            string enumMemberName = type.ToString("G");

            var result = new StringBuilder();
            result.Append(char.ToUpper(enumMemberName[0]));

            enumMemberName = enumMemberName.Remove(0, 1);
            for (var i = 0; i < enumMemberName.Length; i++) {
                char c = enumMemberName[i];
                if (char.IsUpper(c)) {
                    result.Append(" ").Append(char.ToLower(c));
                }
                else {
                    result.Append(c);
                }
            }
            return result.ToString();
        }
    }
}