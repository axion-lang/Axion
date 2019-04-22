using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Syntactic.Statements.Interfaces;
using Axion.Core.Specification;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Axion.Core.Processing.Syntactic.Statements.Definitions {
    /// <summary>
    ///     <c>
    ///         func_def:
    ///             'fn' [ID '.'] ID ['(' [parameters_list] ')'] ['=>' type_name] block
    ///     </c>
    /// </summary>
    public class FunctionDefinition : Statement, IDecorated {
        #region Properties

        private NameExpression name;

        public NameExpression Name {
            get => name;
            set => SetNode(ref name, value);
        }

        private TypeName returnType;

        public TypeName ReturnType {
            get => returnType;
            set => SetNode(ref returnType, value);
        }

        private NodeList<FunctionParameter> parameters;

        public NodeList<FunctionParameter> Parameters {
            get => parameters;
            set => SetNode(ref parameters, value);
        }

        private BlockStatement block;

        public BlockStatement Block {
            get => block;
            set => SetNode(ref block, value);
        }

        private NodeList<Expression> modifiers;

        public NodeList<Expression> Modifiers {
            get => modifiers ??= new NodeList<Expression>(this);
            set {
                if (value != null) {
                    modifiers = value;
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        ///     Constructs new <see cref="FunctionDefinition"/> from Axion tokens.
        /// </summary>
        internal FunctionDefinition(SyntaxTreeNode parent) : base(parent) {
            MarkStart(TokenType.KeywordFn);
            Name = NameExpression.ParseName(this);
            // parameters
            if (MaybeEat(TokenType.OpenParenthesis)) {
                Parameters = ParseParameterList(this, TokenType.CloseParenthesis);
            }

            // return type
            if (MaybeEat(TokenType.RightFatArrow)) {
                ReturnType = TypeName.ParseTypeName(this);
            }

            Ast.PushFunction(this);
            Block = new BlockStatement(this, BlockType.Top);
            MarkEnd(Token);
            Contract.Assert(this == Ast.PopFunction());
            ParentBlock.Functions.Add(this);
        }

        /// <summary>
        ///     Constructs new <see cref="FunctionDefinition"/> from C# syntax.
        /// </summary>
        internal FunctionDefinition(SyntaxTreeNode parent, MethodDeclarationSyntax csNode) : base(
            parent
        ) {
            Name = NameExpression.ParseName(this, csNode.Identifier.Text);
            Parameters = new NodeList<FunctionParameter>(
                this,
                csNode.ParameterList.Parameters.Select(p => new FunctionParameter(this, p))
            );
            ReturnType = TypeName.FromCSharp(this, csNode.ReturnType);
            Block      = new BlockStatement(this, csNode.Body);
            ParentBlock.Functions.Add(this);
        }

        /// <summary>
        ///     Constructs plain <see cref="FunctionDefinition"/> without position in source.
        /// </summary>
        public FunctionDefinition(
            NameExpression               name,
            NodeList<FunctionParameter>? parameters = null,
            BlockStatement?              block      = null,
            TypeName?                    returnType = null
        ) {
            Name       = name;
            Parameters = parameters ?? new NodeList<FunctionParameter>(this);

            Block      = block ?? new BlockStatement(this);
            ReturnType = returnType;
        }

        #endregion

        /// <summary>
        ///     <c>
        ///         parameter_list:
        ///             {named_parameter ","}
        ///             ( "*" [parameter] ("," named_parameter)* ["," "**" parameter]
        ///             | "**" parameter
        ///             | named_parameter[","] )
        ///     </c>
        /// </summary>
        private static NodeList<FunctionParameter> ParseParameterList(
            SyntaxTreeNode parent,
            TokenType      terminator
        ) {
            var parameters               = new NodeList<FunctionParameter>(parent);
            var names                    = new HashSet<string>(StringComparer.Ordinal);
            var haveMultiply             = false;
            var haveKeywordOnlyParameter = false;
            // we want these to be the last two parameters
            FunctionParameter? listParameter = null;
            FunctionParameter? mapParameter  = null;
            var                needDefault   = false;
            while (!parent.MaybeEat(terminator)) {
                if (parent.MaybeEat(TokenType.OpPower)) {
                    mapParameter = new FunctionParameter(parent, names);
                    parent.Eat(terminator);
                    break;
                }

                if (parent.MaybeEat(TokenType.OpMultiply)) {
                    if (haveMultiply) {
                        parent.Unit.ReportError(
                            "Cannot have more than 1 list parameter.",
                            parent.Peek
                        );
                        return new NodeList<FunctionParameter>(parent);
                    }

                    if (parent.Peek.Is(TokenType.Comma)) {
                        // "*"
                    }
                    else {
                        listParameter = new FunctionParameter(parent, names);
                    }

                    haveMultiply = true;
                }
                else {
                    // If a parameter has a default value,
                    // all following parameters up until
                    // the "*" must also have a default value.
                    FunctionParameter param;
                    if (haveMultiply) {
                        param = new FunctionParameter(
                            parent,
                            names
                        );
                        haveKeywordOnlyParameter = true;
                    }
                    else {
                        param = new FunctionParameter(
                            parent,
                            names
                        );
                    }

                    if (param.DefaultValue != null) {
                        needDefault = true;
                    }
                    else if (needDefault && param.DefaultValue == null) {
                        parent.Unit.Blame(BlameType.ExpectedDefaultParameterValue, parent);
                    }

                    parameters.Add(param);
                }

                if (parent.MaybeEat(TokenType.Comma)) {
                    continue;
                }

                parent.Eat(terminator);
                break;
            }

            if (haveMultiply
                && listParameter == null
                && mapParameter != null
                && !haveKeywordOnlyParameter) {
                parent.Unit.ReportError("named arguments must follow bare *", parent.Token);
            }

            if (listParameter != null) {
                parameters.Add(listParameter);
            }

            if (mapParameter != null) {
                parameters.Add(mapParameter);
            }

            return parameters;
        }

        #region Code converters

        public override void ToAxionCode(CodeBuilder c) {
            c.Write("fn " + Name);
            if (Parameters.Count > 0) {
                c.Write(" (");
                c.AddJoin(", ", Parameters);
                c.Write(")");
            }

            if (ReturnType != null) {
                c.Write(" => ", ReturnType);
            }

            c.Write(" ", Block);
        }

        public override void ToCSharpCode(CodeBuilder c) {
            c.Write("public ", ReturnType, " ", Name, "(");
            c.AddJoin(", ", Parameters);
            c.Write(") ", Block);
        }

        #endregion
    }

    /// <summary>
    ///     <c>
    ///         parameter:
    ///             ID ':' type ["=" test]
    ///     </c>
    /// </summary>
    public sealed class FunctionParameter : Expression {
        #region Properties

        private SimpleNameExpression name;

        public SimpleNameExpression Name {
            get => name;
            set => SetNode(ref name, value);
        }

        private TypeName type;

        public TypeName Type {
            get => type;
            set => SetNode(ref type, value);
        }

        private Expression? defaultValue;

        public Expression? DefaultValue {
            get => defaultValue;
            set => SetNode(ref defaultValue, value);
        }

        #endregion

        #region Constructors

        /// <summary>
        ///     Constructs new <see cref="FunctionParameter"/> from Axion tokens.
        /// </summary>
        internal FunctionParameter(
            SyntaxTreeNode  parent,
            HashSet<string> names
        ) : base(parent) {
            MarkStart(Token);
            Name = new SimpleNameExpression(this);
            Eat(TokenType.Colon);
            Type = TypeName.ParseTypeName(this);

            if (names.Contains(Name.Name)) {
                Unit.Blame(BlameType.DuplicatedParameterNameInFunctionDefinition, name);
            }

            names.Add(Name.Name);

            if (MaybeEat(TokenType.OpAssign)) {
                DefaultValue = ParseTestExpr(this);
            }

            MarkEnd(Token);
        }

        /// <summary>
        ///     Constructs new <see cref="FunctionParameter"/> from C# syntax.
        /// </summary>
        internal FunctionParameter(
            SyntaxTreeNode  parent,
            ParameterSyntax csNode
        ) : base(parent) {
            Name = new SimpleNameExpression(this, csNode.Identifier.Text);
            Type = TypeName.FromCSharp(this, csNode.Type);
            throw new NotImplementedException();
            //DefaultValue = Expression.FromCSharp(csNode.Default.Value);
        }

        #endregion

        #region Code converters

        public override void ToAxionCode(CodeBuilder c) {
            c.Write(Name, ": ", Type);
            if (DefaultValue != null) {
                c.Write(" = ", DefaultValue);
            }
        }

        public override void ToCSharpCode(CodeBuilder c) {
            c.Write(Type, " ", Name);
            if (DefaultValue != null) {
                c.Write(" = ", DefaultValue);
            }
        }

        #endregion
    }
}