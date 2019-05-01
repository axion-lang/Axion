using System;
using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.Binary;
using Axion.Core.Processing.Syntactic.Expressions.Multiple;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Specification {
    public partial class Spec {
        internal static TypeName CharType   => new SimpleTypeName("Char");
        internal static TypeName StringType => new SimpleTypeName("String");
        internal static TypeName UnknownBracesCollectionType => new SimpleTypeName("Map|Set?");
        
        internal static TypeName UnknownListType => ListType(new SimpleTypeName("?"));

        internal static TypeName NumberType(NumberOptions options) {
            var name = "";
            if (options.Unlimited) {
                name = "BigInt";
            }
            else if (options.Imaginary) {
                name = "Complex";
            }
            else {
                if (options.Unsigned) {
                    name += "U";
                }

                name += options.Floating
                    ? "Float"
                    : "Int";
                name += options.Bits;
            }

            return new SimpleTypeName(name);
        }

        internal static TypeName MapType(TypeName itemType) {
            return new GenericTypeName(new SimpleTypeName("Map"), itemType);
        }
        
        internal static TypeName SetType(TypeName itemType) {
            return new GenericTypeName(new SimpleTypeName("Set"), itemType);
        }

        internal static TypeName ListType(TypeName itemType) {
            return new GenericTypeName(new SimpleTypeName("List"), itemType);
        }

        internal static TypeName FuncType(IEnumerable<TypeName> parameterTypes, TypeName returnType) {
            return new GenericTypeName(
                new SimpleTypeName("Func"),
                parameterTypes.Append(returnType)
            );
        }

        /// <summary>
        ///     Types of token, that can start a 'block'.
        /// </summary>
        internal static readonly TokenType[] BlockStarters = {
            Colon, Newline, Indent, OpenBrace
        };

        /// <summary>
        ///     Type of tokens, that are treated as
        ///     compile-time constant values.
        /// </summary>
        internal static readonly TokenType[] Constants = {
            TokenType.String,
            Character,
            Number,
            KeywordTrue,
            KeywordFalse,
            KeywordNil
        };

        internal static readonly TokenType[] NeverExprStartTypes = {
            OpAssign,
            OpPlusAssign,
            OpMinusAssign,
            OpMultiplyAssign,
            OpTrueDivideAssign,
            OpRemainderAssign,
            OpBitAndAssign,
            OpBitOrAssign,
            OpBitXorAssign,
            OpBitLeftShiftAssign,
            OpBitRightShiftAssign,
            OpPowerAssign,
            OpFloorDivideAssign,
            Indent,
            Outdent,
            Newline,
            End,
            Semicolon,
            CloseBrace,
            CloseBracket,
            CloseParenthesis,
            Comma,
            KeywordFor,
            OpIn,
            KeywordIf
        };

        internal static readonly Type[] DecoratorExprs = {
            typeof(NameExpression),
            typeof(FunctionCallExpression)
        };

        internal static readonly Type[] VariableLeftExprs = {
            typeof(SimpleNameExpression), typeof(TupleExpression)
        };

        internal static readonly Type[] AssignableExprs =
            VariableLeftExprs
                .Union(
                    new[] {
                        typeof(MemberAccessExpression),
                        typeof(IndexerExpression)
                    }
                )
                .ToArray();

        internal static readonly Type[] PrimaryExprs =
            AssignableExprs
                .Union(
                    new[] {
                        typeof(AwaitExpression),
                        typeof(YieldExpression),
                        typeof(TypeInitializerExpression),
                        typeof(BraceCollectionExpression),
                        typeof(ListInitializerExpression),
                        typeof(FunctionCallExpression),
                        typeof(ConstantExpression),
                        typeof(ParenthesizedExpression),
                        typeof(LambdaExpression)
                    }
                )
                .ToArray();

        internal static readonly Type[] PreGlobalExprs =
            PrimaryExprs
                .Union(
                    new[] {
                        typeof(UnaryOperationExpression),
                        typeof(BinaryOperationExpression),
                        typeof(ConditionalExpression)
                    }
                )
                .ToArray();

        internal static readonly Type[] GlobalExprs =
            PreGlobalExprs
                .Union(
                    new[] {
                        typeof(VariableDefinitionExpression),
                        typeof(GeneratorExpression),
                        typeof(ForComprehension)
                    }
                )
                .ToArray();

        internal static readonly Type[] StatementExprs = {
            typeof(AwaitExpression),
            typeof(YieldExpression),
            typeof(FunctionCallExpression),
            typeof(VariableDefinitionExpression),
            typeof(UnaryOperationExpression),
            typeof(BinaryOperationExpression)
        };
        
        internal static readonly Dictionary<Type[], string> ExprGroupNames = new Dictionary<Type[], string> {
            { DecoratorExprs,           "decorator" },
            { VariableLeftExprs,        "variable name(s)" },
            { AssignableExprs,          "assignable" },
            { PrimaryExprs,             "primary value" },
            { PreGlobalExprs,           "operator" },
            { GlobalExprs,              "any valid expression" },
            { StatementExprs,           "statement" }
        };
    }
}