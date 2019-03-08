using System;
using System.Collections.Generic;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree;
using Axion.Core.Processing.Syntax.Tree.Statements;
using Axion.Core.Processing.Syntax.Tree.Statements.Definitions;

namespace Axion.Core.Processing.Syntax.Parser {
    public partial class SyntaxParser : ParserBase {
        private          bool                      inLoop, inFinally, inFinallyLoop;
        private readonly Stack<FunctionDefinition> functions = new Stack<FunctionDefinition>();

        internal SyntaxParser(List<Token> tokens, Ast outAst, List<Exception> outBlames) : base(
            tokens,
            outAst,
            outBlames
        ) { }

        internal void Process() {
            if (Stream.Tokens.Count == 0) {
                return;
            }
            
            var statements = new List<Statement>();
            while (!Stream.MaybeEat(TokenType.EndOfCode)) {
                Statement statement = ParseStmt();
                // TODO: automatically add imports to ast.
                statements.Add(statement);
            }

            Ast.Root = new BlockStatement(statements.ToArray());
            Ast.Root.MarkPosition(
                Stream.Tokens[0],
                Stream.Tokens[Stream.Tokens.Count - 1]
            );
        }
        
        private Token StartExprOrStmt(TokenType type) {
            Stream.Eat(type);
            return Stream.Token;
        }

        #region Current function

        private FunctionDefinition currentFunction {
            get {
                if (functions != null
                    && functions.Count > 0) {
                    return functions.Peek();
                }

                return null;
            }
        }

        private FunctionDefinition PopFunction() {
            if (functions != null
                && functions.Count > 0) {
                return functions.Pop();
            }

            return null;
        }

        private void PushFunction(FunctionDefinition function) {
            functions.Push(function);
        }

        #endregion
    }
}