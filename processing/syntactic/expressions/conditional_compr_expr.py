from __future__ import annotations

from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.operator import OperatorToken
from processing.lexical.tokens.token_type import TokenType
from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.groups import InfixExpression
from processing.syntactic.expressions.type_names import TypeName
from processing.syntactic.expressions.unary_expr import UnaryExpr
from processing.syntactic.parsing import parse_infix


class ConditionalComprehensionExpr(InfixExpression):
    """ conditional_comprehension:
        ('if' | 'unless') infix_expr;
    """

    @child_property
    def condition(self) -> Expr:
        pass

    @property
    def value_type(self) -> TypeName:
        return self.target.value_type

    def __init__(
            self,
            parent: Expr = None,
            condition: Expr = None
    ):
        super().__init__(parent)
        self.condition = condition

    def parse(self) -> ConditionalComprehensionExpr:
        if self.stream.maybe_eat(TokenType.keyword_if):
            self.condition = parse_infix(self)
        elif self.stream.eat(TokenType.keyword_unless):
            self.condition = UnaryExpr(
                self,
                OperatorToken(self.source, ttype = TokenType.op_not),
                parse_infix(self)
            )
        return self

    def to_axion(self, c: CodeBuilder):
        c += 'if ', self.condition

    def to_python(self, c: CodeBuilder):
        c += 'if ', self.condition
