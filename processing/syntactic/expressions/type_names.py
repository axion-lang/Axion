from __future__ import annotations

from typing import Union, List

from errors.blame import BlameType
from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token import Token
from processing.lexical.tokens.token_type import TokenType
from processing.syntactic.expressions.atomic.name_expr import NameExpr
from processing.syntactic.expressions.expr import Expr, child_property


class TypeName(Expr):
    """type
       : simple_type  | tuple_type
       | generic_type | array_type
       | union_type;
    """

    @property
    def value_type(self) -> TypeName:
        return self

    def __init__(self, parent: Expr):
        super().__init__(parent)

    def parse(self) -> TypeName:
        if self.stream.peek.of_type(TokenType.open_parenthesis):
            tpl = TupleTypeName(self).parse()
            left = tpl.types[0] if len(tpl.types) == 1 else tpl
        elif self.stream.peek.of_type(TokenType.identifier):
            left = SimpleTypeName(self).parse()
        else:
            self.source.blame(BlameType.x, self.stream.peek)
            return SimpleTypeName(self, "Unknown type")
        if self.stream.peek.of_type(TokenType.open_bracket) \
                and not self.stream.peek_by_is(2, TokenType.close_bracket):
            left = GenericTypeName(self, target = left)
        if self.stream.peek.of_type(TokenType.open_bracket):
            left = ArrayTypeName(self, target = left)
        if self.stream.peek.of_type(TokenType.op_bit_or):
            left = UnionTypeName(self, left)
        return left

    def parse_named_type_args(self) -> List[(TypeName, NameExpr)]:
        return []


class SimpleTypeName(TypeName):
    @child_property
    def name(self) -> NameExpr:
        pass

    def __init__(
            self,
            parent: Expr = None,
            name: Union[NameExpr, str] = None
    ):
        super().__init__(parent)
        if isinstance(name, str):
            name = NameExpr(name = name)
        self.name = name

    def parse(self) -> SimpleTypeName:
        self.name = NameExpr(self).parse()
        return self

    def to_axion(self, c: CodeBuilder):
        c += self.name

    def to_csharp(self, c: CodeBuilder):
        c += self.name


class TupleTypeName(TypeName):
    @child_property
    def types(self) -> List[TypeName]:
        pass

    def __init__(
            self,
            parent: Expr = None,
            open_paren: Token = None,
            types: List[TypeName] = None,
            close_paren: Token = None
    ):
        super().__init__(parent)
        self.open_paren = open_paren
        self.types = types
        self.close_paren = close_paren

    def parse(self) -> TupleTypeName:
        self.open_paren = self.stream.eat(TokenType.open_parenthesis)
        if not self.stream.peek.of_type(TokenType.close_parenthesis):
            while True:
                self.types.append(super().parse())
                if not self.stream.maybe_eat(TokenType.comma):
                    break
        self.close_paren = self.stream.eat(TokenType.close_parenthesis)
        return self

    def to_axion(self, c: CodeBuilder):
        c += self.open_paren, self.types, self.close_paren

    def to_csharp(self, c: CodeBuilder):
        c += self.open_paren, self.types, self.close_paren


class GenericTypeName(TypeName):
    @child_property
    def target(self) -> TypeName:
        pass

    @child_property
    def type_args(self) -> List[TypeName]:
        pass

    def __init__(
            self,
            parent: Expr = None,
            target: TypeName = None,
            open_bracket: Token = None,
            type_args: List[TypeName] = None,
            close_bracket: Token = None
    ):
        super().__init__(parent)
        self.target = target
        self.open_bracket = open_bracket
        self.type_args = type_args
        self.close_bracket = close_bracket

    def parse(self) -> GenericTypeName:
        if self.target is None:
            self.target = super().parse()
        self.open_bracket = self.stream.eat(TokenType.open_bracket)
        while True:
            self.type_args.append(super().parse())
            if not self.stream.maybe_eat(TokenType.comma):
                break
        self.close_bracket = self.stream.eat(TokenType.close_bracket)
        return self

    def to_axion(self, c: CodeBuilder):
        c += self.target, self.open_bracket, self.type_args, self.close_bracket

    def to_csharp(self, c: CodeBuilder):
        c += self.target, '<', self.type_args, '>'


class ArrayTypeName(TypeName):
    @child_property
    def target(self) -> TypeName:
        pass

    def __init__(
            self,
            parent: Expr = None,
            target: TypeName = None,
            open_bracket: Token = None,
            close_bracket: Token = None
    ):
        super().__init__(parent)
        self.target = target
        self.open_bracket = open_bracket
        self.close_bracket = close_bracket

    def parse(self) -> ArrayTypeName:
        if self.target is None:
            self.target = super().parse()
        self.open_bracket = self.stream.eat(TokenType.open_bracket)
        self.close_bracket = self.stream.eat(TokenType.close_bracket)
        return self

    def to_axion(self, c: CodeBuilder):
        c += self.target, self.open_bracket, self.close_bracket

    def to_csharp(self, c: CodeBuilder):
        c += self.target, '[]'


class UnionTypeName(TypeName):
    @child_property
    def left_type(self) -> TypeName:
        pass

    @child_property
    def right_type(self) -> TypeName:
        pass

    def __init__(
            self,
            parent: Expr = None,
            left_type: TypeName = None,
            or_token: Token = None,
            right_type: TypeName = None,
    ):
        super().__init__(parent)
        self.left_type = left_type
        self.or_token = or_token
        self.right_type = right_type

    def parse(self) -> UnionTypeName:
        if self.left_type is None:
            self.left_type = super().parse()
        self.or_token = self.stream.eat(TokenType.op_bit_or)
        self.right_type = super().parse()
        return self

    def to_axion(self, c: CodeBuilder):
        c += self.left_type, self.or_token, self.right_type
