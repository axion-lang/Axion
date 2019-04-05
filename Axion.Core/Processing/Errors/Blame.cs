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

        private static string TypeToMessage(BlameType type) {
            string enumMemberName = type.ToString("G");

            var result = new StringBuilder();
            result.Append(char.ToUpper(enumMemberName[0]));

            enumMemberName = enumMemberName.Remove(0, 1);
            foreach (char c in enumMemberName) {
                if (char.IsUpper(c)) {
                    result.Append(" ").Append(char.ToLower(c));
                }
                else {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        public override string ToString() {
            return $"{Severity}: {Message} ({Span})";
        }
    }
}