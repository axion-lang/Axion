import abc
from typing import Type

import processing.syntactic.parsing as parser
import specification as spec
from processing.syntactic.expressions.expr import Expr
from processing.syntactic.expressions.type_names import TypeName
from utils import AutoRepr


class MacroPattern(AutoRepr, metaclass = abc.ABCMeta):
    @abc.abstractmethod
    def match(self, parent: Expr) -> bool:
        pass


class CascadePattern(MacroPattern):
    def __init__(
            self,
            *patterns: MacroPattern,
    ):
        self.patterns = list(patterns)

    def match(self, parent: Expr) -> bool:
        idx = parent.stream.token_idx
        for pattern in self.patterns:
            if not pattern.match(parent):
                parent.stream.move_abs(idx)
                return False
        return True


class ExpressionPattern(MacroPattern):
    def __init__(
            self,
            typ: Type = None,
            parse_fn = None
    ):
        self.typ = typ
        self.parse_fn = parse_fn

    def match(self, parent: Expr) -> bool:
        if parent.stream.peek_is(*spec.never_expr_start_types):
            return True
        idx = parent.stream.token_idx
        if self.parse_fn is not None:
            e = getattr(parser, self.parse_fn)(parent)
            parent.ast.macro_application_parts.append(e)
            return True
        parent.ast.macro_expect_type = self.typ
        if issubclass(self.typ, TypeName):
            e = TypeName(parent).parse()
        else:
            e = parser.parse_any(parent)
        parent.ast.macro_expect_type = None
        if isinstance(e, self.typ):
            parent.ast.macro_application_parts.append(e)
            return True
        parent.stream.move_abs(idx)
        return False


class MultiplePattern(MacroPattern):
    def __init__(
            self,
            *patterns: MacroPattern
    ):
        self.pattern = CascadePattern(*patterns)

    def match(self, parent: Expr) -> bool:
        match_count = 0
        while self.pattern.match(parent):
            match_count += 1
        return match_count > 0


class OptionalPattern(MacroPattern):
    def __init__(
            self,
            *patterns: MacroPattern
    ):
        self.pattern = CascadePattern(*patterns)

    def match(self, parent: Expr) -> bool:
        self.pattern.match(parent)
        return True


class OrPattern(MacroPattern):
    def __init__(
            self,
            left: MacroPattern,
            right: MacroPattern
    ):
        self.left = left
        self.right = right

    def match(self, parent: Expr) -> bool:
        return self.left.match(parent) \
               or self.right.match(parent)


class TokenPattern(MacroPattern):
    def __init__(self, value: str):
        self.value = value

    def match(self, parent: Expr) -> bool:
        if parent.stream.peek.value == self.value:
            parent.stream.eat_any()
            parent.ast.macro_application_parts.append(parent.stream.token)
            return True
        return False
