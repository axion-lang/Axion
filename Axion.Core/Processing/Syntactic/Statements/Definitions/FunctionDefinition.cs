using System;
using System.Collections.Generic;
using System.Diagnostics;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Syntactic.Statements.Interfaces;
using JetBrains.Annotations;

namespace Axion.Core.Processing.Syntactic.Statements.Definitions {
    /// <summary>
    ///     <c>
    ///         func_def ::=
    ///             'fn' [name] parameters ['=>' type_name] block
    ///         parameters ::=
    ///             '(' [parameters_list] ')'
    ///     </c>
    /// </summary>
    public class FunctionDefinition : Statement, IDecorated {
        private NameExpression name;

        public NameExpression Name {
            get => name;
            set => SetNode(ref name, value);
        }

        private NameExpression explicitInterfaceName;

        public NameExpression ExplicitInterfaceName {
            get => explicitInterfaceName;
            set => SetNode(ref explicitInterfaceName, value);
        }

        private TypeName returnType;

        public TypeName ReturnType {
            get => returnType;
            set => SetNode(ref returnType, value);
        }

        private NodeList<Parameter> parameters;

        public NodeList<Parameter> Parameters {
            get => parameters;
            set => SetNode(ref parameters, value);
        }

        private BlockStatement block;

        public BlockStatement Block {
            get => block;
            set => SetNode(ref block, value);
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
            string              name,
            NameExpression      explicitInterfaceName = null,
            NodeList<Parameter> parameters            = null,
            BlockStatement      block                 = null,
            TypeName            returnType            = null
        ) {
            Name                  = new NameExpression(name);
            ExplicitInterfaceName = explicitInterfaceName;
            Parameters            = parameters ?? new NodeList<Parameter>(this);

            Block      = block;
            ReturnType = returnType;
        }

        internal FunctionDefinition(SyntaxTreeNode parent) {
            Parent = parent;
            StartNode(TokenType.KeywordFn);

            Name = new NameExpression(this);
            if (Name.Qualifiers.Count > 0
                && Name.Qualifiers.Count < 3) {
                if (Name.Qualifiers.Count == 2) {
                    ExplicitInterfaceName = new NameExpression(Name.Qualifiers[0]);
                    Name                  = new NameExpression(Name.Qualifiers[1]);
                }
            }
            else {
                Unit.ReportError(
                    "Function name must be simple, or declare one explicit interface name.",
                    Name
                );
            }

            // parameters
            if (MaybeEat(TokenType.OpenParenthesis)) {
                Parameters = Parameter.ParseList(this, TokenType.CloseParenthesis);
            }

            // return type
            if (MaybeEat(TokenType.RightFatArrow)) {
                ReturnType = TypeName.Parse(this);
            }

            Ast.PushFunction(this);
            Block = new BlockStatement(this, BlockType.Top);
            FunctionDefinition ret2 = Ast.PopFunction();
            Debug.Assert(this == ret2);

            MarkEnd(Token);
        }

        internal override CodeBuilder ToCSharpCode(CodeBuilder c) {
            c = c + ReturnType + " " + Name + "(";
            c.AppendJoin(",", Parameters);
            return c + ")" + Block;
        }
    }

    public enum ParameterKind {
        Normal,
        List,
        Map,
        KeywordOnly
    }

    /// <summary>
    ///     <c>
    ///         parameter ::=
    ///             ID ':' type
    ///     </c>
    /// </summary>
    public sealed class Parameter : Expression {
        private NameExpression name;

        [NotNull]
        public NameExpression Name {
            get => name;
            set => SetNode(ref name, value);
        }

        private TypeName typeName;

        [NotNull]
        public TypeName TypeName {
            get => typeName;
            set => SetNode(ref typeName, value);
        }

        private Expression defaultValue;

        public Expression DefaultValue {
            get => defaultValue;
            set => SetNode(ref defaultValue, value);
        }

        private readonly ParameterKind Kind;

