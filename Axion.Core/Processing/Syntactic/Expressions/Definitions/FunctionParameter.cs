using System;
using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Definitions {
    /// <summary>
    ///     <c>
    ///         func-parameter:
    ///             ID ':' type ['=' infix-expr]
    ///     </c>
    /// </summary>
    public sealed class FunctionParameter : Expr, IDefinitionExpr, IDecorableExpr {
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
            string?   name         = null,
            TypeName? valueType    = null,
            Expr?     defaultValue = null
        ) : this(null, new NameExpr(name), valueType, defaultValue) { }

        public FunctionParameter(
            Expr?     parent       = null,
            NameExpr? name         = null,
            TypeName? valueType    = null,
            Expr?     defaultValue = null
        ) : base(
            parent
         ?? GetParentFromChildren(name, valueType, defaultValue)
        ) {
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

        /// <summary>
        ///     <c>
        ///         multiple-parameter:
        ///         {named-parameter ","}
        ///         ( "*" [parameter] ("," named-parameter)* ["," "**" parameter]
        ///         | "**" parameter
        ///         | named-parameter[","] )
        ///     </c>
        /// </summary>
        public static NodeList<FunctionParameter> ParseList(
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
}