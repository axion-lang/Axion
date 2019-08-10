from __future__ import annotations

from typing import List

from processing.lexical.tokens.token import Token
from processing.lexical.tokens.token_type import TokenType
from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.expression_groups import VarTargetExpression
from processing.text_location import span_marker


class TupleExpr(VarTargetExpression):
    @child_property
    def expressions(self) -> List[Expr]: pass

    def __init__(
            self,
            parent: Expr = None,
            open_paren: Token = None,
            expressions: List[Expr] = None,
            close_paren: Token = None
    ):
        super().__init__(parent)
        self.open_paren = open_paren
        self.expressions = expressions
        self.close_paren = close_paren

    @span_marker
    def parse_empty(self) -> TupleExpr:
        self.open_paren = self.stream.eat(TokenType.open_parenthesis)
        self.close_paren = self.stream.eat(TokenType.close_parenthesis)
        return self
