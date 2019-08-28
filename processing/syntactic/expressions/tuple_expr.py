from __future__ import annotations

from typing import List, Collection

from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token import Token
from processing.lexical.tokens.token_type import TokenType
from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.groups import VarTargetExpression
from processing.location import span_marker


class TupleExpr(VarTargetExpression, Collection):
    @child_property
    def expressions(self) -> List[Expr]:
        pass

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
        self.__current_idx = 0

    def __contains__(self, x: object) -> bool:
        return x in self.expressions

    def __iter__(self):
        return self.expressions[self.__current_idx]

    def __len__(self):
        return len(self.expressions)

    @property
    def __current(self):
        return self.expressions[self.__current_idx] if len(self.expressions) < self.__current_idx else None

    def __next__(self):
        self.__current_idx += 1
        if self.__current is None:
            raise StopIteration
        else:
            return self.__current

    @span_marker
    def parse_empty(self) -> TupleExpr:
        self.open_paren = self.stream.eat(TokenType.open_parenthesis)
        self.close_paren = self.stream.eat(TokenType.close_parenthesis)
        return self

    def to_axion(self, c: CodeBuilder):
        c += '('
        c.write_joined(', ', self.expressions)
        c += ')'

    def to_csharp(self, c: CodeBuilder):
        c += '('
        c.write_joined(', ', self.expressions)
        c += ')'

    def to_python(self, c: CodeBuilder):
        c += '('
        c.write_joined(', ', self.expressions)
        c += ')'
