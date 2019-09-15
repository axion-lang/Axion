from __future__ import annotations

from typing import List

from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token_type import TokenType
from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.groups import InfixExpression
from processing.syntactic.expressions.type_names import TypeName
from processing.syntactic.parsing import parse_multiple, parse_infix, parse_atom, parse_prefix


class ForComprehensionExpr(InfixExpression):
    """ for_comprehension:
        'for' simple_name_list 'in' prefix_list [comprehension];
    """

    @child_property
    def target(self) -> Expr:
        pass

    @child_property
    def item(self) -> Expr:
        pass

    @child_property
    def iterable(self) -> Expr:
        pass

    @child_property
    def conditions(self) -> List[Expr]:
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
            target: Expr = None,
            item: Expr = None,
            iterable: Expr = None,
            conditions: List[Expr] = None,
            right: Expr = None,
            nested = False
    ):
        super().__init__(parent)
        self.target = target
        self.item = item
        self.iterable = iterable
        self.conditions = conditions
        self.right = right
        self.nested = nested

    def parse(self) -> ForComprehensionExpr:
        from processing.syntactic.expressions.unary_expr import UnaryExpr
        from processing.lexical.tokens.operator import OperatorToken

        if self.target is None and not self.nested:
            self.target = parse_infix(self)
        self.stream.eat(TokenType.keyword_for)
        self.item = parse_multiple(self, parse_atom)
        self.stream.eat(TokenType.op_in)
        self.iterable = parse_multiple(self, parse_prefix)
        while self.stream.peek_is(TokenType.keyword_if, TokenType.keyword_unless):
            if self.stream.maybe_eat(TokenType.keyword_if):
                self.conditions.append(parse_infix(self))
            elif self.stream.eat(TokenType.keyword_unless):
                self.conditions.append(
                    UnaryExpr(
                        self,
                        OperatorToken(self.source, ttype = TokenType.op_not),
                        parse_infix(self)
                    )
                )
            return self
        if self.stream.peek_is(TokenType.keyword_for):
            self.right = ForComprehensionExpr(self.parent, nested = True).parse()
        return self

    def to_axion(self, c: CodeBuilder):
        c += self.target, ' for ', self.item, ' in ', self.iterable, self.right

    def to_csharp(self, c: CodeBuilder):
        c += 'from ', self.item, ' in ', self.iterable
        if self.right is not None:
            c += ' ', self.right
        if not self.nested:
            c += ' select ', self.target

    def to_python(self, c: CodeBuilder):
        c += self.target, ' for ', self.item, ' in ', self.iterable, self.right
