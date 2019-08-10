from __future__ import annotations

from typing import List

from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token_type import TokenType
from processing.syntactic.expressions.atomic.name_expr import NameExpr
from processing.syntactic.expressions.block_expr import BlockExpr, BlockType
from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.expression_groups import DefinitionExpression


class ModuleDef(DefinitionExpression):
    """module_def:
       'module' name block;
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
            modifiers: List[Expr] = None
    ):
        super().__init__(parent)
        self.name = name
        self.block = block
        self.modifiers = modifiers

    def parse(self) -> ModuleDef:
        self.stream.eat(TokenType.keyword_module)
        self.name = NameExpr(self).parse(must_be_simple = True)
        self.block = BlockExpr(self).parse(BlockType.default)
        return self

    def to_axion(self, c: CodeBuilder):
        c += 'module ', self.name, self.block

    def to_csharp(self, c: CodeBuilder):
        c += 'namespace ', self.name, self.block
