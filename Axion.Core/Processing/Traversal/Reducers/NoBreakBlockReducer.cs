using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Syntactic.Expressions.Operations;
using Axion.Core.Processing.Syntactic.Expressions.Statements;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Traversal.Reducers;

public class NoBreakBlockReducer : Reducer<WhileExpr> {
    protected override void Reduce(WhileExpr loop) {
        // override Add bool before loop, that indicates, was break reached or not.
        // Find all 'break'-s in child scopes and set this
        // bool to 'true' before exiting the loop.
        // Example:
        // while x
        //     do()
        //     if a
        //         do2()
        //         break
        // nobreak
        //     do3()
        // <============================>
        // loop-X-nobreak = true
        // while x
        //     do()
        //     if a
        //         do2()
        //         loop-X-nobreak = false
        //         break
        // if loop-X-nobreak
        //     do3()
        var scope = loop.GetParent<ScopeExpr>()!;
        var (_, whileIndex) = scope.IndexOf(loop);
        var flagName = new NameExpr(
            scope,
            scope.CreateUniqueId("loop_{0}_nobreak")
        );
        scope.Items.Insert(
            whileIndex,
            new VarDef(loop) {
                Name  = flagName,
                Value = ConstantExpr.True(loop)
            }
        );
        // index of while == whileIdx + 1
        var breaks = loop.Scope.FindItemsOfType<BreakExpr>();
        var boolSetter = new BinaryExpr(loop) {
            Left = flagName,
            Operator = new OperatorToken(
                loop.Unit,
                tokenType: EqualsSign
            ),
            Right = ConstantExpr.True(loop)
        };
        foreach (var (_, parentScope, itemIndex) in breaks) {
            parentScope.Items.Insert(itemIndex, boolSetter);
        }

        scope.Items.Insert(
            whileIndex + 2,
            new IfExpr(loop) {
                Condition = flagName,
                ThenScope = loop.NoBreakScope
            }
        );
        loop.NoBreakScope = null;

        loop.Path.Node = loop;
    }
}
