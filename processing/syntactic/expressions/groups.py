from abc import ABCMeta

import processing.syntactic.expressions.expr as e


class DecoratorExpression(e.Expr):
    pass


class StatementExpression(e.Expr):
    pass


class DefinitionExpression(e.Expr, metaclass = ABCMeta):
    @property
    def name(self):
        return None


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
