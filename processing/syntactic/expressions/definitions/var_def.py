from __future__ import annotations

from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token import Token
from processing.lexical.tokens.token_type import TokenType
from processing.syntactic.expressions.definitions.name_def import NameDef
from processing.syntactic.expressions.expr import Expr
from processing.syntactic.expressions.groups import StatementExpression
from processing.syntactic.expressions.type_names import TypeName


class VarDefExpr(NameDef, StatementExpression):
    """ variable_definition_expr:
        ['let'] simple_name_list
        [':' type]
        ['=' expr_list];
    """

    def __init__(
            self,
            parent: Expr,
            let_token: Token = None,
            name: Expr = None,
            colon_token: Token = None,
            value_type: TypeName = None,
            equals_token: Token = None,
            value: Expr = None
    ):
        super().__init__(parent)
        self.let_token = let_token
        self.name = name
        self.colon_token = colon_token
        self.value_type = value_type
        self.equals_token = equals_token
        self.value = value

    def is_immutable(self):
        return self.let_token is not None

    def parse(self) -> VarDefExpr:
        if self.stream.maybe_eat(TokenType.keyword_let):
            self.let_token = self.stream.token
        super().parse()
        return self

    def to_axion(self, c: CodeBuilder):
        if self.is_immutable:
            c += 'let '
        c += self.name
        if self.value_type is not None:
            c += ': ', self.value_type
        c += ' = ', self.value

    def to_csharp(self, c: CodeBuilder):
        if self.value_type is None:
            c += 'var '
        else:
            c += self.value_type
        c += self.name, ' = ', self.value
