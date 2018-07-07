//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Axion.Tokens;

//namespace Axion.Processing
//{
//    /// <summary>
//    ///     Builds tokens list into syntax tree.
//    /// </summary>
//    internal sealed class Parser
//    {
//        private static readonly IReadOnlyList<string> stopValues = new[] { Defs.Newline, ";" };
//        private static SourceFile file;
//        internal static int TokenIndex;
//        private readonly IReadOnlyList<string> currentBlockStopValues = stopValues;

//        internal Parser(SourceFile file)
//        {
//            Parser.file = file;
//            TokenIndex = 0;
//        }

//        /// <summary>
//        ///     This constructor needed for child <see cref="ExprParser" />
//        ///     what processes child expressions list values separated by <see cref="currentBlockStopValues" />.
//        /// </summary>
//        /// <param name="file"></param>
//        /// <param name="currentBlockStopValues"></param>
//        private Parser(SourceFile file, IReadOnlyList<string> currentBlockStopValues)
//        {
//            Parser.file = file;
//            this.currentBlockStopValues = currentBlockStopValues;
//        }

//        internal static TokenCollection Tokens => file.Tokens;

//        /// <summary>
//        ///     Builds an AST (Abstract Syntax Tree) from <see cref="SourceFile.Tokens" />
//        ///     into the <see cref="SourceFile.SyntaxTree" />.
//        /// </summary>
//        internal Token Process()
//        {
//            Token mainBlock = ParseExpression(0, Tokens.Count);
//            return mainBlock;
//        }

//        private Token ParseExpression(int exprStart, int exprEnd, Token prevExpr = null)
//        {
//            for (int i = exprStart; i < exprEnd; i++)
//            {
//                if (TokenIndex >= Tokens.Count ||
//                    currentBlockStopValues.Contains(Tokens[TokenIndex].Value))
//                {
//                    return prevExpr;
//                }

//                Token token = Tokens[TokenIndex];

//                if (token.ID == TokenID.ID     && prevExpr == null ||
//                    token.ID == TokenID.Number && prevExpr == null ||
//                    token.ID == TokenID.String && prevExpr == null)
//                {
//                    prevExpr = token;
//                }
//                else if (Defs.Keywords.Contains(token.Value))
//                {
//                    switch (token.Value)
//                    {
//                        // use { lib1, lib2, etc }
//                        case "use":
//                        {
//                            SkipToken("use");
//                            List<Token> imports = GetExpressionsList("{", new[] { "," }, new[] { "}" });
//                            file.FileImports.AddRange(imports);
//                            break;
//                        }

//                        // if condition:
//                        //     action1()
//                        case "if":
//                        {
//                            BlockToken branchIf =
//                                new BranchingToken(token, GetExpressionsList("if", null, new[] { ":" })[0]);
//                            ParseBlock(ref branchIf);
//                            prevExpr = branchIf;
//                            break;
//                        }
//                        // else if condition:
//                        //     action2()
//                        // else:
//                        //     action3()
//                        case "else" when prevExpr is BranchingToken parentBranch && TokenIndex + 2 < Tokens.Count:
//                        {
//                            SkipToken("else");
//                            Token nextToken = Tokens[TokenIndex];
//                            switch (nextToken.Value)
//                            {
//                                // else if condition:
//                                case "if":
//                                {
//                                    BlockToken elseIf =
//                                        new BranchingToken(token, GetExpressionsList("if", null, new[] { ":" })[0]);
//                                    parentBranch.ElseIfs.Add(elseIf);
//                                    ParseBlock(ref elseIf);
//                                    break;
//                                }
//                                // else:
//                                case ":":
//                                {
//                                    ParseBlock(ref parentBranch.ElseBlock);
//                                    break;
//                                }
//                                // <invalid>
//                                default:
//                                    throw new ProcessingException(
//                                        "'else' block can't have any expressions before colon", ErrorOrigin.Parser,
//                                        nextToken.LinePosition, nextToken.ColumnPosition);
//                            }

//                            prevExpr = parentBranch;
//                            break;
//                        }

