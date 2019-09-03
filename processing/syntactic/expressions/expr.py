from __future__ import annotations

from typing import List, Union, Type

from anytree import NodeMixin

import processing.syntactic.expressions.ast as ast_file
import processing.syntactic.expressions.block_expr as block_file
from errors.blame import BlameSeverity
from processing.codegen.code_builder import CodeBuilder
from processing.location import Span


# noinspection PyPep8Naming
class child_property:
    def __init__(self, attr):
        self.attr = attr
        self.attr_name = "_" + attr.__name__
        self.attr_type: str = attr.__annotations__["return"]

    def __get__(self, obj: NodeMixin, obj_type = None):
        if self.attr_type.lower() == 'list[expr]' \
                and (self.attr_name not in obj.__dict__
                     or obj.__dict__[self.attr_name] is None):
            setattr(obj, self.attr_name, [])
        return obj.__dict__[self.attr_name]

    def __set__(self, obj: NodeMixin, value: Union[NodeMixin, List[NodeMixin]]):
        """
        Example of usage:
            class BreakExpression:
                @child_property
                def loop_name(self):
                    pass
            ...
            ae = BreakExpression()
            ae.loop_name = SimpleNameExpression()

        :param obj: `ae` in example above.
        :param value: `SimpleNameExpression()` in example above.
        """
        if self.attr_name in obj.__dict__ and obj.__dict__[self.attr_name] == value:
            return
        if isinstance(value, list):
            for value_item in value:
                if value_item is not None:
                    value_item.parent = obj
        elif value is None:
            if isinstance(self.attr_type, list):
                value = []
            else:
                value = None
        elif isinstance(value, Expr):
            value.parent = obj
        obj.__dict__.update({self.attr_name: value})

    def __delete__(self, obj: NodeMixin):
        del obj.__dict__[self.attr_name]


class Expr(Span, NodeMixin):
    def __init__(self, parent: Expr):
        self.parent = parent
        if parent is not None:
            super().__init__(parent.source)
            self.stream = self.source.token_stream
        else:
            super().__init__(None)

    @property
    def ast(self) -> ast_file.Ast:
        e = self
        while not isinstance(e, ast_file.Ast):
            e = e.parent
        return e

    @property
    def parent_block(self) -> block_file.BlockExpr:
        e = self
        while True:
            e = e.parent
            if isinstance(e, block_file.BlockExpr):
                break
        return e

    def of_type(self, *type_names: Type) -> bool:
        """
        Checks if expression's type equal to
        one of specified types.
        """
        return isinstance(self, type_names)

    @staticmethod
    def assert_type(expr: Union[Expr, List[Expr]], *type_names: Type):
        def check(exp):
            if exp.of_type(*type_names):
                return
            exp.source.blame(
                f"expected '{type_names[0].__name__}', but got '{exp.__class__.__name__}'",
                exp,
                BlameSeverity.error
            )

        if isinstance(expr, list):
            for expression in expr:
                check(expression)
        else:
            check(expr)

    def to_axion(self, c: CodeBuilder):
        pass

    def to_csharp(self, c: CodeBuilder):
        pass

    def to_python(self, c: CodeBuilder):
        pass
