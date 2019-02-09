using System;
using System.Collections.Generic;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Axion.Core.Processing.Syntax.Tree.Expressions.TypeNames;
using Axion.Core.Processing.Syntax.Tree.Statements.Interfaces;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements.Definitions {
    public class FunctionDefinition : Statement, IDecorated {
        private Expression name;

        private TypeName returnType;

        private Parameter[] parameters;

        private Statement block;

        public FunctionDefinition(Expression name, Parameter[] parameters, TypeName returnType = null) : this(
            name,
            parameters,
            null,
            returnType
        ) {
        }

        public FunctionDefinition(Expression name, Parameter[] parameters, Statement body, TypeName returnType = null) {
            Name       = name;
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            Block      = body;
            ReturnType = returnType;
        }

        public bool IsGenerator { get; set; }

        [JsonProperty]
        internal Expression Name {
            get => name;
            set {
                value.Parent = this;
                name         = value;
            }
        }

        [JsonProperty]
        internal TypeName ReturnType {
            get => returnType;
            set {
                if (value != null) {
                    value.Parent = this;
                }
                returnType = value;
            }
        }

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

        // true if this function can set sys.exc_info(). Only functions with an except block can set that.

        // Called by parser to mark that this function can set sys.exc_info(). 
        // An alternative technique would be to just walk the body after the parse and look for a except block.
        [JsonProperty]
        internal bool CanSetSysExcInfo { get; set; }

        // true if the function contains try/finally, used for generator optimization
        [JsonProperty]
        internal bool ContainsTryFinally { get; set; }

        public List<Expression> Modifiers { get; set; }
    }

    public enum ParameterKind {
        Normal,
        List,
        Map,
        KeywordOnly
    }

    public class Parameter : Expression {
        private NameExpression name;

        private TypeName typeName;

        private Expression defaultValue;

        private readonly ParameterKind Kind;

        public Parameter(NameExpression name, TypeName typeName, ParameterKind kind = ParameterKind.Normal) {
            Name     = name;
            TypeName = typeName;
            Kind     = kind;

            MarkPosition(name);
        }

        [JsonProperty]
        internal NameExpression Name {
            get => name;
            set {
                value.Parent = this;
                name         = value;
            }
        }

        [JsonProperty]
        internal TypeName TypeName {
            get => typeName;
            set {
                if (value != null) {
                    value.Parent = this;
                }
                typeName = value;
            }
        }

        [JsonProperty]
        internal Expression DefaultValue {
            get => defaultValue;
            set {
                value.Parent = this;
                defaultValue = value;
            }
        }

        public override string ToString() {
            return ToAxionCode();
        }

        private string ToAxionCode() {
            return TypeName + " " + Name + (DefaultValue != null ? " = " + DefaultValue : "");
        }
    }
}