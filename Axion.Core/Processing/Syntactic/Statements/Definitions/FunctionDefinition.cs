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
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Statements.Definitions {
    /// <summary>
    ///     <c>
    ///         func_def:
    ///             'fn' [ID '.'] ID ['(' [parameters_list] ')'] ['=>' type] block
    ///     </c>
    /// </summary>
    public class FunctionDefinition : Statement, IDecorated, IFunctionNode {
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

        public override TypeName ValueType =>
            Spec.FuncType(Parameters.Select(p => p.ValueType), ReturnType);

        /// <summary>
        ///     Constructs from Axion tokens.
        /// </summary>
        internal FunctionDefinition(SyntaxTreeNode parent) : base(parent) {
            EatStartMark(KeywordFn);
            Name = NameExpression.ParseName(this);
            // parameters
            if (MaybeEat(OpenParenthesis)) {
                Parameters = ParseParameterList(this, CloseParenthesis);
                Eat(CloseParenthesis);
            }

            // return type
            if (MaybeEat(RightFatArrow)) {
                ReturnType = TypeName.ParseTypeName(this);
            }

            Ast.PushFunction(this);
            Block = new BlockStatement(this, BlockType.Top);
            MarkEnd(Token);
            Contract.Assert(this == Ast.PopFunction());
            ParentBlock.Functions.Add(this);
        }

        /// <summary>
        ///     Constructs from C# syntax.
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
        ///     Constructs without position in source.
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

        /// <summary>
        ///     <c>
        ///         parameter_list:
        ///             {named_parameter ","}
        ///             ( "*" [parameter] ("," named_parameter)* ["," "**" parameter]
        ///             | "**" parameter
        ///             | named_parameter[","] )
        ///     </c>
        /// </summary>
        internal static NodeList<FunctionParameter> ParseParameterList(
            SyntaxTreeNode     parent,
            params TokenType[] terminator
        ) {
            var parameters               = new NodeList<FunctionParameter>(parent);
            var names                    = new HashSet<string>(StringComparer.Ordinal);
            var haveMultiply             = false;
            var haveKeywordOnlyParameter = false;
            // we want these to be the last two parameters
            FunctionParameter? listParameter = null;
            FunctionParameter? mapParameter  = null;
            var                needDefault   = false;
            if (!parent.Peek.Is(terminator)) {
                while (true) {
                    if (parent.MaybeEat(OpPower)) {
                        mapParameter = new FunctionParameter(parent, names);
                        parent.Eat(terminator);
                        break;
                    }

                    if (parent.MaybeEat(OpMultiply)) {
                        if (haveMultiply) {
                            parent.Unit.Blame(
                                BlameType.CannotHaveMoreThan1ListParameter,
                                parent.Peek
                            );
                            return new NodeList<FunctionParameter>(parent);
                        }

                        if (parent.Peek.Is(Comma)) {
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

                    if (parent.Peek.Is(terminator)) {
                        break;
                    }

                    parent.Eat(Comma);
                }
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

        internal override void ToAxionCode(CodeBuilder c) {
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

        internal override void ToCSharpCode(CodeBuilder c) {
            bool haveAccessMod = c.WriteDecorators(Modifiers);
            if (!haveAccessMod) {
                c.Write("public ");
            }

            c.Write(ReturnType, " ", Name, "(");
            c.AddJoin(", ", Parameters);
            c.Write(") ", Block);
        }
    }

    /// <summary>
    ///     <c>
    ///         parameter:
    ///             ID ':' type ["=" test]
    ///     </c>
    /// </summary>
    public sealed class FunctionParameter : Expression {
        private SimpleNameExpression name;

        public SimpleNameExpression Name {
            get => name;
            set => SetNode(ref name, value);
        }

        private TypeName type;

        public override TypeName ValueType {
            get => type;
            set => SetNode(ref type, value);
        }

        private Expression? defaultValue;

        public Expression? DefaultValue {
            get => defaultValue;
            set => SetNode(ref defaultValue, value);
        }

        /// <summary>
        ///     Constructs from Axion tokens.
        /// </summary>
        internal FunctionParameter(
            SyntaxTreeNode  parent,
            HashSet<string> names
        ) : base(parent) {
            MarkStart(Token);
            Name = new SimpleNameExpression(this);
            Eat(Colon);
            ValueType = TypeName.ParseTypeName(this);

            if (names.Contains(Name.Name)) {
                Unit.Blame(BlameType.DuplicatedParameterNameInFunctionDefinition, name);
            }

            names.Add(Name.Name);

            if (MaybeEat(OpAssign)) {
                DefaultValue = ParsePreGlobalExpr(this);
            }

            MarkEnd(Token);
        }

        /// <summary>
        ///     Constructs from C# syntax.
        /// </summary>
        internal FunctionParameter(
            SyntaxTreeNode  parent,
            ParameterSyntax csNode
        ) : base(parent) {
            Name      = new SimpleNameExpression(this, csNode.Identifier.Text);
            ValueType = TypeName.FromCSharp(this, csNode.Type);
            throw new NotImplementedException();
            //DefaultValue = Expression.FromCSharp(csNode.Default.Value);
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(Name, ": ", ValueType);
            if (DefaultValue != null) {
                c.Write(" = ", DefaultValue);
            }
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write(ValueType, " ", Name);
            if (DefaultValue != null) {
                c.Write(" = ", DefaultValue);
            }
        }
    }
}