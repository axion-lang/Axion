using System;
using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Traversal;
using Axion.Core.Specification;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Definitions {
    /// <summary>
    ///     <c>
    ///         func_parameter:
    ///             ID ':' type ['=' test]
    ///     </c>
    /// </summary>
    public sealed class FunctionParameter : Expr, IDefinitionExpr, IDecoratedExpr {
        private NameExpr name;

        public NameExpr Name {
            get => name;
            set => SetNode(ref name, value);
        }

        private Expr defaultValue;

        public Expr DefaultValue {
            get => defaultValue;
            set => SetNode(ref defaultValue, value);
        }

        public FunctionParameter(
            Expr     parent,
            NameExpr name         = null,
            TypeName valueType    = null,
            Expr     defaultValue = null
        ) : base(parent) {
            Name         = name;
            ValueType    = valueType;
            DefaultValue = defaultValue;
        }

        public FunctionParameter Parse(HashSet<string> names) {
            SetSpan(
                () => {
                    Name = new NameExpr(this).Parse();
                    if (Stream.MaybeEat(Colon)) {
                        ValueType = new TypeName(this).ParseTypeName();
                    }
                    else {
                        LangException.Report(BlameType.ImpossibleToInferType, Name);
                    }

                    if (names.Contains(Name.ToString())) {
                        LangException.Report(BlameType.DuplicatedParameterInFunction, Name);
                    }

                    names.Add(Name.ToString());

                    if (Stream.MaybeEat(OpAssign)) {
                        DefaultValue = InfixExpr.Parse(this);
                    }
                }
            );
            return this;
        }

        public override void ToAxion(CodeWriter c) {
            c.Write(Name);
            if (ValueType != null) {
                c.Write(": ", ValueType);
            }

            if (DefaultValue != null) {
                c.Write(" = ", DefaultValue);
            }
        }

        public override void ToCSharp(CodeWriter c) {
            if (!(Parent is FunctionDef f && f.Name == null)) {
                c.Write(ValueType, " ");
            }

            c.Write(Name);
            if (DefaultValue != null) {
                c.Write(" = ", DefaultValue);
            }
        }

        public override void ToPython(CodeWriter c) {
            c.Write(Name);
            if (ValueType != null) {
                c.Write(": ", ValueType);
            }

            if (DefaultValue != null) {
                c.Write(" = ", DefaultValue);
            }
        }
    }

    /// <summary>
    ///     <c>
    ///         func_def:
    ///             'fn' [name] ['(' [parameters_list] ')'] ['->' type] block;
    ///     </c>
    /// </summary>
    public class FunctionDef : Expr, IDefinitionExpr {
        private NameExpr? name;

        public NameExpr? Name {
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

        private BlockExpr block;

        public BlockExpr Block {
            get => block;
            set => SetNode(ref block, value);
        }

        [NoTraversePath]
        public override TypeName ValueType {
            get {
                if (ReturnType != null) {
                    return ReturnType;
                }

                try {
                    List<(ReturnExpr item, BlockExpr itemParentBlock, int itemIndex)> returns =
                        Block.FindItemsOfType<ReturnExpr>();
                    // TODO: handle all possible returns (type unions)
                    if (returns.Count > 0) {
                        return returns[0].item.ValueType;
                    }

                    return new SimpleTypeName("void");
                }
                catch {
                    return new SimpleTypeName("UNKNOWN_TYPE");
                }
            }
        }

        public FunctionDef(
            Expr                           parent     = null,
            NameExpr                       name       = null,
            IEnumerable<FunctionParameter> parameters = null,
            TypeName                       returnType = null,
            BlockExpr                      block      = null
        ) : base(parent) {
            Name       = name;
            Parameters = NodeList<FunctionParameter>.From(this, parameters);
            ReturnType = returnType;
            Block      = block;
        }

        public FunctionDef Parse(bool anonymous = false) {
            SetSpan(
                () => {
                    Stream.Eat(KeywordFn);
                    if (!anonymous) {
                        Name = new NameExpr(this).Parse();
                    }

                    // parameters
                    if (Stream.MaybeEat(OpenParenthesis)) {
                        Parameters = ParseParameterList(
                            this,
                            CloseParenthesis
                        );
                        Stream.Eat(CloseParenthesis);
                    }
                    else {
                        Parameters = new NodeList<FunctionParameter>(this);
                    }

                    // return type
                    if (Stream.MaybeEat(RightArrow)) {
                        ReturnType = new TypeName(this).ParseTypeName();
                    }

                    if (Stream.PeekIs(Spec.BlockStartMarks)) {
                        Block = new BlockExpr(this).Parse(
                            anonymous
                                ? BlockType.Lambda
                                : BlockType.Default
                        );
                    }
                    else {
                        Block = new BlockExpr(this);
                    }
                }
            );
            return this;
        }

        /// <summary>
        ///     <c>
        ///         parameter_list:
        ///         {named_parameter ","}
        ///         ( "*" [parameter] ("," named_parameter)* ["," "**" parameter]
        ///         | "**" parameter
        ///         | named_parameter[","] )
        ///     </c>
        /// </summary>
        public static NodeList<FunctionParameter> ParseParameterList(
            Expr               parent,
            params TokenType[] terminators
        ) {
            var parameters               = new NodeList<FunctionParameter>(parent);
            var names                    = new HashSet<string>(StringComparer.Ordinal);
            var haveMultiply             = false;
            var haveKeywordOnlyParameter = false;
            // we want these to be the last two parameters
            FunctionParameter listParameter = null;
            FunctionParameter mapParameter  = null;
            var               needDefault   = false;
            while (!parent.Stream.PeekIs(terminators)) {
                if (parent.Stream.MaybeEat(OpPower)) {
                    mapParameter = new FunctionParameter(parent).Parse(names);
                    parent.Stream.Eat(terminators);
                    break;
                }

                if (parent.Stream.MaybeEat(OpMultiply)) {
                    if (haveMultiply) {
                        LangException.Report(
                            BlameType.CannotHaveMoreThan1ListParameter,
                            parent.Stream.Peek
                        );
                        return new NodeList<FunctionParameter>(parent);
                    }

                    if (!parent.Stream.PeekIs(Comma)) {
                        listParameter = new FunctionParameter(parent).Parse(names);
                    }
                    // else got ", *,"

                    haveMultiply = true;
                }
                else {
                    // If a parameter has a default value,
                    // all following parameters up until
                    // the "*" must also have a default value.
                    FunctionParameter param;
                    if (haveMultiply) {
                        param                    = new FunctionParameter(parent).Parse(names);
                        haveKeywordOnlyParameter = true;
                    }
                    else {
                        param = new FunctionParameter(parent).Parse(names);
                    }

                    if (param.DefaultValue != null) {
                        needDefault = true;
                    }
                    else if (needDefault && param.DefaultValue == null) {
                        LangException.Report(BlameType.ExpectedDefaultParameterValue, parent);
                    }

                    parameters.Add(param);
                }

                if (parent.Stream.PeekIs(terminators) || !parent.Stream.MaybeEat(Comma)) {
                    break;
                }
            }

            if (haveMultiply
             && listParameter == null
             && mapParameter  != null
             && !haveKeywordOnlyParameter) {
                LangException.Report(BlameType.NamedArgsMustFollowBareStar, parent.Stream.Token);
            }

            if (listParameter != null) {
                parameters.Add(listParameter);
            }

            if (mapParameter != null) {
                parameters.Add(mapParameter);
            }

            return parameters;
        }

        public override void ToAxion(CodeWriter c) {
            c.Write("fn ");
            if (Name != null) {
                c.Write(Name, " ");
            }

            if (Parameters.Count > 0) {
                c.Write("(");
                c.AddJoin(", ", Parameters);
                c.Write(") ");
            }

            if (ValueType != null) {
                c.Write("-> ", ValueType);
            }

            c.Write(Block);
        }

        public override void ToCSharp(CodeWriter c) {
            if (Name == null) {
                c.Write("(");
                c.AddJoin(", ", Parameters);
                c.Write(") => ", Block);
            }
            else {
                c.Write(
                    "public ", ValueType, " ", Name,
                    "("
                );
                c.AddJoin(", ", Parameters);
                c.WriteLine(")");
                c.Write(Block);
            }
        }

        public override void ToPython(CodeWriter c) {
            c.Write("def ", Name, "(");
            c.AddJoin(", ", Parameters);
            c.Write(")");
            if (ValueType != null) {
                c.Write(" -> ", ValueType);
            }

            c.Write(Block);
        }
    }
}