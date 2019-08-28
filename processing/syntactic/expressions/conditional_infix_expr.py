from __future__ import annotations

from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token_type import TokenType
from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.groups import InfixExpression
from processing.syntactic.expressions.type_names import TypeName


class ConditionalInfixExpr(InfixExpression):
    """ conditional_infix_expr:
        expr_list ('if' | 'unless') infix_expr ['else' expr_list];
    """

    @child_property
    def condition(self) -> Expr:
        pass

    @child_property
    def true_expression(self) -> Expr:
        pass

    @child_property
    def false_expression(self) -> Expr:
        pass

    @property
    def value_type(self) -> TypeName:
        return self.true_expression.value_type

    def __init__(
            self,
            parent: Expr = None,
            condition: Expr = None,
            true_expression: Expr = None,
            false_expression: Expr = None
    ):
        super().__init__(parent)
        self.condition = condition
        self.true_expression = true_expression
        self.false_expression = false_expression

    def parse(self) -> ConditionalInfixExpr:
        if self.true_expression is None:
            self.true_expression = self.parse_infix()
        self.stream.eat(TokenType.keyword_if)
        self.condition = self.parse_infix()
        if self.stream.maybe_eat(TokenType.keyword_else):
            self.false_expression = self.parse_infix()
        return self

    def to_axion(self, c: CodeBuilder):
        c += self.true_expression, ' if ', self.condition
        if self.false_expression:
            c += ' else ', self.false_expression

    def to_csharp(self, c: CodeBuilder):
        c += (
            self.true_expression,
            ' if ', self.condition,
            ' else ', self.false_expression
        )
