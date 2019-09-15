from __future__ import annotations

from errors.blame import BlameType
from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token import Token
from processing.lexical.tokens.token_type import TokenType
from processing.location import span_marker
from processing.syntactic.expressions.atomic.name_expr import NameExpr
from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.groups import StatementExpression
from processing.syntactic.parsing import parse_atom


class BreakExpr(StatementExpression):
    """ break_expr:
        'break' [name];
    """

    @child_property
    def loop_name(self) -> NameExpr:
        pass

    def __init__(
            self,
            parent: Expr = None,
            break_token: Token = None,
            loop_name: NameExpr = None
    ):
        super().__init__(parent)
        self.break_token = break_token
        self.loop_name = loop_name
        if isinstance(loop_name, NameExpr) and not loop_name.is_simple:
            self.source.blame(BlameType.expected_simple_name, self)

    @span_marker
    def parse(self) -> BreakExpr:
        self.break_token = self.stream.eat(TokenType.keyword_break)
        if self.stream.peek_is(TokenType.identifier):
            self.loop_name = parse_atom(self)
        return self

    def to_axion(self, c: CodeBuilder):
        c += self.break_token, self.loop_name

    def to_csharp(self, c: CodeBuilder):
        c += 'break'

    def to_python(self, c: CodeBuilder):
        c += 'break'
