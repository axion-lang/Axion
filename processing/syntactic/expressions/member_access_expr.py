from __future__ import annotations

from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token import Token
from processing.lexical.tokens.token_type import TokenType
from processing.syntactic.expressions.atomic.name_expr import NameExpr
from processing.syntactic.expressions.expr import Expr, child_property


class MemberAccessExpr(Expr):
    """ member_expr:
        atom '.' ID;
    """

    @child_property
    def target(self) -> Expr: pass

    @child_property
    def member(self) -> Expr: pass

    def __init__(
            self,
            parent: Expr = None,
            target: Expr = None,
            dot: Token = None,
            member: Expr = None
    ):
        super().__init__(parent)
        self.target = target
        self.dot_token = dot
        self.member = member

    def parse(self) -> MemberAccessExpr:
        if self.target is None:
            self.target = self.parse_atom()
        self.dot_token = self.stream.eat(TokenType.op_dot)
        self.member = self.parse_suffix()
        return self

    def to_axion(self, c: CodeBuilder):
        c += self.target, self.dot_token, self.member

    def to_csharp(self, c: CodeBuilder):
        c += self.target, self.dot_token, self.member

    def to_python(self, c: CodeBuilder):
        c += self.target, '.', self.member
