from __future__ import annotations

from typing import List

from aenum import Flag, auto

from errors.blame import BlameType, BlameSeverity
from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token_type import TokenType
from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.groups import DefinitionExpression
from processing.syntactic.parsing import parse_any


class BlockType(Flag):
    default = auto()
    ast = auto()
    named = auto()
    loop = auto()
    fn = auto()


class BlockExpr(Expr):
    """ block:
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

    def has_variable(self, var_target: Expr):
        pass

    def parse(self, block_type: BlockType) -> BlockExpr:
        if block_type == BlockType.ast:
            terminator, error = TokenType.end, False
        else:
            terminator, error = self.parse_start()
        if terminator == TokenType.outdent and BlockType.fn in block_type:
            self.source.blame(BlameType.lambda_cannot_have_indented_body, self)
        elif not error and not self.stream.maybe_eat(terminator):
            while True:
                self.items.extend(self.parse_cascade(terminator))
                if self.stream.maybe_eat(terminator) or terminator == TokenType.newline:
                    break
                if self.stream.peek.of_type(TokenType.end):
                    if terminator != TokenType.outdent:
                        self.source.blame(BlameType.unexpected_end_of_code, self.stream.token)
                    break
                if not self.stream.token.of_type(TokenType.outdent, TokenType.newline):
                    self.source.blame(f'newline expected, got {self.stream.token.ttype.name}', self.stream.token)
        return self

    def parse_cascade(self, terminator = TokenType.empty) -> List[Expr]:
        """ expr {';' expr} [';'] (NEWLINE | END | terminator)
        """
        items = []
        while True:
            items.append(parse_any(self))
            self.stream.maybe_eat(TokenType.semicolon)
            if self.stream.peek.of_type(TokenType.newline, TokenType.end, terminator):
                break
        return items

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
            self.source.blame("expected ':'", self.stream.peek, BlameSeverity.error)
            return TokenType.newline, True
        return TokenType.newline, False

    def to_axion(self, c: CodeBuilder):
        c.indent()
        c += self.items
        c.outdent()

    def to_csharp(self, c: CodeBuilder):
        c.write_line()
        c.write_line('{')
        c.indent()
        for item in self.items:
            c += item
            if isinstance(item, DefinitionExpression):
                c.write_line()
            else:
                c.write_line(';')
        c.outdent()
        c += '}'
        c.write_line()

    def to_python(self, c: CodeBuilder):
        c += ':'
        c.indent()
        c.write_line()
        for item in self.items:
            c += item
            c.write_line()
        c.outdent()