//                        case "while":
//                        {
//                            BlockToken loopToken;
//                            List<Token> declarations = GetExpressionsList("while", new[] { "," }, new[] { ":" });
//                            switch (declarations.Count)
//                            {
//                                // while condition:
//                                case 1:
//                                {
//                                    loopToken = new LoopToken(declarations[0]);
//                                    break;
//                                }
//                                // while i = 0, condition:
//                                case 2:
//                                {
//                                    loopToken = new LoopToken(declarations[1], declarations[0]);
//                                    break;
//                                }
//                                // while i = 0, condition, i++:
//                                case 3:
//                                {
//                                    loopToken = new LoopToken(declarations[1], declarations[0], declarations[2]);
//                                    break;
//                                }
//                                default:
//                                {
//                                    throw new ProcessingException(
//                                        $"Invalid declaration of 'while' loop:\r\n{string.Join("\r\n", declarations.Select(d => d.ToString()))}",
//                                        ErrorOrigin.Parser, token.LinePosition, token.ColumnPosition);
//                                }
//                            }

//                            ParseBlock(ref loopToken);
//                            prevExpr = loopToken;
//                            break;
//                        }

//                        default:
//                        {
//                            throw new ProcessingException($"Invalid position of: {token}", ErrorOrigin.Parser,
//                                                          token.LinePosition, token.ColumnPosition);
//                        }
//                    }
//                }
//                else if (Defs.Operators.TryGetValue(token.Value, out Operator @operator))
//                {
//                    string op = @operator.Value;
//                    switch (@operator.InputSide)
//                    {
//                        case InputSide.Left:
//                        {
//                            break;
//                        }
//                        // not
//                        case InputSide.Right:
//                        {
//                            Token rightOperand = MoveNextTokenSkipBreaks($"'{op}' operator without right operand");
//                            return new OperationToken(@operator, null, rightOperand);
//                        }
//                        case InputSide.Both:
//                        {
//                            switch (op)
//                            {
//                                // functionName(arg1, arg2, etc)
//                                case "(" when TokenIndex >= 1 && Tokens[TokenIndex - 1].ID == TokenID.ID:
//                                {
//                                    Token funcName = Tokens[TokenIndex - 1];
//                                    List<Token> arguments = GetExpressionsList("(", new[] { "," }, new[] { ")" });
//                                    prevExpr = new FunctionCallToken(funcName, arguments);
//                                    break;
//                                }

//                                // collectionOrString[index]
//                                case "[" when prevExpr != null && Tokens.Count > TokenIndex + 2:
//                                {
//                                    if (prevExpr.ID != TokenID.String && prevExpr.ID != TokenID.ID)
//                                    {
//                                        throw new ProcessingException(
//                                            $"Indexer cannot be applied to type: '{prevExpr.ID:G}'", ErrorOrigin.Parser,
//                                            prevExpr.LinePosition, prevExpr.ColumnPosition);
//                                    }

//                                    List<Token> list = GetExpressionsList("[", null, new[] { "]" });
//                                    if (list.Count != 1)
//                                    {
//                                        throw new ProcessingException("Invalid indexer property", ErrorOrigin.Parser,
//                                                                      token.LinePosition, token.ColumnPosition);
//                                    }

//                                    prevExpr = new IndexerToken(prevExpr, list[0]);
//                                    break;
//                                }

//                                case "{" when prevExpr != null && TokenIndex > 0:
//                                {
//                                    Token itemType;
//                                    CollectionType collectionType;
//                                    Token prevToken = Tokens[TokenIndex - 1];
//                                    // array - type{ 1, 2, 3 }
//                                    if (prevToken.ID == TokenID.BuiltIn)
//                                    {
//                                        itemType = prevExpr;
//                                        collectionType = CollectionType.Array;
//                                    }
//                                    // list - type*{ 1, 2, 3 }
//                                    else if (prevToken.Value == "*" && Tokens.Count > 3)
//                                    {
//                                        itemType = Tokens[TokenIndex - 3];
//                                        collectionType = CollectionType.List;
//                                    }
//                                    // 'array(type) {}', 'list(type) {}', 'matrix(type) {}', etc.
//                                    else if (!(prevExpr is FunctionCallToken collectionInitCallToken) ||
//                                             !Enum.TryParse(collectionInitCallToken.NameToken.Value, true,
//                                                            out collectionType))
//                                    {
//                                        throw new ProcessingException("'{' is at invalid position", ErrorOrigin.Parser,
//                                                                      token.LinePosition, token.ColumnPosition);
//                                    }
//                                    else
//                                    {
//                                        itemType = collectionInitCallToken;
//                                    }

