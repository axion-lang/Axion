from __future__ import annotations

from typing import Optional, List

from processing.syntactic.expressions.definitions.macro_def import MacroDef
from processing.syntactic.expressions.expr import Expr
from processing.syntactic.expressions.macro_patterns import CascadePattern, TokenPattern


class MacroApplication(Expr):
    def __init__(self, parent: Expr):
        """ Tries to match next piece of source
            with previously defined macro syntax-es.
            This class instance should be deleted immediately if it's
            [macro_definition] is null.
        """
        super().__init__(parent)
        self.macro_definition: Optional[MacroDef] = None
        self.expressions: List[Expr] = []

    def parse_macro(self) -> MacroApplication:
        try:
            self.macro_definition = [
                m for m in self.ast.macros
                if isinstance(m.syntax.patterns[0], TokenPattern)
                   and m.syntax.match(self)
            ][0]
        except IndexError:
            self.parent = None
            return self
        self.expressions = list(self.ast.macro_application_parts)
        self.ast.macro_application_parts.clear()
        return self

    def parse_infix_macro(self, left_expr: Expr) -> MacroApplication:
        """ Infix macros (starts with expression).
            Tries to match next piece of source
            with previously defined macro syntax-es.
            This class instance should be deleted immediately if it's
            [macro_definition] is null.
        """
        try:
            self.macro_definition = [
                m for m in self.ast.macros
                if len(m.syntax.patterns) > 0
                   and isinstance(m.syntax.patterns[1], TokenPattern)
                   and m.syntax.patterns[1].value == self.stream.peek.value
            ][0]
        except IndexError:
            self.parent = None
            return self
        self.ast.macro_application_parts.append(left_expr)
        self.ast.macro_application_parts.append(self.stream.eat_any())
        rest_cascade = CascadePattern(*self.macro_definition.syntax.patterns[2:])
        if rest_cascade.match(self):
            self.expressions = list(self.ast.macro_application_parts)
            self.ast.macro_application_parts.clear()
        else:
            self.macro_definition = None
            self.parent = None
        return self
