using System;
using System.Collections.Generic;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Axion.Core.Processing.Syntax.Tree.Statements.Interfaces;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements.Definitions {
    public class FunctionDefinition : Statement, IDecorated {
        private Expression name;

        [JsonProperty]
        internal Expression Name {
            get => name;
            set {
                value.Parent = this;
                name         = value;
            }
        }

        private Expression returnType;

        [JsonProperty]
        internal Expression ReturnType {
            get => returnType;
            set {
                if (value != null) {
                    value.Parent = this;
                }
                returnType = value;
            }
        }

        private Parameter[] parameters;

        [JsonProperty]
        internal Parameter[] Parameters {
            get => parameters;
            set {
                parameters = value;
                foreach (Parameter expr in parameters) {
                    expr.Parent = this;
                }
            }
        }

        private Statement block;

        [JsonProperty]
        internal Statement Block {
            get => block;
            set {
                if (value != null) {
                    value.Parent = this;
                }
                block = value;
            }
        }

        public List<Expression> Modifiers { get; set; }

        public bool IsGenerator { get; set; }

        // true if this function can set sys.exc_info(). Only functions with an except block can set that.

        // Called by parser to mark that this function can set sys.exc_info(). 
        // An alternative technique would be to just walk the body after the parse and look for a except block.
        [JsonProperty]
        internal bool CanSetSysExcInfo { get; set; }

        // true if the function contains try/finally, used for generator optimization
        [JsonProperty]
        internal bool ContainsTryFinally { get; set; }

        public FunctionDefinition(Expression name, Parameter[] parameters, Expression returnType = null)
            : this(name, parameters, null, returnType) {
        }

        public FunctionDefinition(Expression name, Parameter[] parameters, Statement body, Expression returnType = null) {
            Name       = name;
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            Block       = body;
            ReturnType = returnType;
        }
    }

    public enum ParameterKind {
        Normal,
        List,
        Map,
        KeywordOnly
    }

    public class Parameter : Expression {
        private NameExpression name;

        [JsonProperty]
        internal NameExpression Name {
            get => name;
            set {
                value.Parent = this;
                name         = value;
            }
        }

        private Expression typeName;

        [JsonProperty]
        internal Expression TypeName {
            get => typeName;
            set {
                if (value != null) {
                    value.Parent = this;
                }
                typeName = value;
            }
        }

        private Expression defaultValue;

        [JsonProperty]
        internal Expression DefaultValue {
            get => defaultValue;
            set {
                value.Parent = this;
                defaultValue = value;
            }
        }

        private readonly ParameterKind Kind;

        public Parameter(Expression typeName, NameExpression name, ParameterKind kind = ParameterKind.Normal) {
            TypeName = typeName;
            Name     = name;
            Kind     = kind;

            MarkPosition(name);
        }

        public override string ToString() {
            return ToAxionCode();
        }

        private string ToAxionCode() {
            return TypeName + " " + Name + (DefaultValue != null ? " = " + DefaultValue : "");
        }
    }
}