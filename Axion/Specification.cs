using System.Collections.Generic;
using Axion.Processing.Tokens;

namespace Axion {
    /// <summary>
    ///     Static class, contains all language's syntax definitions (allowed operators, keywords, etc.)
    /// </summary>
    public static class Specification {
        /// <summary>
        ///     Contains all built-in Axion language keywords. TODO complete keywords list
        /// </summary>
        public static readonly string[] Keywords = {
            // loops
            "while", "foreach", "out", "next",
            // conditions
            "if", "else",
            "match", "case", "default",
            // variables
            "new", "delete", "as", "ref",
            "true", "false", "null",
            // errors
            "try", "catch", "anyway", "raise",
            // imports
            "use", "module",
            // modifiers
            "private", "inner", "public", "readonly", "static",
            "async", "await", "react",
            // definitions
            "class", "struct", "enum", "self",
            //
            "return", "yield"
        };

        /// <summary>
        ///     Contains all Axion language built-in types. TODO complete built-in types list
        /// </summary>
        public static readonly string[] BuiltInTypes = {
            "bool", "byte", "int16", "int", "int64",
            "float", "float64", "str"
        };

        public static readonly char[] StringQuotes = {
            '"', '\'', '`'
        };

        #region Operators

        public static readonly Dictionary<string, Operator> Operators = new Dictionary<string, Operator> {
            // TODO SET OPERATORS PRECEDENCE
            { ":", new Operator(":",     InputSide.Both,    false, 4) },
            { "?", new Operator("?",     InputSide.Both,    false, 4) },
            { "=", new Operator("=",     InputSide.Both,    false, 4) },
            { ",", new Operator(",",     InputSide.Both,    false, 10) },
            { "or", new Operator("or",   InputSide.Both,    false, 16) },
            { "and", new Operator("and", InputSide.Both,    false, 20) },
            { "==", new Operator("==",   InputSide.Both,    false, 26) },
            { "!=", new Operator("!=",   InputSide.Both,    false, 26) },
            { ">", new Operator(">",     InputSide.Both,    false, 30) },
            { "<", new Operator("<",     InputSide.Both,    false, 30) },
            { ">=", new Operator(">=",   InputSide.Both,    false, 30) },
            { "<=", new Operator("<=",   InputSide.Both,    false, 30) },
            { "+", new Operator("+",     InputSide.Both,    false, 34) },
            { "-", new Operator("-",     InputSide.Both,    false, 34) },
            { "*", new Operator("*",     InputSide.Both,    false, 38) },
            { "**", new Operator("**",   InputSide.Both,    false, 38) }, // check
            { "/", new Operator("/",     InputSide.Both,    false, 38) },
            { "%", new Operator("%",     InputSide.Both,    false, 42) },
            { "not", new Operator("not", InputSide.Right,   false, 46) },
            { "++", new Operator("++",   InputSide.SomeOne, false, 47) },
            { "--", new Operator("--",   InputSide.SomeOne, false, 47) },
            { ".", new Operator(".",     InputSide.Both,    false, 52) },
            { "{", new Operator("{",     InputSide.Both,    false, 100) },
            { "}", new Operator("}",     InputSide.Left,    false, 99) },
            { "[", new Operator("[",     InputSide.Both,    false, 100) },
            { "]", new Operator("]",     InputSide.Left,    false, 99) },
            { "(", new Operator("(",     InputSide.Both,    false, 100) },
            { ")", new Operator(")",     InputSide.Left,    false, 99) }
        };

        internal static readonly char[] OperatorChars = {
            '+', '-', '*', '/', '%',
            '!', '>', '<', '=', '|',
            '&', '^', '~', '.',
            '{', '}', '[', ']', '(', ')',
            ',', ':', '?'
        };

        internal static readonly string[] BoolOperators = {
            ">", "<", ">=", "<=", "==", "!=",
            "not", "and", "or", "in"
        };

        #endregion

        #region Regular Expressions

        /* //TODO add number regex checks
        // copied from python integer parser
        private static readonly Regex HexNumber = new Regex("0[xX](?:_?[0-9a-fA-F])+");
        private static readonly Regex BinNumber = new Regex("0[bB](?:_?[01])+");
        private static readonly Regex OctNumber = new Regex("0[oO](?:_?[0-7])+");
        private static readonly Regex DecNumber = new Regex("(?:0(?:_?0)*|[1-9](?:_?[0-9])*)");
        private static readonly Regex IntNumber = new Regex(Group(Hexnumber, Binnumber, Octnumber, Decnumber));
        private static readonly Regex Exponent = new Regex("[eE][-+]?[0-9](?:_?[0-9])*");
        private static readonly Regex PointFloat = new Regex(Group("[0-9](?:_?[0-9])*\\.(?:[0-9](?:_?[0-9])*)?", "\\.[0-9](?:_?[0-9])*" + Maybe(Exponent)));
        private static readonly Regex ExpFloat = new Regex("[0-9](?:_?[0-9])*" + Exponent);
        private static readonly Regex FloatNumber = new Regex(Group(Pointfloat, Expfloat));
        private static readonly Regex ImagNumber = new Regex(Group("[0-9](?:_?[0-9])*[jJ]", Floatnumber + "[jJ]"));
        private static readonly Regex Number = new Regex(Group(Imagnumber, Floatnumber, Intnumber));
        */

        #endregion
    }
}