using System;
using System.Linq;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Axion.Core.Processing.Syntax.Tree.Statements.Interfaces;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class FunctionDefinition : Statement, IAsyncStatement {
        public bool IsLambda { get; }

        public Parameter[] Parameters { get; }

        internal string[] ParameterNames {
            get { return Parameters.Select(val => val.Name).ToArray(); }
        }

        internal int ArgCount {
            get {
                var argCount = 0;
                for (; argCount < Parameters.Length; argCount++) {
                    Parameter p = Parameters[argCount];
                    if (p.IsMap || p.IsList || p.IsKeywordOnly) {
                        break;
                    }
                }
                return argCount;
            }
        }

        internal int KwOnlyArgCount {
            get {
                var kwOnlyArgCount = 0;
                for (int i = ArgCount; i < Parameters.Length; i++, kwOnlyArgCount++) {
                    Parameter p = Parameters[i];
                    if (p.IsMap || p.IsList) {
                        break;
                    }
                }
                return kwOnlyArgCount;
            }
        }

        [JsonProperty]
        public Statement Body { get; set; }

        [JsonProperty]
        public string Name { get; }

        public Expression[] Decorators { get; internal set; }

        public Expression ReturnAnnotation { get; internal set; }

        internal bool IsGeneratorMethod => IsGenerator;

        public bool IsGenerator { get; set; }

        // true if this function can set sys.exc_info(). Only functions with an except block can set that.

        // Called by parser to mark that this function can set sys.exc_info(). 
        // An alternative technique would be to just walk the body after the parse and look for a except block.
        [JsonProperty]
        internal bool CanSetSysExcInfo { get; set; }

        // true if the function contains try/finally, used for generator optimization
        [JsonProperty]
        internal bool ContainsTryFinally { get; set; }

        public FunctionDefinition(string name, Parameter[] parameters, Position start)
            : this(name, parameters, null, start) {
        }

        public FunctionDefinition(string name, Parameter[] parameters, Statement body, Position start) {
            if (parameters == null) {
                throw new ArgumentNullException(nameof(parameters));
            }

//            if (name == null) {
//                Name = "<lambda$" + Interlocked.Increment(ref _lambdaId) + ">";
//                IsLambda = true;
//            }
//            else
            {
                Name = name;
            }

            MarkStart(start);
            Parameters = parameters;
            Body       = body;
        }

        [JsonProperty]
        public bool IsAsync { get; set; }
    }

    public enum ParameterKind {
        Normal,
        List,
        Map,
        KeywordOnly
    }

    public class Parameter : SpannedRegion {
        public string Name { get; }

        public Expression Annotation { get; set; }

        public Expression DefaultValue { get; set; }

        public bool IsList => Kind == ParameterKind.List;

        public bool IsMap => Kind == ParameterKind.Map;

        internal bool IsKeywordOnly => Kind == ParameterKind.KeywordOnly;

        private readonly ParameterKind Kind;

        public Parameter(string name, ParameterKind kind = ParameterKind.Normal) {
            Name = name;
            Kind = kind;
        }

        public Parameter(Token name, ParameterKind kind = ParameterKind.Normal) {
            Name = name.Value;
            Kind = kind;

            MarkPosition(name);
        }
    }
}