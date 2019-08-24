from __future__ import annotations

from processing.lexical.tokens.token_type import TokenType
from processing.syntactic.expressions.conditional_compr_expr import ConditionalComprehensionExpr
from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.groups import InfixExpression
from processing.syntactic.expressions.type_names import TypeName


class ForComprehensionExpr(InfixExpression):
    """ for_comprehension:
        'for' simple_name_list 'in' prefix_list [comprehension];
    """

    @child_property
    def item(self) -> Expr:
        pass

    @child_property
    def iterable(self) -> Expr:
        pass

    @child_property
    def right(self) -> Expr:
        pass

    @property
    def value_type(self) -> TypeName:
        return self.target.value_type

    def __init__(
            self,
            parent: Expr = None,
            item: Expr = None,
            iterable: Expr = None,
            right: Expr = None
    ):
        super().__init__(parent)
        self.item = item
        self.iterable = iterable
        self.right = right

    def parse(self) -> ForComprehensionExpr:
        self.stream.eat(TokenType.keyword_for)
        self.item = self.parse_multiple(self.parse_atom)
        self.stream.eat(TokenType.op_in)
        self.iterable = self.parse_multiple(self.parse_prefix)
        if self.stream.peek.of_type(TokenType.keyword_for):
            self.right = ForComprehensionExpr(self.parent).parse()
        elif self.stream.peek.of_type(TokenType.keyword_if, TokenType.keyword_unless):
            self.right = ConditionalComprehensionExpr(self.parent).parse()
        return self
