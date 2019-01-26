using System.Text;

namespace Axion.Core.Processing.Errors {
    public class Blame {
        public string        Message  { get; }
        public BlameSeverity Severity { get; }

        public readonly Span Span;

        public Blame(
            string        message,
            BlameSeverity severity,
            Position      start,
            Position      end
        ) {
            Message  = message;
            Severity = severity;
            Span     = new Span(start, end);
        }

        public Blame(
            BlameType     type,
            BlameSeverity severity,
            Position      start,
            Position      end
        ) {
            Message  = TypeToMessage(type);
            Severity = severity;
            Span     = new Span(start, end);
        }

        public Blame(
            string        message,
            BlameSeverity severity,
            Span          span
        ) {
            Message  = message;
            Severity = severity;
            Span     = span;
        }

        public Blame(
            BlameType     type,
            BlameSeverity severity,
            Span          span
        ) {
            Message  = TypeToMessage(type);
            Severity = severity;
            Span     = span;
        }

        private string TypeToMessage(BlameType type) {
            string enumMemberName = type.ToString("G");

            StringBuilder result = new StringBuilder().Append(char.ToUpper(enumMemberName[0]));

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

        public string AsMessage() {
            return Severity + ": " +
                   Message +
                   $" (line {Span.StartPosition.Line + 1}, column {Span.StartPosition.Column + 1})";
        }
    }
}