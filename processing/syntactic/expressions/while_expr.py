from __future__ import annotations

from processing.lexical.tokens.token_type import TokenType
from processing.syntactic.expressions.block_expr import BlockExpr, BlockType
from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.groups import StatementExpression


class WhileExpr(StatementExpression):
    """ while_expr:
        'while' infix_expr block
        ['nobreak' block];
    """

    @child_property
    def condition(self) -> Expr:
        pass

    @child_property
    def block(self) -> BlockExpr:
        pass

    @child_property
    def no_break_block(self) -> BlockExpr:
        pass

    def __init__(
            self,
            parent: Expr = None,
            condition: Expr = None,
            block: BlockExpr = None,
            no_break_block: BlockExpr = None
    ):
        super().__init__(parent)
        self.condition = condition
        self.block = block
        self.no_break_block = no_break_block

    def parse(self) -> WhileExpr:
        self.stream.eat(TokenType.keyword_while)
        self.condition = self.parse_infix()
        self.block = BlockExpr(self).parse(BlockType.default)
        if self.stream.maybe_eat(TokenType.keyword_nobreak):
            self.no_break_block = BlockExpr(self).parse(BlockType.default)
        return self
