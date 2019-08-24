from __future__ import annotations

from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token import Token
from processing.lexical.tokens.token_type import TokenType
from processing.syntactic.expressions.atomic.name_expr import NameExpr
from processing.syntactic.expressions.expr import child_property, Expr
from processing.syntactic.expressions.type_names import TypeName


class NameDef(Expr):
    """ name_def:
        name_list ((':' type) | ('=' infix_expr) | (':' type '=' infix_expr));
    """

    @child_property
    def name(self) -> Expr:
        pass

    @child_property
    def value_type(self) -> TypeName:
        pass

    @child_property
    def value(self) -> Expr:
        pass

    def __init__(
            self,
            parent: Expr,
            name: Expr = None,
            colon_token: Token = None,
            value_type: TypeName = None,
            equals_token: Token = None,
            value: Expr = None
    ):
        super().__init__(parent)
        self.name = name
        self.colon_token = colon_token
        self.value_type = value_type
        self.equals_token = equals_token
        self.value = value

    def parse(self) -> NameDef:
        self.name = self.parse_multiple(NameExpr(self).parse)
        if self.stream.maybe_eat(TokenType.colon):
            self.colon_token = self.stream.token
            self.value_type = TypeName(self).parse()
        if self.stream.maybe_eat(TokenType.op_assign):
            self.equals_token = self.stream.token
            self.value = self.parse_infix()
        return self

    def to_axion(self, c: CodeBuilder):
        c += self.name
        if self.value_type is not None:
            c += self.colon_token, self.value_type
        if self.value is not None:
            c += self.equals_token, self.value

    def to_csharp(self, c: CodeBuilder):
        if self.value_type is None:
            c += 'var '
        else:
            c += self.value_type
        c += self.name
        if self.value is not None:
            c += ' = ', self.value
