from __future__ import annotations

from typing import List, Union

from errors.blame import BlameType
from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token import Token
from processing.lexical.tokens.token_type import TokenType
from processing.syntactic.expressions.expr import Expr
from processing.syntactic.expressions.groups import AtomExpression
from processing.location import span_marker


class NameExpr(AtomExpression):
    """ name_expr:
        ID {'.' ID};
    """

    @property
    def is_simple(self) -> bool:
        return len(self.qualifiers) > 0

    @property
    def qualifiers(self) -> List[Token]:
        return [t for t in self.name_parts if t.ttype == TokenType.identifier]

    def __init__(
            self,
            parent: Expr = None,
            name: Union[List[Token], str] = None
    ):
        super().__init__(parent)
        if isinstance(name, str):
            self.name_parts = []
            qs = name.split('.')
            for i, q in enumerate(qs):
                self.name_parts.append(Token(self.source, TokenType.identifier, q))
                if i != len(qs) - 1:
                    self.name_parts.append(Token(self.source, TokenType.op_dot, '.'))
        else:
            self.name_parts = name or []

    def __repr__(self):
        return f"{self.__class__.__name__} ('{'.'.join(map(lambda t: str(t.value), self.qualifiers))}')"

    @span_marker
    def parse(self, must_be_simple = False) -> NameExpr:
        self.name_parts.append(self.stream.eat(TokenType.identifier))
        if not must_be_simple:
            while self.stream.maybe_eat(TokenType.op_dot):
                self.name_parts.append(self.stream.token)
                self.name_parts.append(self.stream.eat(TokenType.identifier))
        return self

    def to_axion(self, c: CodeBuilder):
        c += self.name_parts

    def to_csharp(self, c: CodeBuilder):
        if self.is_simple and self.name_parts[0].value == 'self':
            c += 'this'
            return
        c += self.name_parts

    def to_python(self, c: CodeBuilder):
        c += self.name_parts
