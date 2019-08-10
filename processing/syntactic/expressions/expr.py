from __future__ import annotations

from functools import lru_cache
from typing import Sequence, List, Union

from anytree import NodeMixin

import specification as spec
from errors.blame import BlameType
from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.operator import InputSide
from processing.lexical.tokens.operator import OperatorToken
from processing.lexical.tokens.token_type import TokenType
from processing.text_location import Span


# noinspection PyPep8Naming
class child_property:
    def __init__(self, attr):
        self.attr = attr
        self.attr_name = "_" + attr.__name__
        self.attr_type: str = attr.__annotations__["return"]

    def __get__(self, obj: NodeMixin, obj_type = None):
        if self.attr_type.casefold() == 'list[expr]' \
                and (self.attr_name not in obj.__dict__
                     or obj.__dict__[self.attr_name] is None):
            setattr(obj, self.attr_name, [])
        return obj.__dict__[self.attr_name]

    def __set__(self, obj: NodeMixin, value: Union[NodeMixin, List[NodeMixin]]):
        """
        Example of usage:
            class BreakExpression:
                @child_property
                def loop_name(self): pass
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
        else:
            value.parent = obj
        obj.__dict__.update({ self.attr_name: value })

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
    @lru_cache
    def ast(self):
        from processing.syntactic.expressions.ast import Ast

        e = self
        while not isinstance(e, Ast):
            e = e.parent
        return e

    @property
    @lru_cache
    def parent_block(self):
        from processing.syntactic.expressions.block_expr import BlockExpr
        e = self
        while True:
            e = e.parent
            if isinstance(e, BlockExpr):
                break
        return e

    def of_type(self, *type_names) -> bool:
        """
        Checks if expression's type equal to
        one of specified types.
        """
        return isinstance(self, type_names)

    @staticmethod
    def assert_all_of_type(expressions: Sequence[Expr], *type_names):
        for expr in expressions:
            Expr.assert_type(expr, *type_names)

    @staticmethod
    def assert_type(expression: Expr, *type_names):
        if expression.of_type(*type_names):
            return
        expression.source.blame(BlameType.y, expression)

    def parse_atom(self) -> Expr:
        # region Expressions imports
        from processing.syntactic.expressions.unknown_expr import UnknownExpr
        from processing.syntactic.expressions.atomic.await_expr import AwaitExpr
        from processing.syntactic.expressions.atomic.empty_expr import EmptyExpr
        from processing.syntactic.expressions.atomic.break_expr import BreakExpr
        from processing.syntactic.expressions.atomic.continue_expr import ContinueExpr
        from processing.syntactic.expressions.atomic.return_expr import ReturnExpr
        from processing.syntactic.expressions.atomic.yield_expr import YieldExpr
        from processing.syntactic.expressions.atomic.code_quote import CodeQuoteExpr
        from processing.syntactic.expressions.atomic.constant_expr import ConstantExpr
        from processing.syntactic.expressions.atomic.name_expr import NameExpr
        from processing.syntactic.expressions.tuple_expr import TupleExpr
        from processing.syntactic.expressions.block_expr import BlockExpr, BlockType
        from processing.syntactic.expressions.conditional_expr import ConditionalExpr
        from processing.syntactic.expressions.while_expr import WhileExpr
        from processing.syntactic.expressions.definitions.module_def import ModuleDef
        from processing.syntactic.expressions.definitions.class_def import ClassDef
        from processing.syntactic.expressions.definitions.enum_def import EnumDef
        from processing.syntactic.expressions.definitions.function_def import FunctionDef
        from processing.syntactic.expressions.macro_application_expr import MacroApplicationExpr
        # endregion

        start_tk = self.stream.peek
        if start_tk.of_type(TokenType.identifier):
            return NameExpr(self).parse()
        elif start_tk.of_type(TokenType.semicolon, TokenType.keyword_pass):
            return EmptyExpr(self).parse()
        elif start_tk.of_type(TokenType.keyword_break):
            return BreakExpr(self).parse()
        elif start_tk.of_type(TokenType.keyword_continue):
            return ContinueExpr(self).parse()
        elif start_tk.of_type(TokenType.keyword_return):
            return ReturnExpr(self).parse()
        elif start_tk.of_type(TokenType.keyword_await):
            return AwaitExpr(self).parse()
        elif start_tk.of_type(TokenType.keyword_yield):
            return YieldExpr(self).parse()
        elif start_tk.of_type(TokenType.open_double_brace):
            return CodeQuoteExpr(self).parse()
        elif start_tk.of_type(TokenType.open_parenthesis):
            if self.stream.peek_by_is(2, TokenType.close_parenthesis):
                return TupleExpr(self).parse_empty()
            return self.parse_any_list()
        elif start_tk.of_type(TokenType.keyword_if):
            return ConditionalExpr(self).parse()
        elif start_tk.of_type(TokenType.keyword_while):
            return WhileExpr(self).parse()
        elif start_tk.of_type(TokenType.keyword_module):
            return ModuleDef(self).parse()
        elif start_tk.of_type(TokenType.keyword_class):
            return ClassDef(self).parse()
        elif start_tk.of_type(TokenType.keyword_enum):
            return EnumDef(self).parse()
        elif start_tk.of_type(TokenType.keyword_fn):
            return FunctionDef(self).parse()
        elif start_tk.of_type(TokenType.indent, TokenType.open_brace, TokenType.colon) \
                and isinstance(self.ast.macro_expect_type, BlockExpr):
            return BlockExpr(self).parse(BlockType.default)
        elif self.stream.peek.ttype in spec.constants:
            return ConstantExpr(self).parse()
        else:
            macro = MacroApplicationExpr(self).parse()
            if macro is not None:
                return macro
        return UnknownExpr(self).parse()

    def parse_suffix(self) -> Expr:
        from processing.syntactic.expressions.member_access_expr import MemberAccessExpr
        from processing.syntactic.expressions.function_call_expr import FunctionCallExpr
        from processing.syntactic.expressions.function_call_expr import FunctionCallArgument
        from processing.syntactic.expressions.unary_expr import UnaryExpr
        from processing.syntactic.expressions.indexer_expr import IndexerExpr

        def _parse_suffix(result: Expr) -> Expr:
            while True:
                if self.stream.peek.of_type(TokenType.dot):
                    result = MemberAccessExpr(self, result)
                elif self.stream.peek.of_type(TokenType.open_parenthesis):
                    result = FunctionCallExpr(self).parse(result, True)
                elif self.stream.peek.of_type(TokenType.open_bracket):

                    result = IndexerExpr(self, result)
                else:
                    break
            if self.stream.maybe_eat(TokenType.op_increment, TokenType.op_decrement):
                operator: OperatorToken = result.stream.token
                operator.operator = InputSide.right
                result = UnaryExpr(self, operator, result)
            return result

        e = self.parse_atom()
        from processing.syntactic.expressions.expression_groups import DefinitionExpression
        if isinstance(e, DefinitionExpression):
            return e
        if self.stream.maybe_eat(TokenType.right_pipeline):
            while True:
                e = FunctionCallExpr(
                    self,
                    _parse_suffix(self.parse_atom()),
                    args = [FunctionCallArgument(self, value = e)]
                )
                if not self.stream.maybe_eat(TokenType.right_pipeline):
                    break
        return _parse_suffix(e)

    def parse_prefix(self) -> Expr:
        from processing.syntactic.expressions.unary_expr import UnaryExpr
        if self.stream.maybe_eat(*spec.prefix_operators):
            operator: OperatorToken = self.stream.token
            operator.operator = InputSide.right
            return UnaryExpr(self, operator, self.parse_prefix())
        return self.parse_suffix()

    def parse_infix(self, precedence: int = 0) -> Expr:
        from processing.syntactic.expressions.binary_expr import BinaryExpr
        from processing.syntactic.expressions.conditional_infix_expr import ConditionalInfixExpr
        from processing.syntactic.expressions.macro_application_expr import MacroApplicationExpr

        left_e = self.parse_prefix()
        from processing.syntactic.expressions.expression_groups import DefinitionExpression
        if self.stream.token.of_type(TokenType.newline) \
                or isinstance(left_e, DefinitionExpression):
            return left_e
        macro = MacroApplicationExpr(self).parse_infix_macro(left_e)
        if macro is not None:
            return macro
        while True:
            if isinstance(self.stream.peek, OperatorToken):
                new_precedence = self.stream.peek.precedence
            elif not self.stream.token.of_type(TokenType.newline) \
                    and self.stream.peek.of_type(TokenType.identifier):
                new_precedence = 4
            else:
                break
            if new_precedence < precedence:
                break
            self.stream.eat_any()
            left_e = BinaryExpr(
                self,
                left_e,
                self.stream.token,
                self.parse_infix(new_precedence + 1)
            )
        if self.stream.peek.of_type(TokenType.keyword_if, TokenType.keyword_unless) \
                and not self.stream.token.of_type(TokenType.newline, TokenType.outdent):
            return ConditionalInfixExpr(self, left_e)
        return left_e

    def parse_any(self) -> Expr:
        from processing.syntactic.expressions.binary_expr import BinaryExpr
        from processing.syntactic.expressions.definitions.var_def import VarDefExpr
        from processing.syntactic.expressions.type_names import TypeName

        immutable = self.stream.maybe_eat(TokenType.keyword_let)
        e = self.parse_any_list(self.parse_infix)
        from processing.syntactic.expressions.expression_groups import VarTargetExpression
        if isinstance(e, BinaryExpr) \
                and e.operator.of_type(TokenType.op_assign) \
                and e.left.of_type(VarTargetExpression):
            if e.parent_block.has_variable(e.left):
                return e
            return VarDefExpr(
                self,
                e.left,
                None,
                e.right,
                immutable
            )
        if not immutable and not self.stream.maybe_eat(TokenType.colon):
            return e
        var_type = TypeName(self).parse()
        var_value = self.parse_any_list() if self.stream.maybe_eat(TokenType.op_assign) else None
        return VarDefExpr(self, e, var_type, var_value, immutable)

    def parse_any_list(self, parse_fn = None) -> Expr:
        from processing.syntactic.expressions.for_comprehension_expr import ForComprehensionExpr
        from processing.syntactic.expressions.generator_expr import GeneratorExpr
        from processing.syntactic.expressions.parenthesized_expr import ParenthesizedExpr

        parse_fn = parse_fn or self.parse_any
        parens = self.stream.maybe_eat(TokenType.open_parenthesis)
        lst = [parse_fn()]
        if parens and self.stream.maybe_eat(TokenType.close_parenthesis):
            return lst[0]
        # tuple
        if self.stream.maybe_eat(TokenType.comma):
            while True:
                lst.append(parse_fn())
                if not self.stream.maybe_eat(TokenType.comma):
                    break
        elif not self.stream.token.of_type(TokenType.newline) \
                and self.stream.peek.of_type(TokenType.keyword_for):
            lst[0] = ForComprehensionExpr(self, lst[0])
            if parens:
                lst[0] = GeneratorExpr(self, lst[0])
        from processing.syntactic.expressions.expression_groups import InfixExpression
        self.assert_all_of_type(lst, InfixExpression)
        if parens:
            self.stream.eat(TokenType.close_parenthesis)
            if len(lst) == 1:
                return ParenthesizedExpr(lst[0])
        return self.maybe_tuple(lst)

    def parse_cascade(self, terminator = TokenType.empty) -> List[Expr]:
        items = [self.parse_any()]
        if self.stream.maybe_eat(TokenType.semicolon):
            while self.stream.token.of_type(TokenType.semicolon) \
                    and not self.stream.maybe_eat(TokenType.newline) \
                    and not self.stream.peek.of_type(terminator, TokenType.end):
                items.append(self.parse_any())
                if self.stream.maybe_eat(terminator, TokenType.end):
                    break
                if not self.stream.maybe_eat(TokenType.semicolon):
                    self.stream.eat(TokenType.newline)
        return items

    def maybe_tuple(self, exprs: List[Expr]) -> Expr:
        from processing.syntactic.expressions.tuple_expr import TupleExpr
        if len(exprs) == 1:
            return exprs[0]
        return TupleExpr(self, expressions = exprs)

    def to_axion(self, c: CodeBuilder):
        pass

    def to_csharp(self, c: CodeBuilder):
        pass
