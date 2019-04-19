namespace Axion.Core.Specification {
    public partial class Spec {
        internal static bool IsValidOctalDigit(this char c) {
            return c == '0'
                   || c == '1'
                   || c == '2'
                   || c == '3'
                   || c == '4'
                   || c == '5'
                   || c == '6'
                   || c == '7';
        }

        internal static bool IsValidHexadecimalDigit(this char c) {
            return c == '0'
                   || c == '1'
                   || c == '2'
                   || c == '3'
                   || c == '4'
                   || c == '5'
                   || c == '6'
                   || c == '7'
                   || c == '8'
                   || c == '9'
                   || c == 'a'
                   || c == 'b'
                   || c == 'c'
                   || c == 'd'
                   || c == 'e'
                   || c == 'f'
                   || c == 'A'
                   || c == 'B'
                   || c == 'C'
                   || c == 'D'
                   || c == 'E'
                   || c == 'F';
        }

        internal static bool IsSpaceOrTab(this char c) {
            return c == ' ' || c == '\t';
        }

        internal static bool IsLetterOrNumberPart(this char c) {
            return char.IsLetterOrDigit(c) || c == '_' || c == '.';
        }

        internal static bool IsValidIdStart(this char start) {
            return char.IsLetter(start) || start == '_';
        }

        internal static bool IsValidIdChar(this char c) {
            return char.IsLetterOrDigit(c) || c == '_';
        }
    }
}