from __future__ import annotations

from typing import List

from errors.blame import BlameType
from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token import Token
from processing.lexical.tokens.token_type import TokenType
from processing.syntactic.expressions.atomic.name_expr import NameExpr
from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.for_compr_expr import ForComprehensionExpr
from processing.syntactic.expressions.generator_expr import GeneratorExpr
from processing.syntactic.expressions.groups import StatementExpression, InfixExpression


class FuncCallArg(Expr):
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
    def parse_list(parent: Expr, first: FuncCallArg = None) -> List[FuncCallArg]:
        args: List[FuncCallArg] = []
        if first is not None:
            args.append(first)
        if parent.stream.peek.of_type(TokenType.close_parenthesis):
            return args
        while True:
            name_or_value = Expr(parent).parse_infix()
            if parent.stream.maybe_eat(TokenType.op_assign):
                arg = FuncCallArg.finish_named(parent, name_or_value)
                if not FuncCallArg.is_unique_kwarg(args, arg):
                    parent.source.blame(BlameType.duplicated_named_argument, arg)
            else:
                if parent.stream.maybe_eat(TokenType.op_multiply):
                    pass
                arg = FuncCallArg(parent, value = name_or_value)
            args.append(arg)
            if not parent.stream.maybe_eat(TokenType.comma):
                break
        return args

    @staticmethod
    def parse_generator(parent: Expr) -> List[FuncCallArg]:
        if parent.stream.peek.of_type(
                TokenType.close_parenthesis,
                TokenType.op_multiply,
                TokenType.op_power
        ):
            return FuncCallArg.parse_list(parent)
        name_or_value = Expr(parent).parse_infix()
        is_generator = False
        if parent.stream.maybe_eat(TokenType.op_assign):
            arg = FuncCallArg.finish_named(parent, name_or_value)
        elif parent.stream.peek.of_type(TokenType.keyword_for):
            arg = FuncCallArg(
                parent,
                value = GeneratorExpr(parent, ForComprehensionExpr(parent).parse())
            )
        else:
            arg = FuncCallArg(parent, value = name_or_value)
        if not is_generator and parent.stream.maybe_eat(TokenType.comma):
            return FuncCallArg.parse_list(parent, arg)
        return [arg]

    @staticmethod
    def is_unique_kwarg(args: List[FuncCallArg], arg: FuncCallArg) -> bool:
        return not str(arg.name) in [str(a.name) for a in args]

    @staticmethod
    def finish_named(parent: Expr, name_or_value: Expr) -> FuncCallArg:
        if isinstance(name_or_value, NameExpr):
            value = Expr(parent).parse_infix()
            return FuncCallArg(parent, name_or_value, value)
        else:
            parent.source.blame(BlameType.expected_simple_name, name_or_value)
        return FuncCallArg(parent, value = name_or_value)

    def to_axion(self, c: CodeBuilder):
        if self.name:
            c += self.name, ' = '
        c += self.value

    def to_csharp(self, c: CodeBuilder):
        if self.name:
            c += self.name, ': '
        c += self.value

    def to_python(self, c: CodeBuilder):
        if self.name:
            c += self.name, '='
        c += self.value


class FuncCallExpr(InfixExpression, StatementExpression):
    """ call_expr:
        atom '(' [arg_list | (arg comprehension)] ')';
    """

    @child_property
    def target(self) -> Expr: pass

    @child_property
    def args(self) -> List[FuncCallArg]: pass

    def __init__(
            self,
            parent: Expr = None,
            target: Expr = None,
            open_paren: Token = None,
            args: List[FuncCallArg] = None,
            close_paren: Token = None
    ):
        super().__init__(parent)
        self.target = target
        self.open_paren = open_paren
        self.args = args
        self.close_paren = close_paren

    def parse(self, allow_generator = False) -> FuncCallExpr:
        if self.target is None:
            self.target = self.parse_atom()
        self.open_paren = self.stream.eat(TokenType.open_parenthesis)
        self.args = FuncCallArg.parse_generator(self) \
            if allow_generator \
            else FuncCallArg.parse_list(self)
        self.close_paren = self.stream.eat(TokenType.close_parenthesis)
        return self

    def to_axion(self, c: CodeBuilder):
        c += self.target, self.open_paren
        c.write_joined(', ', self.args)
        c += self.close_paren

    def to_csharp(self, c: CodeBuilder):
        c += self.target, '('
        c.write_joined(', ', self.args)
        c += ')'

    def to_python(self, c: CodeBuilder):
        c += self.target, '('
        c.write_joined(', ', self.args)
        c += ')'
