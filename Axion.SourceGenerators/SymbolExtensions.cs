using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Axion.SourceGenerators {
    internal static class SymbolExtensions {
        public static bool DefaultEquals(
            this INamedTypeSymbol symbol,
            INamedTypeSymbol      other
        ) {
            return SymbolEqualityComparer.Default.Equals(symbol, other);
        }

        public static bool TryGetAttribute(
            this ISymbol                   symbol,
            INamedTypeSymbol               attributeType,
            out IEnumerable<AttributeData> attributes
        ) {
            attributes = symbol.GetAttributes()
                .Where(a => a.AttributeClass!.DefaultEquals(attributeType));
            return attributes.Any();
        }

        public static bool HasAttribute(
            this ISymbol     symbol,
            INamedTypeSymbol attributeType
        ) {
            return symbol.GetAttributes()
                .Any(
                    a => SymbolEqualityComparer.Default.Equals(
                        a.AttributeClass,
                        attributeType
                    )
                );
        }
    }
}
