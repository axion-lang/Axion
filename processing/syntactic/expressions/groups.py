import processing.syntactic.expressions.expr as e


class DecoratorExpression(e.Expr):
    pass


class StatementExpression(e.Expr):
    pass


class DefinitionExpression(e.Expr):
    pass


class GlobalExpression(e.Expr):
    pass


class InfixExpression(GlobalExpression):
    pass


class AtomExpression(InfixExpression):
    pass


class AssignableExpression(AtomExpression):
    pass


class VarTargetExpression(AssignableExpression):
    pass
