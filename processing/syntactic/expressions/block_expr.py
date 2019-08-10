from __future__ import annotations

from typing import List

from aenum import Flag, auto

from errors.blame import BlameType, BlameSeverity
from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token_type import TokenType
from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.expression_groups import DefinitionExpression


class BlockType(Flag):
    default = auto()
    named = auto()
    loop = auto()
    fn = auto()


class BlockExpr(Expr):
    """block:
       (':' expr)
       | ([':'] '{' expr* '}')
       | ([':'] NEWLINE INDENT expr+ OUTDENT);
    """

    @child_property
    def items(self) -> List[Expr]:
        pass

    def __init__(self, parent: Expr = None, items: List[Expr] = None):
        super().__init__(parent)
        self.items = items

    # region _
    def has_variable(self, var_target: Expr):
        pass

    @property
    def current_function(self):
        pass

    @current_function.setter
    def current_function(self, value):
        pass

    def pop_function(self):
        pass

    @property
    def current_class(self):
        pass

    @current_class.setter
    def current_class(self, value):
        pass

    def pop_class(self):
        pass

    @property
    def current_enum(self):
        pass

    @current_enum.setter
    def current_enum(self, value):
        pass

    def pop_enum(self):
        pass

    @property
    def current_module(self):
        pass

    @current_module.setter
    def current_module(self, value):
        pass

    def pop_module(self):
        pass

    def register_named_node(self, node: Expr, parser = None):
        pass

    # endregion

    def parse(self, block_type: BlockType) -> BlockExpr:
        terminator, error = self.parse_start()
        if terminator == TokenType.outdent and BlockType.fn in block_type:
            self.source.blame(BlameType.lambda_cannot_have_indented_body, self)
        if terminator == TokenType.newline:
            self.items.extend(self.parse_cascade())
        elif not error and not self.stream.maybe_eat(terminator):
            while True:
                self.items.extend(self.parse_cascade(terminator))
                if self.stream.maybe_eat(terminator):
                    break
                if self.stream.peek.of_type(TokenType.end):
                    if terminator != TokenType.outdent:
                        self.source.blame(BlameType.unexpected_end_of_code, self.stream.token)
                    break
        return self

    def parse_start(self) -> (TokenType, bool):
        has_colon = self.stream.maybe_eat(TokenType.colon)
        block_start = self.stream.token
        has_newline = self.stream.maybe_eat(TokenType.newline) if has_colon \
            else block_start.of_type(TokenType.newline)
        if self.stream.maybe_eat(TokenType.open_brace):
            if has_colon:
                self.source.blame(BlameType.redundant_colon_with_braces, self.stream.peek)
                return TokenType.close_brace, False
        if self.stream.maybe_eat(TokenType.indent):
            return TokenType.outdent, False
        if has_newline:
            self.source.blame(BlameType.expected_block_declaration, self.stream.peek)
            return TokenType.newline, True
        if not has_colon:
            self.source.blame("Expected ':'", self.stream.peek, BlameSeverity.error)
            return TokenType.newline, True
        return TokenType.newline, False

    def to_axion(self, c: CodeBuilder):
        from processing.syntactic.expressions.ast import Ast

        if isinstance(self, Ast):
            c.write_line()
            c += self.items
        else:
            c.indent()
            c += self.items
            c.outdent()
        c.write_line()

    def to_csharp(self, c: CodeBuilder):
        from processing.syntactic.expressions.ast import Ast

        c.write_line()
        if not isinstance(self, Ast):
            c.write_line('{')
            c.indent()
        for item in self.items:
            c += item
            if isinstance(item, DefinitionExpression):
                c.write_line()
            else:
                c.write_line(';')
        if not isinstance(self, Ast):
            c.outdent()
            c += '}'
        c.write_line()