//                                    List<Token> items = GetExpressionsList("{", new[] { "," }, new[] { "}" });
//                                    prevExpr = new CollectionToken(itemType, collectionType, items);
//                                    break;
//                                }
//                            }

//                            break;
//                        }
//                        // ++, --
//                        case InputSide.SomeOne:
//                        {
//                            if (prevExpr == null || prevExpr is BlockToken)
//                            {
//                                // prefix (++num, --num)
//                                if (TokenIndex >= Tokens.Count)
//                                {
//                                    throw new ProcessingException($"Invalid position of '{op}' operator",
//                                                                  ErrorOrigin.Parser,
//                                                                  token.LinePosition, token.ColumnPosition);
//                                }

//                                TokenIndex++;
//                                Token rightOperand = ParseExpression();
//                                TokenIndex--;

//                                if (!Defs.LiteralTypes.Contains(rightOperand.ID))
//                                {
//                                    throw new ProcessingException($"Invalid right operand: {rightOperand}",
//                                                                  ErrorOrigin.Parser, rightOperand.LinePosition,
//                                                                  rightOperand.ColumnPosition);
//                                }

//                                prevExpr = new OperationToken(@operator, null, rightOperand);
//                            }
//                            else
//                            {
//                                // postfix (num++, num--)
//                                if (!Defs.LiteralTypes.Contains(prevExpr.ID))
//                                {
//                                    throw new ProcessingException($"Invalid left operand: {prevExpr}",
//                                                                  ErrorOrigin.Parser,
//                                                                  prevExpr.LinePosition, prevExpr.ColumnPosition);
//                                }

//                                prevExpr = new OperationToken((Operator) token, prevExpr, null);
//                            }

//                            break;
//                        }
//                        default:
//                        {
//                            throw new ProcessingException($"invalid input side of: {@operator.InputSide:G}",
//                                                          ErrorOrigin.Parser);
//                        }
//                    }
//                }
//                else if (token.Value == Defs.EndOfFile)
//                {
//                    return prevExpr;
//                }
//                else if (token.Value == Defs.Newline || token.Value == Defs.Indent || token.Value == Defs.Outdent)
//                {
//                    // these types skipped if they are not in stopValues
//                }
//                else
//                {
//                    throw new ProcessingException($"Invalid token: {token}", ErrorOrigin.Parser,
//                                                  token.LinePosition, token.ColumnPosition);
//                }
//            }

//            return prevExpr;
//        }

//        /// <summary>
//        ///     Parses multiple expressions separated by <see cref="separatorValues" /> until first token matching
//        ///     <see cref="listEndValues" />.
//        /// </summary>
//        private static List<Token> GetExpressionsList(string listStartValue,
//                                                      IEnumerable<string> separatorValues,
//                                                      IReadOnlyList<string> listEndValues)
//        {
//            SkipToken(listStartValue);
//            if (TokenIndex >= Tokens.Count || listEndValues.Contains(Tokens[TokenIndex].Value))
//            {
//                return new List<Token>();
//            }

//            var listParser = new Parser(file, (separatorValues?.Union(listEndValues) ?? listEndValues).ToArray());
//            var exprList = new List<Token>();
//            for (; TokenIndex < Tokens.Count; TokenIndex++)
//            {
//                Token token = listParser.ParseExpression();
//                if (token != null)
//                {
//                    exprList.Add(token);
//                }

//                bool endOfFile = TokenIndex >= Tokens.Count;

//                if (!endOfFile && listEndValues.Contains(Tokens[TokenIndex].Value))
//                {
//                    break;
//                }

//                if (endOfFile)
//                {
//                    throw new ProcessingException(
//                        $"Expected: ('{string.Join("', '", listEndValues)}'), but reached end of file",
//                        ErrorOrigin.Parser,
//                        token?.LinePosition   ?? -1,
//                        token?.ColumnPosition ?? -1);
//                }
//            }

//            return exprList;
//        }

//        private static void ParseBlock(ref BlockToken block)
//        {
//            if (Tokens[TokenIndex].Value != ":")
//            {
//                Token invalidToken = Tokens[TokenIndex];
//                throw new ProcessingException($"Expected ':' before block, got: '{invalidToken}'",
//                                              ErrorOrigin.Parser,
//                                              invalidToken.LinePosition,
//                                              invalidToken.ColumnPosition);
//            }

