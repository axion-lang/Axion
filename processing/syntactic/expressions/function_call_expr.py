from __future__ import annotations

from typing import List

from errors.blame import BlameType
from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token import Token
from processing.lexical.tokens.token_type import TokenType
from processing.syntactic.expressions.atomic.name_expr import NameExpr
from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.expression_groups import StatementExpression, InfixExpression
from processing.syntactic.expressions.for_comprehension_expr import ForComprehensionExpr
from processing.syntactic.expressions.generator_expr import GeneratorExpr


class FunctionCallArgument(Expr):
    @child_property
    def name(self) -> NameExpr:
        pass

    @child_property
    def value(self) -> Expr:
        pass

    def __init__(
            self,
            parent: Expr = None,
            name: NameExpr = None,
            value: Expr = None
    ):
        super().__init__(parent)
        self.name = name
        self.value = value

    @staticmethod
    def parse_list(parent: Expr, first: FunctionCallArgument = None) -> List[FunctionCallArgument]:
        args: List[FunctionCallArgument] = []
        if first is not None:
            args.append(first)
        if parent.stream.peek.of_type(TokenType.close_parenthesis):
            return args
        while True:
            name_or_value = Expr(parent).parse_infix()
            if parent.stream.maybe_eat(TokenType.op_assign):
                arg = FunctionCallArgument.finish_named(parent, name_or_value)
                if not FunctionCallArgument.is_unique_kwarg(args, arg):
                    parent.source.blame(BlameType.duplicated_named_argument, arg)
            else:
                if parent.stream.maybe_eat(TokenType.op_multiply):
                    pass
                arg = FunctionCallArgument(parent, value = name_or_value)
            args.append(arg)
            if not parent.stream.maybe_eat(TokenType.comma):
                break
        return args

    @staticmethod
    def parse_generator(parent: Expr) -> List[FunctionCallArgument]:
        if parent.stream.peek.of_type(
                TokenType.close_parenthesis,
                TokenType.op_multiply,
                TokenType.op_power
        ):
            return FunctionCallArgument.parse_list(parent)
        name_or_value = Expr(parent).parse_infix()
        is_generator = False
        if parent.stream.maybe_eat(TokenType.op_assign):
            arg = FunctionCallArgument.finish_named(parent, name_or_value)
        elif parent.stream.peek.of_type(TokenType.keyword_for):
            arg = FunctionCallArgument(
                parent,
                value = GeneratorExpr(parent, ForComprehensionExpr(parent, name_or_value))
            )
        else:
            arg = FunctionCallArgument(parent, value = name_or_value)
        if not is_generator and parent.stream.maybe_eat(TokenType.comma):
            return FunctionCallArgument.parse_list(parent, arg)
        return [arg]

    @staticmethod
    def is_unique_kwarg(args: List[FunctionCallArgument], arg: FunctionCallArgument) -> bool:
        return not str(arg.name) in [str(a.name) for a in args]

    @staticmethod
    def finish_named(parent: Expr, name_or_value: Expr) -> FunctionCallArgument:
        if isinstance(name_or_value, NameExpr):
            value = Expr(parent).parse_infix()
            return FunctionCallArgument(parent, name_or_value, value)
        else:
            parent.source.blame(BlameType.expected_simple_name, name_or_value)
        return FunctionCallArgument(parent, value = name_or_value)

    def to_axion(self, c: CodeBuilder):
        c += self.name, ': ', self.value

    def to_csharp(self, c: CodeBuilder):
        c += self.value, self.name


class FunctionCallExpr(InfixExpression, StatementExpression):
    """call_expr:
       atom '(' [arg_list | (arg comprehension)] ')';
    """

    @child_property
    def target(self) -> Expr: pass

    @child_property
    def args(self) -> List[FunctionCallArgument]: pass

    def __init__(
            self,
            parent: Expr = None,
            target: Expr = None,
            open_paren: Token = None,
            args: List[FunctionCallArgument] = None,
            close_paren: Token = None
    ):
        super().__init__(parent)
        self.target = target
        self.open_paren = open_paren
        self.args = args
        self.close_paren = close_paren

    def parse(self, target: Expr = None, allow_generator = False) -> FunctionCallExpr:
        self.target = self.parse_atom() if target is None else target
        self.open_paren = self.stream.eat(TokenType.open_parenthesis)
        self.args = FunctionCallArgument.parse_generator(self) if allow_generator \
            else FunctionCallArgument.parse_list(self)
        self.close_paren = self.stream.eat(TokenType.close_parenthesis)
        return self

    def to_axion(self, c: CodeBuilder):
        c += self.target, self.open_paren, self.args, self.close_paren

    def to_csharp(self, c: CodeBuilder):
        c += self.target, '('
        c.write_joined(', ', self.args)
        c += ')'
