from __future__ import annotations

import specification as spec
from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token import Token
from processing.lexical.tokens.token_type import TokenType
from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.groups import InfixExpression, StatementExpression


class BinaryExpr(StatementExpression, InfixExpression):
    """ binary_expr:
        expr OPERATOR expr;
    """

    @child_property
    def left(self) -> Expr:
        pass

    @child_property
    def right(self) -> Expr:
        pass

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

    def reduce(self):
        from processing.syntactic.expressions.parenthesized_expr import ParenthesizedExpr
        from processing.lexical.tokens.operator import OperatorToken
        from processing.syntactic.expressions.unary_expr import UnaryExpr

        # TODO: constant folding
        # TODO: aug-assignment
        if self.operator.ttype in spec.basic_compare_ops \
                and isinstance(self.left, BinaryExpr) \
                and self.left.operator.ttype in spec.basic_compare_ops:
            # (l < mid < r) -> (l < mid) and (mid < r)
            # (l >= mid >= r) -> (l >= mid) and (mid >= r)
            l = self.left.left
            l_op = self.left.operator
            mid = self.left.right
            r = self.right
            r_op = self.operator
            self.operator = OperatorToken(self.source, ttype = TokenType.op_and)
            self.left = BinaryExpr(self, l, l_op, mid)
            self.right = BinaryExpr(self, mid, r_op, r)
        elif self.operator.ttype in (TokenType.op_in, TokenType.op_not_in):
            # x in (a and b) -> (x in a) and (x in b)
            # x not in (a or b) -> (x not in a) or (x not in b)
            if isinstance(self.right, ParenthesizedExpr):
                self.right = self.right.value
            if isinstance(self.right, BinaryExpr) and self.right.operator.ttype in [TokenType.op_and, TokenType.op_or]:
                seq_op = self.operator
                self.operator = self.right.operator
                item = self.left
                self.left = BinaryExpr(self, item, seq_op, self.right.left)
                self.right = BinaryExpr(self, item, seq_op, self.right.right)
        if isinstance(self.left, (BinaryExpr, UnaryExpr)):
            self.left = ParenthesizedExpr(self, self.left)
        if isinstance(self.right, (BinaryExpr, UnaryExpr)):
            self.right = ParenthesizedExpr(self, self.right)

    def to_axion(self, c: CodeBuilder):
        self.reduce()
        c += self.left, ' ', self.operator, ' ', self.right

    def to_csharp(self, c: CodeBuilder):
        if self.operator.ttype == TokenType.op_power:
            c += 'Math.Pow(', self.left, ', ', self.right, ')'
        else:
            self.to_axion(c)

    def to_python(self, c: CodeBuilder):
        self.to_axion(c)
