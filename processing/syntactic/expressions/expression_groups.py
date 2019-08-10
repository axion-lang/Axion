import processing.syntactic.expressions.expr


class DecoratorExpression(processing.syntactic.expressions.expr.Expr):
    pass


class StatementExpression(processing.syntactic.expressions.expr.Expr):
    pass


class DefinitionExpression(processing.syntactic.expressions.expr.Expr):
    pass


class GlobalExpression(processing.syntactic.expressions.expr.Expr):
    pass


class InfixExpression(GlobalExpression):
    pass


class AtomExpression(InfixExpression):
    pass


class AssignableExpression(AtomExpression):
    pass


class VarTargetExpression(AssignableExpression):
    pass
