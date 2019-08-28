from __future__ import annotations

from errors.blame import BlameType
from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token import Token
from processing.lexical.tokens.token_type import TokenType
from processing.syntactic.expressions.atomic.name_expr import NameExpr
from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.groups import StatementExpression
from processing.location import span_marker


class ContinueExpr(StatementExpression):
    """ continue_expr:
        'continue' [name];
    """

    @child_property
    def loop_name(self) -> NameExpr:
        pass

    def __init__(
            self,
            parent: Expr = None,
            continue_token: Token = None,
            loop_name: NameExpr = None
    ):
        super().__init__(parent)
        self.continue_token = continue_token
        self.loop_name = loop_name
        if isinstance(loop_name, NameExpr) and not loop_name.is_simple:
            self.source.blame(BlameType.expected_simple_name, self)

    @span_marker
    def parse(self) -> ContinueExpr:
        self.continue_token = self.stream.eat(TokenType.keyword_continue)
        if self.stream.peek.of_type(TokenType.identifier):
            self.loop_name = self.parse_atom()
        return self

    def to_axion(self, c: CodeBuilder):
        c += self.continue_token, self.loop_name

    def to_csharp(self, c: CodeBuilder):
        c += 'continue'

    def to_python(self, c: CodeBuilder):
        c += 'continue'
