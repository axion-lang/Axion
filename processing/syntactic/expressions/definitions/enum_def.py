from __future__ import annotations

from typing import List

from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.expression_groups import DefinitionExpression


class EnumDef(DefinitionExpression):
    """enum_def:
       'enum' simple_name ['(' type_arg_list ')']
       block_start enum_item {',' enum_item} block_terminator;
    TODO: fix syntax for enum definition """

    @child_property
    def name(self) -> Expr:
        pass

    @child_property
    def bases(self) -> List[Expr]:
        pass

    @child_property
    def items(self) -> List[Expr]:
        pass

    @child_property
    def modifiers(self) -> List[Expr]:
        pass

    def __init__(
            self,
            parent: Expr = None,
            name: Expr = None,
            bases: List[Expr] = None,
            items: List[Expr] = None,
            modifiers: List[Expr] = None,
    ):
        super().__init__(parent)
        self.name = name
        self.bases = bases
        self.items = items
        self.modifiers = modifiers

    def parse(self) -> EnumDef:
        raise NotImplementedError()
        # self.stream.eat(TokenType.keyword_enum)
        # self.name = NameExpression(self).parse(must_be_simple = True)
        # self.block = BlockExpression(self).parse(BlockType.default)
        # return self
