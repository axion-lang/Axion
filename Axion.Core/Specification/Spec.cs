using System.Collections.Generic;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Specification {
    /// <summary>
    ///     Static class, contains all language's syntax definitions (allowed operators, keywords, etc.)
    /// </summary>
    public static partial class Spec {
        public const string SourceFileExtension = ".ax";
        public const string OutputFileExtension = ".ax";

        /// <summary>
        ///     End of source stream mark.
        /// </summary>
        public const char EndOfCode = '\0';

        /// <summary>
        ///     Contains all valid newline sequences.
        /// </summary>
        public static readonly string[] EndOfLines = { "\r\n", "\n" };
        
        internal const string CommentStart             = "#";
        internal const string MultiCommentStart        = "#|";
        internal const string MultiCommentStartPattern = @"\#\|";
        internal const string MultiCommentEnd          = "|#";
        internal const string MultiCommentEndPattern   = @"\|\#";

        /// <summary>
        ///     Contains all language keywords.
        /// </summary>
        public static readonly Dictionary<string, TokenType> Keywords =
            new Dictionary<string, TokenType> {
                { "all",       KeywordAll      },
                // bool operators
                { "not",       KeywordNot      },
                { "and",       OpAnd           },
                { "or",        OpOr            },
                { "in",        KeywordIn       },
                { "not in",    KeywordNotIn    },
                { "is",        KeywordIs       },
                { "is not",    KeywordIsNot    },
                { "as",        KeywordAs       },
                // testing
                { "assert",    KeywordAssert   },
                // branching
                { "unless",    KeywordUnless   },
                { "if",        KeywordIf       },
                { "elif",      KeywordElseIf   },
                { "else",      KeywordElse     },
                { "match",     KeywordMatch    },
                { "case",      KeywordCase     },
                { "default",   KeywordDefault  },
                // loops
                { "for",       KeywordFor      },
                { "do",        KeywordDo       },
                { "while",     KeywordWhile    },
                { "break",     KeywordBreak    },
                { "nobreak",   KeywordNoBreak  },
                { "continue",  KeywordContinue },
                // exceptions
                { "try",       KeywordTry      },
                { "raise",     KeywordRaise    },
                { "catch",     KeywordCatch    },
                { "anyway",    KeywordAnyway   },
                { "const",     KeywordConst    },
                // asynchronous
                { "async",     KeywordAsync    },
                { "await",     KeywordAwait    },
                // modules
                { "use",       KeywordUse      },
                { "module",    KeywordModule   },
                { "mixin",     KeywordMixin    },
                { "from",      KeywordFrom     },
                // structures
                { "class",     KeywordClass    },
                { "extends",   KeywordExtends  },
                { "struct",    KeywordStruct   },
                { "enum",      KeywordEnum     },
                { "fn",        KeywordFn       },
                // variables
                { "let",       KeywordLet      },
                { "new",       KeywordNew      },
                { "delete",    KeywordDelete   },
                // returns
                { "yield",     KeywordYield    },
                { "return",    KeywordReturn   },
                { "pass",      KeywordPass     },
                // values
                { "nil",       KeywordNil      },
                { "true",      KeywordTrue     },
                { "false",     KeywordFalse    },
                { "with",      KeywordWith     },
                { "when",      KeywordWhen     }

            };
    }
}