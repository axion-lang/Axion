using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Axion.Core.Processing.Syntax.Tree.Expressions.TypeNames;
using Axion.Core.Processing.Syntax.Tree.Statements;
using Axion.Core.Processing.Syntax.Tree.Statements.Definitions;
using Axion.Core.Processing.Syntax.Tree.Statements.Interfaces;
using Axion.Core.Specification;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntax.Parser {
    public partial class SyntaxParser {
        /// <summary>
        ///     <c>
        ///         decorated ::=
        ///             decorators (decorated_stmt)
        ///     </c>
        /// </summary>
        private Statement ParseDecorated() {
            List<Expression> decorators = ParseDecorators();

            Statement stmt = ParseStmt(true);
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
        ///     <c>
        ///         type ::=
        ///             (simple_type | tuple_type) [trail_type]
        ///         trail_type ::=
        ///             generic_type | union_type | array_type
        ///         simple_type ::=
        ///             name
        ///         generic_type ::=
        ///             name '&lt;' type {',' type} '&gt;'
        ///         union_type ::=
        ///             type ('|' type)+
        ///         tuple_type ::=
        ///             '(' [type {',' type}] ')'
        ///         array_type ::=
        ///             type '[' ']'
        ///     </c>
        /// </summary>
        private TypeName ParseTypeName() {
            TypeName leftTypeName = null;
            // simple
            if (Stream.PeekIs(Identifier)) {
                leftTypeName = new SimpleTypeName(ParseQualifiedName());
            }
            // tuples
            else if (Stream.MaybeEat(LeftParenthesis)) {
                var types = new List<TypeName>();
                if (!Stream.MaybeEat(RightParenthesis)) {
                    while (true) {
                        types.Add(ParseTypeName());
                        if (!Stream.MaybeEat(Comma)) {
                            Stream.Eat(RightParenthesis);
                            break;
                        }
                    }
                }

                if (types.Count == 1) {
                    leftTypeName = types[0];
                }
                else {
                    leftTypeName = new TupleTypeName(types.ToArray());
                }
            }

            // trailing
            if (leftTypeName != null) {
                // generic
                if (Stream.MaybeEat(OpLessThan)) {
                    var generics = new List<TypeName>();
                    do {
                        generics.Add(ParseTypeName());
                    } while (Stream.MaybeEat(Comma));

                    Stream.Eat(OpGreaterThan);
                    leftTypeName = new GenericTypeName(leftTypeName, generics.ToArray());
                }

                // array
                if (Stream.MaybeEat(LeftBracket)) {
                    Stream.Eat(RightBracket);
                    leftTypeName = new ArrayTypeName(leftTypeName);
                }

                // union
                if (Stream.MaybeEat(OpBitwiseOr)) {
                    TypeName right = ParseTypeName();
                    leftTypeName = new UnionTypeName(leftTypeName, right);
                }
            }

            return leftTypeName;
        }

        /// <summary>
        ///     for class, enum, enum item.
        /// </summary>
        private List<(NameExpression, TypeName)> ParseTypeArgs(bool allowNamed) {
            var   typeArgs = new List<(NameExpression, TypeName)>();
            Token start    = Stream.Peek;
            // '(' type (',' type)* ')'
            if (Stream.MaybeEat(LeftParenthesis)) {
                if (Stream.MaybeEat(RightParenthesis)) {
                    // redundant parens
                    Blame(
                        BlameType.RedundantEmptyListOfTypeArguments,
                        start.Span.StartPosition,
                        tokenEnd
                    );
                }
                else {
                    do {
                        NameExpression name     = null;
                        int            startIdx = Stream.Index;
                        if (Stream.MaybeEat(Identifier)) {
                            Token id = Stream.Token;
                            if (Stream.MaybeEat(Assign)) {
                                if (!allowNamed) {
                                    ReportError("Base of this type cannot be named.", id);
                                }

                                name = new NameExpression(id);
                            }
                            else {
                                Stream.MoveTo(startIdx);
                            }
                        }

                        typeArgs.Add((name, ParseTypeName()));
                    } while (Stream.MaybeEat(Comma));

                    Stream.Eat(RightParenthesis);
                }
            }

            return typeArgs;
        }

        #endregion

        #region Module

        /// <summary>
        ///     <c>
        ///         module_def ::=
        ///             'module' name block
        ///     </c>
        /// </summary>
        private ModuleDefinition ParseModuleDef() {
            Token          start  = StartExprOrStmt(KeywordModule);
            Expression     name   = ParseQualifiedName();
            BlockStatement block  = ParseTopLevelBlock();
            var            result = new ModuleDefinition(name, block);
            result.MarkStart(start);
            return result;
        }

        #endregion

        #region Enum

        /// <summary>
        ///     <c>
        ///         enum_def ::=
        ///             'enum' name ['(' args_list ')']
        ///             block_start enum_item* block_terminator
        ///     </c>
        /// </summary>
        private EnumDefinition ParseEnumDef() {
            Token start = StartExprOrStmt(KeywordEnum);

            NameExpression name = ParseName();
            if (name == null) {
                return new EnumDefinition(start.Span.StartPosition, tokenEnd, null);
            }

            // TODO: support for functions in enums.
            TypeName[] bases = ParseTypeArgs(false).Select(it => it.Item2).ToArray();
            (TokenType terminator, _, bool error) = ParseBlockStart();
            var items = new List<EnumItem>();

            if (!Stream.MaybeEat(KeywordPass)
                && !error) {
                do {
                    items.Add(ParseEnumItem());
                } while (!Stream.MaybeEat(terminator)
                         && !CheckUnexpectedEoc()
                         && Stream.MaybeEat(Comma));
            }

            return new EnumDefinition(
                start.Span.StartPosition,
                tokenEnd,
                name,
                bases,
                items.ToArray()
            );
        }

        /// <summary>
        ///     <c>
        ///         enum_item ::=
        ///             name ['(' [type {',' type}] ')'] ['=' constant_expr]
        ///     </c>
        /// </summary>
        private EnumItem ParseEnumItem() {
            Stream.MaybeEatNewline();
            NameExpression                   name     = ParseName();
            List<(NameExpression, TypeName)> typeArgs = ParseTypeArgs(false);
            ConstantExpression               value    = null;
            if (Stream.MaybeEat(Assign)) {
                value = ParsePrimaryExpr() as ConstantExpression;
                if (value == null) {
                    Blame(BlameType.ConstantValueExpected, Stream.Token);
                }
            }

            return new EnumItem(name, typeArgs.Select(it => it.Item2).ToArray(), value);
        }

        #endregion

        #region Class

        /// <summary>
        ///     <c>
        ///         class_def ::=
        ///             'class' name ['(' args_list ')'] block
        ///     </c>
        /// </summary>
        private ClassDefinition ParseClassDef() {
            Token start = StartExprOrStmt(KeywordClass);

            Expression name = ParseName();
            // TODO: add generic classes
            if (name == null) {
                // no name, assume there's no class.
                return new ClassDefinition(
                    null,
                    new TypeName[0],
                    new Expression[0],
                    ErrorStmt()
                );
            }

            Expression                       metaClass = null;
            var                              bases     = new List<TypeName>();
            var                              keywords  = new List<Expression>();
            List<(NameExpression, TypeName)> types     = ParseTypeArgs(true);
            foreach ((NameExpression name, TypeName type) type in types) {
                if (type.name == null) {
                    bases.Add(type.type);
                }
                else {
                    keywords.Add(type.type);
                    if (type.name.Name.Value == "metaclass") {
                        metaClass = type.type;
                    }
                }
            }

            BlockStatement block = ParseTopLevelBlock();
            var classDef = new ClassDefinition(
                name,
                bases.ToArray(),
                keywords.ToArray(),
                block,
                metaClass
            );
            classDef.MarkPosition(start.Span.StartPosition, tokenEnd);
            return classDef;
        }

        #endregion

        #region Function

        /// <summary>
        ///     <c>
        ///         func_def ::=
        ///             'fn' name parameters ['=>' type_name] block
        ///         parameters ::=
        ///             '(' [parameters_list] ')'
        ///     </c>
        /// </summary>
        private FunctionDefinition ParseFunctionDef() {
            Position   start    = StartExprOrStmt(KeywordFn).Span.StartPosition;
            Expression funcName = ParseQualifiedName();
            // parameters
            Stream.Eat(LeftParenthesis);
            Parameter[] parameters = ParseFunctionParameterList(RightParenthesis);
            // return type
            TypeName returnType = null;
            if (Stream.MaybeEat(RightFatArrow)) {
                returnType = ParseTypeName();
            }

            var ret = new FunctionDefinition(funcName, parameters, returnType);

            PushFunction(ret);
            Statement          block = ParseTopLevelBlock();
            FunctionDefinition ret2  = PopFunction();
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
        ///     <c>
        ///         parameter_list ::=
        ///             {named_parameter ","}
        ///             ( "*" [parameter] ("," named_parameter)* ["," "**" parameter]
        ///             | "**" parameter
        ///             | named_parameter[","] )
        ///     </c>
        /// </summary>
        private Parameter[] ParseFunctionParameterList(TokenType terminator) {
            var  parameters  = new List<Parameter>();
            var  names       = new HashSet<string>(StringComparer.Ordinal);
            bool needDefault = false, readMultiply = false, hasKeywordOnlyParameter = false;
            // we want these to be the last two parameters
            Parameter listParameter = null;
            Parameter mapParameter  = null;
            while (!Stream.MaybeEat(terminator)) {
                if (Stream.MaybeEat(OpPower)) {
                    mapParameter = ParseParameter(names, ParameterKind.Map);
                    if (mapParameter == null) {
                        // no parameter name, syntax error
                        return new Parameter[0];
                    }

                    Stream.Eat(terminator);
                    break;
                }

                if (Stream.MaybeEat(OpMultiply)) {
                    if (readMultiply) {
                        ReportError(
                            "Invalid syntax (ALE-1, description_not_implemented)",
                            Stream.Peek
                        );
                        return new Parameter[0];
                    }

                    if (Stream.PeekIs(Comma)) {
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
                        var _ = false;
                        parameter = ParseNamedParameter(
                            names,
                            ParameterKind.KeywordOnly,
                            ref _
                        );
                        hasKeywordOnlyParameter = true;
                    }
                    else {
                        parameter = ParseNamedParameter(
                            names,
                            ParameterKind.Normal,
                            ref needDefault
                        );
                    }

                    if (parameter == null) {
                        // no parameter, syntax error
                        return new Parameter[0];
                    }

                    parameters.Add(parameter);
                }

                if (!Stream.MaybeEat(Comma)) {
                    Stream.Eat(terminator);
                    break;
                }
            }

            if (readMultiply
                && listParameter == null
                && mapParameter != null
                && !hasKeywordOnlyParameter) {
                ReportError("named arguments must follow bare *", Stream.Token);
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
        ///     <c>
        ///         named_parameter ::=
        ///             parameter ["=" test]
        ///     </c>
        /// </summary>
        private Parameter ParseNamedParameter(
            HashSet<string> names,
            ParameterKind   parameterKind,
            ref bool        needDefault
        ) {
            Parameter parameter = ParseParameter(names, parameterKind);
            if (Stream.MaybeEat(Assign)) {
                needDefault            = true;
                parameter.DefaultValue = ParseTestExpr();
            }
            else if (needDefault) {
                Blame(BlameType.ExpectedDefaultParameterValue, Stream.Token);
            }

            return parameter;
        }

        /// <summary>
        ///     <c>
        ///         parameter ::=
        ///             ID [':' type]
        ///     </c>
        /// </summary>
        private Parameter ParseParameter(HashSet<string> names, ParameterKind parameterKind) {
            if (!Stream.EnsureNext(Identifier)) {
                return null;
            }

            NameExpression name = ParseName();
            // type
            TypeName typeName = null;
            if (Stream.MaybeEat(Colon)) {
                typeName = ParseTypeName();
            }

            var parameter = new Parameter(name, typeName, parameterKind);
            CheckUniqueParameter(names, parameter.Name.Name);
            return parameter;
        }

        #endregion

        #endregion
    }
}