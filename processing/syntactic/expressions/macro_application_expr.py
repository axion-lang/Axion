from __future__ import annotations

from typing import Optional

from processing.syntactic.expressions.definitions.macro_def import MacroDef
from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.macro_patterns import CascadePattern
from processing.syntactic.expressions.macro_patterns import TokenPattern


class MacroApplicationExpr(Expr):
    """Actually, it's not an expression,
       but some piece of code defined as expression
       by language macros."""

    @child_property
    def macro_definition(self) -> MacroDef:
        pass

    def __init__(self, parent: Expr):
        """Tries to match next piece of source
           with previously defined macro syntax-es.
           This class instance should be deleted immediately if it's
           [macro_definition] is null."""
        super().__init__(parent)
        self.macro_definition = None
        self.expressions = None

    def parse(self) -> Optional[MacroApplicationExpr]:
        try:
            self.macro_definition = list(filter(
                lambda m: isinstance(m.syntax.patterns[0], TokenPattern)
                          and m.syntax.match(self),
                self.ast.macros
            ))[0]
        except IndexError:
            self.parent = None
            return None

        if self.macro_definition is not None:
            self.expressions = list(self.ast.macro_application_parts)
            self.ast.macro_application_parts.clear()
        return self

    def parse_infix_macro(self, left_expr: Expr) -> Optional[MacroApplicationExpr]:
        """Infix macros (starts with expression).
           Tries to match next piece of source
           with previously defined macro syntax-es.
           This class instance should be deleted immediately if it's
           [macro_definition] is null."""
        try:
            self.macro_definition = list(filter(
                lambda m: len(m.syntax.patterns) > 0
                          and isinstance(m.syntax.patterns[1], TokenPattern)
                          and m.syntax.patterns[1].value == self.stream.peek.value,
                self.ast.macros
            ))[0]
        except IndexError:
            self.parent = None
            return None

        idx = self.stream.token_idx
        self.stream.eat_any()
        self.ast.macro_application_parts.append(self.stream.token)
        rest_cascade = CascadePattern(self.macro_definition.syntax.patterns[2:])
        if rest_cascade.match(self):
            self.expressions = [left_expr] + self.ast.macro_application_parts
            self.ast.macro_application_parts.clear()
        else:
            self.macro_definition = None
            self.stream.move_abs(idx)
        return self
