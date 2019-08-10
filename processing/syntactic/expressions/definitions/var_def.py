from __future__ import annotations

from processing.codegen.code_builder import CodeBuilder
from processing.syntactic.expressions.left_right_expr import LeftRightExpr
from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.expression_groups import StatementExpression
from processing.syntactic.expressions.type_names import TypeName


class VarDefExpr(LeftRightExpr, StatementExpression):
    """variable_definition_expr:
       ['let'] simple_name_list
       [':' type]
       ['=' expr_list];
    """

    @child_property
    def left(self) -> Expr: pass

    @child_property
    def right(self) -> Expr: pass

    @child_property
    def value_type(self) -> TypeName:
        pass

    def __init__(
            self,
            parent: Expr = None,
            left: Expr = None,
            value_type: TypeName = None,
            right: Expr = None,
            is_immutable: bool = False
    ):
        super().__init__(parent)
        self.left = left
        self.right = right
        self.value_type = value_type
        self.is_immutable = is_immutable

    def to_axion(self, c: CodeBuilder):
        if self.is_immutable:
            c += 'let '
        c += self.left
        if self.value_type is not None:
            c += ': ', self.value_type
        c += ' = ', self.right

    def to_csharp(self, c: CodeBuilder):
        if self.value_type is None:
            c += 'var '
        else:
            c += self.value_type
        c += self.left, ' = ', self.right
