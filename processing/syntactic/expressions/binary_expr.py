from __future__ import annotations

from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token import Token
from processing.syntactic.expressions.left_right_expr import LeftRightExpr
from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.expression_groups import InfixExpression, StatementExpression


class BinaryExpr(LeftRightExpr, StatementExpression, InfixExpression):
    """binary_expr:
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
        c += self.left, self.operator, self.right

    def to_csharp(self, c: CodeBuilder):
        c += '(', self.left, ') ', self.operator, ' (', self.right, ')'