//            List<Token> blockTokens =
//                GetExpressionsList(":", new[] { Defs.Newline, ";" }, new[] { Defs.Outdent, ";;" });
//            block.Children.AddRange(blockTokens);
//        }

//        private static int SkipBrace(int start)
//        {
//            Token op = Tokens[start];
//            string open = op.Value;
//            string close;
//            int step;
//            switch (op.Value)
//            {
//                case "(":
//                {
//                    close = ")";
//                    step = 1;
//                    break;
//                }
//                case ")":
//                {
//                    close = "(";
//                    step = -1;
//                    break;
//                }
//                case "[":
//                {
//                    close = "]";
//                    step = 1;
//                    break;
//                }
//                case "]":
//                {
//                    close = "[";
//                    step = -1;
//                    break;
//                }
//                case "{":
//                {
//                    close = "}";
//                    step = 1;
//                    break;
//                }
//                case "}":
//                {
//                    close = "{";
//                    step = -1;
//                    break;
//                }
//                default:
//                {
//                    throw new ProcessingException(
//                        $"Expected brace, got: '{op.Value}'", ErrorOrigin.Parser,
//                        op.LinePosition, op.ColumnPosition);
//                }
//            }

//            var c = 1;
//            int i = start;
//            while (true)
//            {
//                i += step;

//                if (i >= Tokens.Count)
//                {
//                    throw new ProcessingException(
//                        "Matching brace not found", ErrorOrigin.Parser,
//                        op.LinePosition, op.ColumnPosition);
//                }

//                if (Tokens[i].Value == open)
//                {
//                    c++;
//                }
//                else if (Tokens[i].Value == close)
//                {
//                    c--;

//                    if (c <= 0)
//                    {
//                        return i;
//                    }
//                }
//            }
//        }

//        private void ParseChain(int left, int right, Operator splitter, List<Token> result)
//        {
//            int start = left;

//            for (int i = left; i <= right; i++)
//            {
//                Operator op = Tokens[i] as Operator ??
//                              throw new ProcessingException(
//                                  $"Operator expected in chain, got '{Tokens[i].Value}'",
//                                  ErrorOrigin.Parser);
//                if (op.IsOpenBrace)
//                {
//                    i = SkipBrace(i);
//                }
//                else if (op == splitter)
//                {
//                    if (start <= i - 1)
//                    {
//                        result.Add(ParseExpression(start, i - 1));
//                    }

//                    start = i + 1;
//                }
//                else if (op.Precedence == splitter.Precedence)
//                {
//                    break;
//                }
//                else if (op.Precedence < splitter.Precedence)
//                {
//                    result.Clear();
//                    result.Add(ParseExpression(left, right));
//                    return;
//                }
//            }

//            if (start <= right)
//            {
//                result.Add(ParseExpression(start, right));
//            }
//        }

//        //
//        //
//        //
//        //
//        //

//        private static Token PeekToken(bool skipStopValues, int index = 1)
//        {
//            int nextIndex = TokenIndex + index;
//            if (skipStopValues)
//            {
//                while (nextIndex < Tokens.Count &&
//                       stopValues.Contains(Tokens[nextIndex].Value))
//                {
//                    nextIndex++;
//                }
//            }

//            return nextIndex < Tokens.Count
//                ? Tokens[nextIndex]
//                : null;
//        }

//        private static void SkipToken(string validTokenValue)
//        {
//            Token token = Tokens[TokenIndex];
//            if (token.Value != validTokenValue)
//            {
//                throw new ProcessingException(
//                    $"Expected value: '{validTokenValue}', got: '{token.Value}'",
//                    ErrorOrigin.Parser,
//                    token.LinePosition, token.ColumnPosition);
//            }

//            TokenIndex++;
//        }

//        private static Token MoveNextTokenSkipBreaks(string failAtEndMsg)
//        {
//            while (TokenIndex < Tokens.Count &&
//                   stopValues.Contains(Tokens[TokenIndex].Value))
//            {
//                TokenIndex++;
//            }

//            return TokenIndex < Tokens.Count
//                ? Tokens[TokenIndex]
//                : throw new ProcessingException(failAtEndMsg, ErrorOrigin.Parser);
//        }
//    }
//}

