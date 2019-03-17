using System;
using System.Collections.Generic;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Axion.Core.Processing.Syntax.Tree.Expressions.TypeNames;
using Axion.Core.Processing.Syntax.Tree.Statements.Interfaces;

namespace Axion.Core.Processing.Syntax.Tree.Statements.Definitions {
    public class FunctionDefinition : Statement, IDecorated {
        public  string         Name { get; set; }
        private NameExpression explicitInterfaceName;

        public NameExpression ExplicitInterfaceName {
            get => explicitInterfaceName;
            set {
                if (value != null) {
                    value.Parent = this;
                }

                explicitInterfaceName = value;
            }
        }

        private TypeName returnType;

        public TypeName ReturnType {
            get => returnType;
            set {
                if (value != null) {
                    value.Parent = this;
                }

                returnType = value;
            }
        }

        private Parameter[] parameters;

        public Parameter[] Parameters {
            get => parameters;
            set {
                parameters = value;
                foreach (Parameter expr in parameters) {
                    expr.Parent = this;
                }
            }
        }

        private BlockStatement block;

        public BlockStatement Block {
            get => block;
            set {
                if (value != null) {
                    value.Parent = this;
                }

                block = value;
            }
        }

        public bool IsGenerator { get; set; }

        // true if this function can set sys.exc_info(). Only functions with an except block can set that.

        // Called by parser to mark that this function can set sys.exc_info(). 
        // An alternative technique would be to just walk the body after the parse and look for a except block.
        public bool CanSetSysExcInfo { get; set; }

        // true if the function contains try/finally, used for generator optimization
        public bool             ContainsTryFinally { get; set; }
        public List<Expression> Modifiers          { get; set; }

        public FunctionDefinition(
            string         name,
            NameExpression explicitInterfaceName = null,
            Parameter[]    parameters            = null,
            BlockStatement block                 = null,
            TypeName       returnType            = null
        ) {
            Name                  = name;
            ExplicitInterfaceName = explicitInterfaceName;
            Parameters            = parameters ?? new Parameter[0];
            Block                 = block;
            ReturnType            = returnType;
        }

        internal override CSharpCodeBuilder ToCSharpCode(CSharpCodeBuilder c) {
            c = c + ReturnType + " " + Name + "(";
            c.AppendJoin(", ", Parameters);
            return c + ") " + Block;
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

        public NameExpression Name {
            get => name;
            set {
                value.Parent = this;
                name         = value;
            }
        }

        private TypeName typeName;

        public TypeName TypeName {
            get => typeName;
            set {
                value.Parent = this;
                typeName     = value;
            }
        }

        private Expression defaultValue;

        public Expression DefaultValue {
            get => defaultValue;
            set {
                value.Parent = this;
                defaultValue = value;
            }
        }

        private readonly ParameterKind Kind;

        public Parameter(
            NameExpression name,
            TypeName       typeName,
            ParameterKind  kind = ParameterKind.Normal
        ) {
            Name     = name ?? throw new ArgumentNullException(nameof(name));
            TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
            Kind     = kind;

            MarkPosition(name);
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            // BUG
            return c + TypeName + " " + Name + (DefaultValue != null ? " = " + DefaultValue : "");
        }
    }
}