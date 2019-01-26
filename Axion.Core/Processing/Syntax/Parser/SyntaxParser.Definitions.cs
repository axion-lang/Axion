using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Axion.Core.Processing.Syntax.Tree.Statements;
using Axion.Core.Processing.Syntax.Tree.Statements.Definitions;
using Axion.Core.Processing.Syntax.Tree.Statements.Interfaces;
using Axion.Core.Specification;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntax.Parser {
    public partial class SyntaxParser {
        /// <summary>
        /// <c>
        ///     decorated:
        ///         decorators (class_def | enum_def | func_def)
        /// </c>
        /// </summary>
        private Statement ParseDecorated() {
            List<Expression> decorators = ParseDecorators();

            Statement stmt = ParseStmt(onlyDecorated: true);
            if (stmt is IDecorated def) {
                def.Modifiers = decorators;
            }
            else {
                ReportError(Spec.ERR_InvalidDecoratorPosition, stmt);
            }

            return stmt;
        }

        #region Type name

        /// <summary>
        /// <c>
        ///     type:
        ///         simple_type | generic_type
        ///       | union_type  | tuple_type
        ///     simple_type:
        ///         name
        ///     generic_type:
        ///         name '[' type ']'
        ///     union_type:
        ///         type ('|' type)+
        ///     tuple_type:
        ///         '(' [type (',' type)*] ')'
        /// </c>
        /// </summary>
        private void ValidateTypeName(SpannedRegion span) {
            if (span is TupleExpression tuple) {
                // tuple of types
                for (var i = 0; i < tuple.Expressions.Length; i++) {
                    ValidateTypeName(tuple.Expressions[i]);
                }
            }
            else if (span is BinaryExpression bin
                  && bin.Operator.Type == OpBitwiseOr) {
                // union - type
                ValidateTypeName(bin.Left);
                ValidateTypeName(bin.Right);
            }
            else if (span is IndexExpression index) {
                // generic
                ValidateTypeName(index.Target);
                ValidateTypeName(index.Index);
            }
            else if (span is MemberExpression member) {
                ValidateTypeName(member.Name);
                ValidateTypeName(member.Target);
            }
            else if (span is NameExpression) {
                // type name
            }
            else {
                Blame(BlameType.InvalidTypeNameExpression, span);
            }
        }

        /// <summary>
        ///     @Nullable
        /// </summary>
        private Expression ParseTypeName() {
            int savedIdx = stream.Index;
            if (stream.MaybeEat(Identifier)) {
                bool isTypeName = stream.PeekIs(Spec.TypeNameFollowers);
                stream.MoveTo(savedIdx);
                if (!isTypeName) {
                    return null;
                }
            }
            Expression typeName = ParseTestExpr();
            ValidateTypeName(typeName);
            return typeName;
        }

        private List<(NameExpression, Expression)> ParseTypeArgs(bool allowNamed) {
            var   typeArgs = new List<(NameExpression, Expression)>();
            Token start    = stream.Peek;
            // '(' type (',' type)* ')'
            if (stream.MaybeEat(LeftParenthesis)) {
                if (stream.MaybeEat(RightParenthesis)) {
                    // redundant parens
                    Blame(BlameType.RedundantEmptyListOfTypeArguments, (start.Span.EndPosition, tokenEnd));
                }
                else {
                    do {
                        NameExpression name     = null;
                        int            startIdx = stream.Index;
                        if (stream.MaybeEat(Identifier)) {
                            Token id = stream.Token;
                            if (stream.MaybeEat(Assign)) {
                                if (!allowNamed) {
                                    ReportError("Base of this type cannot be named.", id);
                                }
                                name = new NameExpression(id);
                            }
                            else {
                                stream.MoveTo(startIdx);
                            }
                        }
                        typeArgs.Add((name, ParseTypeName()));
                    } while (stream.MaybeEat(Comma));
                    stream.Eat(RightParenthesis);
                }
            }
            return typeArgs;
        }

        #endregion

        #region Class

        /// <summary>
        /// <c>
        ///     class_def:
        ///         'class' ID ['(' args_list ')'] block
        /// </c>
        /// </summary>
        private ClassDefinition ParseClassDef() {
            Token start = StartNewStmt(KeywordClass);

            NameExpression name = ParseName();
            if (name == null) {
                // no name, assume there's no class.
                return new ClassDefinition(null, new Expression[0], new Expression[0], ErrorStmt());
            }

            Expression                         metaClass = null;
            var                                bases     = new List<Expression>();
            var                                keywords  = new List<Expression>();
            List<(NameExpression, Expression)> types     = ParseTypeArgs(true);
            foreach ((NameExpression name, Expression type) type in types) {
                if (type.name == null) {
                    bases.Add(type.type);
                }
                else {
                    keywords.Add(type.type);
                    if (type.name.Name == "metaclass") {
                        metaClass = type.type;
                    }
                }
            }

            var classDef = new ClassDefinition(name, bases.ToArray(), keywords.ToArray(), metaClass: metaClass);
            ast.PushClass(classDef);

            // Parse class block
            Statement block = ParseTopLevelBlock();

            ClassDefinition ret2 = ast.PopClass();
            Debug.Assert(classDef == ret2);

            classDef.Block = block;
            classDef.MarkPosition(start.Span.StartPosition, tokenEnd);
            return classDef;
        }

        #endregion

        #region Enum

        /// <summary>
        /// <c>
        ///     enum_def:
        ///         'enum' ID ['(' args_list ')'] block_start enum_item* block_terminator
        /// </c>
        /// </summary>
        private EnumDefinition ParseEnumDef() {
            Token start = StartNewStmt(KeywordEnum);

            NameExpression name = ParseName();
            if (name == null || !name.IsSimple) {
                return new EnumDefinition(start.Span.StartPosition, tokenEnd, name);
            }
            Expression[]                                     bases = ParseTypeArgs(false).Select(it => it.Item2).ToArray();
            (TokenType terminator, bool oneLine, bool error) block = ParseBlockStart();
            var                                              items = new List<EnumItem>();
            if (!stream.MaybeEat(KeywordPass) && !block.error) {
                do {
                    items.Add(ParseEnumItem());
                } while (!stream.MaybeEat(block.terminator)
                      && !CheckUnexpectedEOS()
                      && stream.MaybeEat(Comma));
            }
            return new EnumDefinition(start.Span.StartPosition, tokenEnd, name, bases, items.ToArray());
        }

        /// <summary>
        /// <c>
        ///     enum_item:
        ///         ID ['(' type (',' type)* ')'] ['=' constant_expr]
        /// </c>
        /// </summary>
        private EnumItem ParseEnumItem() {
            stream.MaybeEatNewline();
            NameExpression                     name     = ParseName();
            List<(NameExpression, Expression)> typeArgs = ParseTypeArgs(false);
            ConstantExpression                 value    = null;
            if (stream.MaybeEat(Assign)) {
                value = ParsePrimaryExpr() as ConstantExpression;
                if (value == null) {
                    Blame(BlameType.ConstantValueExpected, stream.Token);
                }
            }
            return new EnumItem(name, typeArgs.Select(it => it.Item2).ToArray(), value);
        }

        #endregion

        #region Function

        /// <summary>
        /// <c>
        ///     func_def:
        ///         'fn' [type_name] ID parameters block
        ///     parameters:
        ///         '(' [parameters_list] ')'
        /// </c>
        /// </summary>
        private FunctionDefinition ParseFunctionDef() {
            Position       start      = StartNewStmt(KeywordFn).Span.StartPosition;
            Expression     returnType = ParseTypeName();
            NameExpression funcName   = ParseName();

            stream.Eat(LeftParenthesis);
            Parameter[] parameters = ParseFunctionParameterList(RightParenthesis);

            var ret = new FunctionDefinition(funcName, parameters, returnType);

            ast.PushFunction(ret);
            Statement          block = ParseTopLevelBlock();
            FunctionDefinition ret2  = ast.PopFunction();
            Debug.Assert(ret == ret2);

            ret.Block = block;
            ret.MarkPosition(start, block.Span.EndPosition);
            return ret;
        }

        #region Parameters

        private void CheckUniqueParameter(HashSet<string> names, Token name) {
            if (names.Contains(name.Value)) {
                Blame(BlameType.DuplicatedParameterNameInFunctionDefinition, name);
            }
            names.Add(name.Value);
        }

        /// <summary>
        /// <c>
        ///     parameter_list:
        ///         (named_parameter ",")*
        ///         ( "*" [parameter] ("," named_parameter)* ["," "**" parameter]
        ///         | "**" parameter
        ///         | named_parameter[","] )
        /// </c>
        /// </summary>
        private Parameter[] ParseFunctionParameterList(TokenType terminator) {
            var parameters              = new List<Parameter>();
            var names                   = new HashSet<string>(StringComparer.Ordinal);
            var needDefault             = false;
            var readMultiply            = false;
            var hasKeywordOnlyParameter = false;
            // we want these to be the last two parameters
            Parameter listParameter = null;
            Parameter mapParameter  = null;
            while (!stream.MaybeEat(terminator)) {
                if (stream.MaybeEat(OpPower)) {
                    mapParameter = ParseParameter(names, ParameterKind.Map);
                    if (mapParameter == null) {
                        // no parameter name, syntax error
                        return new Parameter[0];
                    }
                    stream.Eat(terminator);
                    break;
                }

                if (stream.MaybeEat(OpMultiply)) {
                    if (readMultiply) {
                        ReportError("Invalid syntax (ALE-1, description_not_implemented)", stream.Peek);
                        return new Parameter[0];
                    }

                    if (stream.PeekIs(Comma)) {
                        // "*"
                    }
                    else {
                        listParameter = ParseParameter(names, ParameterKind.List);
                        if (listParameter == null) {
                            // no parameter name, syntax error
                            return new Parameter[0];
                        }
                    }

                    readMultiply = true;
                }
                else {
                    // If a parameter has a default value, all following parameters up until the "*" must also have a default value
                    Parameter parameter;
                    if (readMultiply) {
                        var dontCare = false;
                        parameter               = ParseNamedParameter(names, ParameterKind.KeywordOnly, ref dontCare);
                        hasKeywordOnlyParameter = true;
                    }
                    else {
                        parameter = ParseNamedParameter(names, ParameterKind.Normal, ref needDefault);
                    }
                    if (parameter == null) {
                        // no parameter, syntax error
                        return new Parameter[0];
                    }

                    parameters.Add(parameter);
                }

                if (!stream.MaybeEat(Comma)) {
                    stream.Eat(terminator);
                    break;
                }
            }

            if (readMultiply
             && listParameter == null
             && mapParameter != null
             && !hasKeywordOnlyParameter) {
                ReportError("named arguments must follow bare *", stream.Token);
            }

            if (listParameter != null) {
                parameters.Add(listParameter);
            }
            if (mapParameter != null) {
                parameters.Add(mapParameter);
            }

            return parameters.ToArray();
        }

        /// <summary>
        /// <c>
        ///     named_parameter:
        ///         parameter ["=" test]
        /// </c>
        /// </summary>
        private Parameter ParseNamedParameter(HashSet<string> names, ParameterKind parameterKind, ref bool needDefault) {
            Parameter parameter = ParseParameter(names, parameterKind);
            if (stream.MaybeEat(Assign)) {
                needDefault            = true;
                parameter.DefaultValue = ParseTestExpr();
            }
            else if (needDefault) {
                Blame(BlameType.ExpectedDefaultParameterValue, stream.Token);
            }
            return parameter;
        }

        /// <summary>
        /// <c>
        ///     parameter:
        ///         [type] ID [":" test]
        /// </c>
        /// </summary>
        private Parameter ParseParameter(HashSet<string> names, ParameterKind parameterKind) {
            var parameter = new Parameter(ParseTypeName(), ParseName(), parameterKind);
            CheckUniqueParameter(names, parameter.Name.NameParts[0]);
            return parameter;
        }

        #endregion

        #endregion
    }
}