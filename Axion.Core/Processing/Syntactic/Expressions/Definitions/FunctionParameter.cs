using System;
using System.Collections.Generic;
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
    public sealed class FunctionParameter : NameDef {
        public FunctionParameter(Node parent) : base(parent) { }

        public FunctionParameter Parse(HashSet<string> names) {
            SetSpan(
                () => {
                    Name = new NameExpr(this).Parse();
                    if (Stream.MaybeEat(Colon)) {
                        ValueType = TypeName.Parse(this);
                    }
                    else {
                        LangException.Report(
                            BlameType.ImpossibleToInferType,
                            Name
                        );
                    }

                    if (names.Contains(Name.ToString())) {
                        LangException.Report(
                            BlameType.DuplicatedParameterInFunction,
                            Name
                        );
                    }

                    names.Add(Name.ToString());

                    if (Stream.MaybeEat(OpAssign)) {
                        Value = InfixExpr.Parse(this);
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
            Node               parent,
            params TokenType[] terminators
        ) {
            var s = parent.Unit.TokenStream;

            var parameters = new NodeList<FunctionParameter>(parent);
            var names = new HashSet<string>(StringComparer.Ordinal);
            var haveMultiply = false;
            var haveKeywordOnlyParameter = false;
            // we want these to be the last two parameters
            FunctionParameter? listParameter = null;
            FunctionParameter? mapParameter  = null;
            var                needDefault   = false;
            while (!s.PeekIs(terminators)) {
                if (s.MaybeEat(OpPower)) {
                    mapParameter = new FunctionParameter(parent).Parse(names);
                    s.Eat(terminators);
                    break;
                }

                if (s.MaybeEat(OpMultiply)) {
                    if (haveMultiply) {
                        LangException.Report(
                            BlameType.CannotHaveMoreThan1ListParameter,
                            s.Peek
                        );
                        return new NodeList<FunctionParameter>(parent);
                    }

                    if (!s.PeekIs(Comma)) {
                        listParameter =
                            new FunctionParameter(parent).Parse(names);
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
                        param = new FunctionParameter(parent).Parse(names);
                        haveKeywordOnlyParameter = true;
                    }
                    else {
                        param = new FunctionParameter(parent).Parse(names);
                    }

                    if (param.Value != null) {
                        needDefault = true;
                    }
                    else if (needDefault) {
                        LangException.Report(
                            BlameType.ExpectedDefaultParameterValue,
                            parent
                        );
                    }

                    parameters.Add(param);
                }

                if (s.PeekIs(terminators) || !s.MaybeEat(Comma)) {
                    break;
                }
            }

            if (haveMultiply
             && listParameter == null
             && mapParameter  != null
             && !haveKeywordOnlyParameter) {
                LangException.Report(
                    BlameType.NamedArgsMustFollowBareStar,
                    s.Token
                );
            }

            if (listParameter != null) {
                parameters.Add(listParameter);
            }

            if (mapParameter != null) {
                parameters.Add(mapParameter);
            }

            return parameters;
        }
    }
}
