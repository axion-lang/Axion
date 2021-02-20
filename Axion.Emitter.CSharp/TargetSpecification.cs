using System.Collections.Generic;
using System.Collections.Immutable;
using Axion.Core;
using Axion.Core.Processing.Lexical.Tokens;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Emitter.CSharp {
    public class TargetSpecification {
        public static readonly string[] AccessModifiers = {
            "private",
            "internal",
            "protected",
            "public",
            "private-protected",
            "protected-internal"
        };

        public static readonly string[] AllowedModifiers =
            AccessModifiers.Union(
                "abstract",
                "const",
                "extern",
                "override",
                "partial",
                "readonly",
                "sealed",
                "unsafe",
                "virtual",
                "volatile",
                "static"
            );

            // @formatter:off

            public static readonly ImmutableDictionary<string, string> BuiltInNames = new Dictionary<string, string> {
                { "Int8",         "sbyte" },
                { "UInt8",        "byte" },
                { "Int16",        "short" },
                { "UInt16",       "ushort" },
                { "Int32",        "int" },
                { "UInt32",       "uint" },
                { "Int64",        "long" },
                { "UInt64",       "ulong" },
                { "Float32",      "float" },
                { "Float64",      "double" },
                { "Float128",     "decimal" },
                { "Char",         "char" },
                { "Bool",         "bool" },
                { "Object",       "object" },
                { "self",         "this" }
            }.ToImmutableDictionary();

            public static readonly ImmutableDictionary<TokenType, string> BinaryOperators = new Dictionary<TokenType, string> {
                { And,                "&&" },
                { Or,                 "||" }
            }.ToImmutableDictionary();
    }
}
