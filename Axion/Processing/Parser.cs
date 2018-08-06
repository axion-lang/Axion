using System.Collections.Generic;
using Axion.Tokens;
using Axion.Tokens.Ast;

namespace Axion.Processing {
    internal class Parser {

        private readonly SourceCode src;
        private readonly LinkedList<Token> tokens;
        private readonly Ast ast;
        
        private LinkedListNode<Token> token;
        private LinkedListNode<Token> lookahead => token.Next;

        //private readonly Stack<ImportDefinition>   imports   = new Stack<ImportDefinition>();
        //private readonly Stack<ClassDefinition>    classes   = new Stack<ClassDefinition>();
        //private readonly Stack<FunctionDefinition> functions = new Stack<FunctionDefinition>();

        internal Parser(SourceCode source) {
            src = source;
            tokens = src.Tokens;
            ast = source.SyntaxTree;
        }

        internal void Process() {
            if (tokens.Count == 0) {
                return;
            }
            token = tokens.First;
        }

        //private Statement ParseStmt() {
        //    switch (token.Value.Type) {
        //        case TokenType.KeywordIf:
        //            return ParseIfStmt();
        //        case TokenType.KeywordWhile:
        //            return ParseWhileStmt();
        //        case TokenType.KeywordFor:
        //            return ParseForStmt();
        //        case TokenType.KeywordTry:
        //            return ParseTryStatement();
        //        case TokenType.KeywordClass:
        //            return ParseClassDef();
        //        case TokenType.KeywordUse:
        //            return ParseUseStmt();
        //        case TokenType.KeywordAsync:
        //            return ParseAsyncStmt();
        //        default:
        //            return ParseSimpleStmt();
        //    }
        //}
    }
}