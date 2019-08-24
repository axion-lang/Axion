from __future__ import annotations

from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token import Token
from processing.lexical.tokens.token_type import TokenType
from processing.syntactic.expressions.expr import Expr, child_property
from processing.text_location import span_marker


class CodeQuoteExpr(Expr):
    """ code_quote_expr:
        '{{' expr '}}';
    """

    @child_property
    def value(self) -> Expr: pass

    def __init__(
            self,
            parent: Expr = None,
            open_quote: Token = None,
            close_quote: Token = None,
            value: Expr = None
    ):
        super().__init__(parent)
        self.open_quote = open_quote
        self.close_quote = close_quote
        self.value = value

    @span_marker
    def parse(self) -> CodeQuoteExpr:
        self.open_quote = self.stream.eat(TokenType.open_double_brace)
        self.value = self.parse_any()
        self.close_quote = self.stream.eat(TokenType.close_double_brace)
        return self

    def to_axion(self, c: CodeBuilder):
        c += self.open_quote, self.value, self.close_quote
