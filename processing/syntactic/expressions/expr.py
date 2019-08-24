from __future__ import annotations

import sys
from typing import List, Union, Callable, Type

from anytree import NodeMixin

import specification as spec
from errors.blame import BlameSeverity
from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.operator import InputSide, OperatorToken
from processing.lexical.tokens.token_type import TokenType
from processing.text_location import Span
import processing.syntactic.expressions.ast as ast_file
import processing.syntactic.expressions.block_expr as block_file


def rebind_parent(fn):
    def wrapper(*args, **kwargs):
        e = fn(*args, *kwargs)
        # Following checks prevent 'Expr' class from showing in tree.
        # That works for cases 'Expr(parent).parse_*()
        # After parsing, this rebinds 'e.parent' from 'Expr'
        # base class to 'Expr.parent'.
        if type(args[0]) == Expr and args[0].parent is not None:
            caller_name = sys._getframe().f_back.f_back.f_code.co_name
            if caller_name != wrapper.__name__:
                e.parent = args[0].parent
                args[0].parent = None
        return e

    return wrapper


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

    @rebind_parent
    def parse_multiple(self, parse_fn: Union[Callable[[], Expr], Type] = None, allow_generator = True) -> Expr:
        # region imports
        from processing.syntactic.expressions.for_compr_expr import ForComprehensionExpr
        from processing.syntactic.expressions.generator_expr import GeneratorExpr
        from processing.syntactic.expressions.parenthesized_expr import ParenthesizedExpr
        # endregion

        if isinstance(parse_fn, type):
            expr_type: Type = parse_fn

            def inner():
                return expr_type(self).parse()

            parse_fn = inner
        parse_fn = parse_fn or self.parse_any
        parens = self.stream.maybe_eat(TokenType.open_parenthesis)
        lst = [parse_fn()]  # len(lst) >= 1
        # tuple
        if self.stream.maybe_eat(TokenType.comma):
            while True:
                lst.append(parse_fn())
                if not self.stream.maybe_eat(TokenType.comma):
                    break
        elif allow_generator and not self.stream.token.of_type(TokenType.newline) \
                and self.stream.peek.of_type(TokenType.keyword_for):
            lst[0] = ForComprehensionExpr(self).parse()
            if parens:
                lst[0] = GeneratorExpr(self, lst[0])
        # self.assert_type(lst, InfixExpression)  # TODO
        if parens:
            self.stream.eat(TokenType.close_parenthesis)
            if len(lst) == 1:
                return ParenthesizedExpr(self, lst[0])
        return self.maybe_tuple(lst)

    @rebind_parent
    def parse_any(self) -> Expr:
        # region imports
        from processing.syntactic.expressions.binary_expr import BinaryExpr
        from processing.syntactic.expressions.definitions.var_def import VarDefExpr
        from processing.syntactic.expressions.type_names import TypeName
        from processing.syntactic.expressions.groups import VarTargetExpression
        # endregion

        immutable = self.stream.maybe_eat(TokenType.keyword_let)
        let_token = None
        if immutable:
            let_token = self.stream.token

        e = self.parse_multiple(self.parse_infix)
        if isinstance(e, BinaryExpr) \
                and e.operator.of_type(TokenType.op_assign) \
                and e.left.of_type(VarTargetExpression):
            if e.parent_block.has_variable(e.left):
                return e
            return VarDefExpr(
                self,
                let_token = let_token,
                name = e.left,
                equals_token = e.operator,
                value = e.right
            )
        if not immutable and not self.stream.maybe_eat(TokenType.colon):
            return e

        colon_token = self.stream.token if self.stream.token.of_type(TokenType.colon) else None
        var_type = TypeName(self).parse()

        assign_token = None
        var_value = None
        if self.stream.maybe_eat(TokenType.op_assign):
            assign_token = self.stream.token
            var_value = self.parse_multiple()
        return VarDefExpr(
            self,
            let_token = let_token,
            name = e,
            colon_token = colon_token,
            value_type = var_type,
            equals_token = assign_token,
            value = var_value
        )

    @rebind_parent
    def parse_infix(self, precedence = 0) -> Expr:
        # region imports
        from processing.syntactic.expressions.binary_expr import BinaryExpr
        from processing.syntactic.expressions.conditional_infix_expr import ConditionalInfixExpr
        from processing.syntactic.expressions.macro_application import MacroApplication
        from processing.syntactic.expressions.groups import DefinitionExpression
        # endregion

        left_e = self.parse_prefix()
        if self.stream.token.of_type(TokenType.newline) or isinstance(left_e, DefinitionExpression):
            return left_e

        macro = MacroApplication(self).parse_infix_macro(left_e)
        if macro.macro_definition is not None:
            return macro

        while True:
            if isinstance(self.stream.peek, OperatorToken):
                new_precedence = self.stream.peek.precedence
            elif not self.stream.token.of_type(TokenType.newline, TokenType.outdent) \
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
            return ConditionalInfixExpr(self, true_expression = left_e).parse()
        return left_e

    @rebind_parent
    def parse_prefix(self) -> Expr:
        from processing.syntactic.expressions.unary_expr import UnaryExpr

        if self.stream.maybe_eat(*spec.prefix_operators):
            operator: OperatorToken = self.stream.token
            operator.input_side = InputSide.right
            return UnaryExpr(self, operator, self.parse_prefix())
        return self.parse_suffix()

    @rebind_parent
    def parse_suffix(self) -> Expr:
        # region imports
        from processing.syntactic.expressions.member_access_expr import MemberAccessExpr
        from processing.syntactic.expressions.func_call_expr import FuncCallExpr
        from processing.syntactic.expressions.func_call_expr import FuncCallArg
        from processing.syntactic.expressions.unary_expr import UnaryExpr
        from processing.syntactic.expressions.indexer_expr import IndexerExpr
        from processing.syntactic.expressions.groups import DefinitionExpression
        # endregion

        def _parse_suffix(result: Expr) -> Expr:
            while True:
                if self.stream.peek.of_type(TokenType.op_dot):
                    result = MemberAccessExpr(self, target = result).parse()
                elif self.stream.peek.of_type(TokenType.open_parenthesis):
                    result = FuncCallExpr(self, target = result).parse(allow_generator = True)
                elif self.stream.peek.of_type(TokenType.open_bracket):
                    result = IndexerExpr(self, target = result).parse()
                else:
                    break
            if self.stream.maybe_eat(TokenType.op_increment, TokenType.op_decrement):
                operator: OperatorToken = result.stream.token
                operator.input_side = InputSide.right
                result = UnaryExpr(self, operator, result)
            return result

        e = self.parse_atom()
        if isinstance(e, DefinitionExpression):
            return e
        if self.stream.maybe_eat(TokenType.right_pipeline):
            while True:
                e = FuncCallExpr(
                    self,
                    _parse_suffix(self.parse_atom()),
                    args = [FuncCallArg(self, value = e)]
                )
                if not self.stream.maybe_eat(TokenType.right_pipeline):
                    break
        return _parse_suffix(e)

    @rebind_parent
    def parse_atom(self) -> Expr:
        # region imports
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
        from processing.syntactic.expressions.definitions.func_def import FuncDef
        from processing.syntactic.expressions.macro_application import MacroApplication
        # endregion

        if self.stream.peek.of_type(TokenType.identifier):
            return NameExpr(self).parse(must_be_simple = True)
        elif self.stream.peek.of_type(TokenType.semicolon, TokenType.keyword_pass):
            return EmptyExpr(self).parse()
        elif self.stream.peek.of_type(TokenType.keyword_break):
            return BreakExpr(self).parse()
        elif self.stream.peek.of_type(TokenType.keyword_continue):
            return ContinueExpr(self).parse()
        elif self.stream.peek.of_type(TokenType.keyword_return):
            return ReturnExpr(self).parse()
        elif self.stream.peek.of_type(TokenType.keyword_await):
            return AwaitExpr(self).parse()
        elif self.stream.peek.of_type(TokenType.keyword_yield):
            return YieldExpr(self).parse()
        elif self.stream.peek.of_type(TokenType.open_double_brace):
            return CodeQuoteExpr(self).parse()
        elif self.stream.peek.of_type(TokenType.open_parenthesis):
            if self.stream.peek_by_is(2, TokenType.close_parenthesis):
                return TupleExpr(self).parse_empty()
            return self.parse_multiple()
        elif self.stream.peek.of_type(TokenType.keyword_if):
            return ConditionalExpr(self).parse()
        elif self.stream.peek.of_type(TokenType.keyword_while):
            return WhileExpr(self).parse()
        elif self.stream.peek.of_type(TokenType.keyword_module):
            return ModuleDef(self).parse()
        elif self.stream.peek.of_type(TokenType.keyword_class):
            return ClassDef(self).parse()
        elif self.stream.peek.of_type(TokenType.keyword_enum):
            return EnumDef(self).parse()
        elif self.stream.peek.of_type(TokenType.keyword_fn):
            return FuncDef(self).parse()
        elif self.stream.peek.of_type(TokenType.indent, TokenType.open_brace, TokenType.colon) \
                and self.ast.macro_expect_type is not None \
                and issubclass(BlockExpr, self.ast.macro_expect_type):
            return BlockExpr(self).parse(BlockType.default)
        elif self.stream.peek.ttype in spec.constants:
            return ConstantExpr(self).parse()
        else:
            macro = MacroApplication(self).parse_macro()
            if macro.macro_definition is not None:
                return macro
        return UnknownExpr(self).parse()

    def maybe_tuple(self, exprs: List[Expr]) -> Expr:
        from processing.syntactic.expressions.tuple_expr import TupleExpr
        if len(exprs) == 1:
            return exprs[0]
        return TupleExpr(self, expressions = exprs)

    def of_type(self, *type_names: Type) -> bool:
        """
        Checks if expression's type equal to
        one of specified types.
        """
        return isinstance(self, type_names)

    @staticmethod
    def assert_type(expr: Union[Expr, List[Expr]], *type_names: Type):
        def check(e):
            if e.of_type(*type_names):
                return
            e.source.blame(
                f"Expected '{type_names[0].__name__}', but got '{e.__class__.__name__}'",
                e,
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
