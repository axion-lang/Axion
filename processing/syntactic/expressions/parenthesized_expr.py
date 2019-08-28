from __future__ import annotations

from processing.codegen.code_builder import CodeBuilder
from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.tuple_expr import TupleExpr
from processing.syntactic.expressions.type_names import TypeName


class ParenthesizedExpr(Expr):
    """ parenthesis_expr:
        '(' expr ')';
    """

    @child_property
    def value(self) -> Expr:
        pass

    @property
    def value_type(self) -> TypeName:
        return self.value.value_type

    def __init__(self, parent: Expr, value: Expr):
        super().__init__(parent)
        self.value = value

    def to_axion(self, c: CodeBuilder):
        if isinstance(self.value, TupleExpr):
            c += self.value
            return
        c += '(', self.value, ')'

    def to_csharp(self, c: CodeBuilder):
        if isinstance(self.value, TupleExpr):
            c += self.value
            return
        c += '(', self.value, ')'

    def to_python(self, c: CodeBuilder):
        if isinstance(self.value, TupleExpr):
            c += self.value
            return
        c += '(', self.value, ')'
