from __future__ import annotations

from processing.codegen.code_builder import CodeBuilder
from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.groups import InfixExpression
from processing.syntactic.expressions.type_names import TypeName


class GeneratorExpr(InfixExpression):
    """ generator_expr:
        '(' comprehension ')';
    """

    @child_property
    def comprehension(self) -> Expr:
        pass

    @property
    def value_type(self) -> TypeName:
        return self.comprehension.value_type

    def __init__(
            self,
            parent: Expr,
            comprehension: Expr
    ):
        super().__init__(parent)
        self.comprehension = comprehension

    def to_axion(self, c: CodeBuilder):
        c += '(', self.comprehension, ')'

    def to_python(self, c: CodeBuilder):
        c += '(', self.comprehension, ')'
