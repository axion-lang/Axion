using System.Linq;
using Axion.Core.Processing.Lexical.Tokens;

namespace Axion.Core.Specification {
    public partial class Spec {
        internal static readonly TokenType[] AccessModifiers = {
            TokenType.KeywordPublic,
            TokenType.KeywordInner,
            TokenType.KeywordPrivate
        };

        internal static readonly TokenType[] PropertyModifiers = {
        };

        internal static readonly TokenType[] Modifiers = AccessModifiers.Union(PropertyModifiers).ToArray();
    }
}