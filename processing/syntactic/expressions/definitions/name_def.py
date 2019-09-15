from __future__ import annotations

from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token import Token
from processing.lexical.tokens.token_type import TokenType
from processing.syntactic.expressions.atomic.name_expr import NameExpr
from processing.syntactic.expressions.expr import child_property, Expr
from processing.syntactic.expressions.groups import DefinitionExpression
from processing.syntactic.expressions.type_names import TypeName, SimpleTypeName
from processing.syntactic.parsing import parse_infix


class NameDef(DefinitionExpression):
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
        self.name = NameExpr(self).parse()
        if self.stream.maybe_eat(TokenType.colon):
            self.colon_token = self.stream.token
            self.value_type = TypeName(self).parse()
        if self.stream.maybe_eat(TokenType.op_assign):
            self.equals_token = self.stream.token
            self.value = parse_infix(self)
        return self

    def to_axion(self, c: CodeBuilder):
        c += self.name
        if self.value_type is not None:
            c += self.colon_token, self.value_type
        if self.value is not None:
            c += self.equals_token, self.value

    def to_csharp(self, c: CodeBuilder):
        from processing.syntactic.expressions.definitions.func_def import FuncParameter
        from processing.syntactic.expressions.definitions.func_def import FuncDef

        # skip parameters typing for anonymous functions
        parent_fn = self.get_parent_of_type(FuncDef)
        if not (isinstance(self, FuncParameter) and parent_fn is not None and parent_fn.name is None):
            if self.value_type is None:
                if hasattr(self.value, 'value_type'):
                    typ = self.value.value_type or SimpleTypeName(self, 'Unknown')
                    c += typ, ' '
                else:
                    c += 'Unknown '
            else:
                c += self.value_type, ' '
        c += self.name
        if self.value is not None:
            c += ' = ', self.value

    def to_python(self, c: CodeBuilder):
        c += self.name
        if self.value_type is not None:
            c += ': ', self.value_type
        if self.value is not None:
            c += ' = ', self.value