        public Parameter(
            [NotNull] NameExpression name,
            [NotNull] TypeName       typeName,
            ParameterKind            kind = ParameterKind.Normal
        ) {
            Name     = name;
            TypeName = typeName;
            Kind     = kind;

            MarkPosition(name);
        }

        /// <summary>
        ///     <c>
        ///         named_parameter ::=
        ///             parameter ["=" test]
        ///     </c>
        /// </summary>
        internal Parameter(
            SyntaxTreeNode  parser,
            HashSet<string> names,
            ParameterKind   paramKind,
            ref bool        needDefault
        ) : this(parser, names, paramKind) {
            if (MaybeEat(TokenType.OpAssign)) {
                needDefault  = true;
                DefaultValue = ParseTestExpr(parser);
            }
            else if (needDefault) {
                Unit.Blame(BlameType.ExpectedDefaultParameterValue, Token);
            }

            MarkEnd(Token);
        }

        internal Parameter(SyntaxTreeNode parent, HashSet<string> names, ParameterKind paramKind) {
            Parent = parent;
            Kind   = paramKind;

            MarkStart(Token);

            Name = new NameExpression(this, true);
            Eat(TokenType.Colon);
            TypeName = TypeName.Parse(this);

            if (names.Contains(Name.Qualifiers[0].Value)) {
                Unit.Blame(BlameType.DuplicatedParameterNameInFunctionDefinition, name);
            }

            names.Add(Name.Qualifiers[0].Value);

            MarkEnd(Token);
        }

        /// <summary>
        ///     <c>
        ///         parameter_list ::=
        ///             {named_parameter ","}
        ///             ( "*" [parameter] ("," named_parameter)* ["," "**" parameter]
        ///             | "**" parameter
        ///             | named_parameter[","] )
        ///     </c>
        /// </summary>
        internal static NodeList<Parameter> ParseList(SyntaxTreeNode parent, TokenType terminator) {
            var parameters               = new NodeList<Parameter>(parent);
            var names                    = new HashSet<string>(StringComparer.Ordinal);
            var needDefault              = false;
            var haveMultiply             = false;
            var haveKeywordOnlyParameter = false;
            // we want these to be the last two parameters
            Parameter listParameter = null;
            Parameter mapParameter  = null;
            while (!parent.MaybeEat(terminator)) {
                if (parent.MaybeEat(TokenType.OpPower)) {
                    mapParameter = new Parameter(parent, names, ParameterKind.Map);
                    parent.Eat(terminator);
                    break;
                }

                if (parent.MaybeEat(TokenType.OpMultiply)) {
                    if (haveMultiply) {
                        parent.Unit.ReportError(
                            "Cannot have more than 1 list parameter.",
                            parent.Peek
                        );
                        return null;
                    }

                    if (parent.PeekIs(TokenType.Comma)) {
                        // "*"
                    }
                    else {
                        listParameter = new Parameter(parent, names, ParameterKind.List);
                    }

                    haveMultiply = true;
                }
                else {
                    // If a parameter has a default value,
                    // all following parameters up until
                    // the "*" must also have a default value.
                    Parameter parameter;
                    if (haveMultiply) {
                        var _ = false;
                        parameter = new Parameter(
                            parent,
                            names,
                            ParameterKind.KeywordOnly,
                            ref _
                        );
                        haveKeywordOnlyParameter = true;
                    }
                    else {
                        parameter = new Parameter(
                            parent,
                            names,
                            ParameterKind.Normal,
                            ref needDefault
                        );
                    }

                    parameters.Add(parameter);
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

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            c = c + Name + ": " + TypeName;
            if (DefaultValue != null) {
                c = c + " = " + DefaultValue;
            }

            return c;
        }

        internal override CodeBuilder ToCSharpCode(CodeBuilder c) {
            c = c + TypeName + " " + Name;
            if (DefaultValue != null) {
                c = c + "=" + DefaultValue;
            }

            return c;
        }
    }
}