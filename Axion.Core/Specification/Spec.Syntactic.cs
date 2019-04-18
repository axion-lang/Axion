using System;
using System.Linq;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.Binary;
using Axion.Core.Processing.Syntactic.Expressions.Multiple;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Specification {
    public partial class Spec {
        internal static TypeName MapType() {
            throw new NotImplementedException();
        }

        internal static TypeName SetType() {
            throw new NotImplementedException();
        }

        internal static TypeName ListType(TypeName itemType) {
            throw new NotImplementedException();
        }

        internal static TypeName TupleType() {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Types of token, that can start a 'block'.
        /// </summary>
        internal static readonly TokenType[] BlockStarters = {
            Colon, Indent, OpenBrace
        };

        /// <summary>
        ///     Type of tokens, that are treated as
        ///     compile-time constant values.
        /// </summary>
        internal static readonly TokenType[] Literals = {
            TokenType.String,
            Character,
            Number,
            KeywordTrue,
            KeywordFalse,
            KeywordNil
        };

        internal static readonly TokenType[] NeverTestTypes = {
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
            KeywordIn,
            KeywordIf
        };

        internal static readonly Type[] DeletableExprs = {
            typeof(NameExpression), typeof(MemberAccessExpression), typeof(IndexerExpression)
        };

        internal static readonly Type[] AssignableExprs = {
            typeof(NameExpression),
            typeof(TupleExpression),
            typeof(MemberAccessExpression),
            typeof(IndexerExpression)
        };

        internal static readonly Type[] PrimaryExprs = AssignableExprs.Union(
            new[] {
              typeof(
                  AwaitExpression),
              typeof(
                  YieldExpression),
              typeof(
                  TypeInitializerExpression
              ),
              typeof(
                  HashCollectionExpression
              ),
              typeof(
                  ListInitializerExpression
              ),
              typeof(
                  FunctionCallExpression
              ),
              typeof(
                  ConstantExpression
              )
            }
            )
            .ToArray();

        internal static readonly Type[] TestExprs = PrimaryExprs.Union(
            new[] {
                typeof(
                    FunctionCallExpression
                ),
                typeof(IndexerExpression),
                typeof(
                    UnaryOperationExpression
                ),
                typeof(
                    BinaryOperationExpression
                ),
                typeof(
                    ConditionalExpression),
                typeof(ForComprehension)
            }
            )
            .ToArray();

        internal static readonly Type[] AllExprs = TestExprs.Union(
                new[] {
                    typeof(
                        VariableDefinitionExpression
                    )
                }
            )
            .ToArray();
    }
}