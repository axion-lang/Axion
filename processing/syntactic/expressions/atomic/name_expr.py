from __future__ import annotations

from typing import List, Union

from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token import Token
from processing.lexical.tokens.token_type import TokenType
from processing.location import span_marker
from processing.syntactic.expressions.expr import Expr
from processing.syntactic.expressions.groups import VarTargetExpression


class NameExpr(VarTargetExpression):
    """ name_expr:
        ID {'.' ID};
    """

    @property
    def is_simple(self) -> bool:
        return len(self.qualifiers) > 0

    @property
    def qualifiers(self) -> List[Token]:
        return [t for t in self.name_parts if t.ttype == TokenType.identifier]

    @property
    def value_type(self):
        from processing.syntactic.expressions.block_expr import BlockExpr

        x = self.get_parent_of_type(BlockExpr).get_def_by_name(self)
        if x is not None:
            return x.value_type

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

    def __str__(self):
        return '.'.join(t.value for t in self.qualifiers)

    def __repr__(self):
        return f"{self.__class__.__name__} ('{'.'.join(t.value for t in self.qualifiers)}')"

    @span_marker
    def parse(self, must_be_simple = False) -> NameExpr:
        self.stream.eat(TokenType.identifier)
        self.name_parts.append(self.stream.token)
        if not must_be_simple:
            while self.stream.maybe_eat(TokenType.op_dot):
                self.name_parts.append(self.stream.token)
                if self.stream.eat(TokenType.identifier):
                    self.name_parts.append(self.stream.token)
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
