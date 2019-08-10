from __future__ import annotations

from typing import List

from processing.lexical.tokens.token_type import TokenType
from processing.syntactic.expressions.atomic.name_expr import NameExpr
from processing.syntactic.expressions.block_expr import BlockExpr, BlockType
from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.expression_groups import DefinitionExpression
from processing.syntactic.expressions.macro_patterns import CascadePattern
from processing.syntactic.expressions.macro_patterns import MacroPattern


class MacroDef(DefinitionExpression):
    """macro_def:
       'macro' name block;
    """

    @child_property
    def name(self) -> NameExpr:
        pass

    @child_property
    def block(self) -> BlockExpr:
        pass

    @child_property
    def modifiers(self) -> List[Expr]:
        pass

    def __init__(
            self,
            parent: Expr = None,
            name: NameExpr = None,
            block: BlockExpr = None,
            modifiers: List[Expr] = None,
            patterns: List[MacroPattern] = None
    ):
        super().__init__(parent)
        self.name = name
        self.block = block
        self.modifiers = modifiers
        self.syntax = CascadePattern(*patterns)

    def parse(self) -> MacroDef:
        self.stream.eat(TokenType.keyword_macro)
        self.name = NameExpr(self).parse(must_be_simple = True)
        self.block = BlockExpr(self).parse(BlockType.default)
        return self
