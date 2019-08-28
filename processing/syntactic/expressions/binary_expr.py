from __future__ import annotations

from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token import Token
from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.groups import InfixExpression, StatementExpression


class BinaryExpr(StatementExpression, InfixExpression):
    """ binary_expr:
        expr OPERATOR expr;
    """

    @child_property
    def left(self) -> Expr: pass

    @child_property
    def right(self) -> Expr: pass

    def __init__(
            self,
            parent: Expr = None,
            left: Expr = None,
            operator: Token = None,
            right: Expr = None
    ):
        super().__init__(parent)
        self.left = left
        self.operator = operator
        self.right = right

    def to_axion(self, c: CodeBuilder):
        from processing.syntactic.expressions.unary_expr import UnaryExpr
        if isinstance(self.left, (BinaryExpr, UnaryExpr)):
            c += '(', self.left, ')'
        else:
            c += self.left
        c += ' ', self.operator, ' '
        if isinstance(self.right, (BinaryExpr, UnaryExpr)):
            c += '(', self.right, ')'
        else:
            c += self.right

    def to_csharp(self, c: CodeBuilder):
        self.to_axion(c)

    def to_python(self, c: CodeBuilder):
        self.to_axion(c)
