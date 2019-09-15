from __future__ import annotations

from inspect import getmembers
from typing import List, Collection, Union, Optional, Type, TypeVar

from aenum import Flag, auto

import specification as spec
from errors.blame import BlameType, BlameSeverity
from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token_type import TokenType
from processing.syntactic.expressions.definitions.var_def import VarDef
from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.groups import DefinitionExpression, VarTargetExpression
from processing.syntactic.parsing import parse_any


class BlockType(Flag):
    default = auto()
    ast = auto()
    named = auto()
    loop = auto()
    anon_fn = auto()


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

    def is_defined(self, name: Union[str, VarTargetExpression]) -> bool:
        from processing.syntactic.expressions.ast import Ast

        if not isinstance(self, Ast) and self.get_parent_of_type(BlockExpr).is_defined(name):
            return True
        defs = self.get_all_defs()
        if isinstance(name, Collection):
            for n in name:
                if str(n) in defs:
                    return True
            return False
        else:
            return str(name) in defs

    def get_def_by_name(self, name: Union[str, VarTargetExpression]) -> Optional[Expr]:
        from processing.syntactic.expressions.ast import Ast

        if not isinstance(self, Ast):
            e = self.get_parent_of_type(BlockExpr).get_def_by_name(name)
            if e is not None:
                return e
        defs = self.get_all_defs()
        if isinstance(name, Collection):
            for n in name:
                s = str(n)
                if s in defs:
                    return defs[s]
            return None
        else:
            return defs.get(str(name))

    def get_all_defs(self):
        from processing.syntactic.expressions.definitions.func_def import FuncDef

        defs = {str(i.name): i for i in self.items if isinstance(i, DefinitionExpression)}
        parent_fn = self.get_parent_of_type(FuncDef)
        if parent_fn is not None:
            defs[str(parent_fn.name)] = parent_fn.name
            defs.update({str(p): p for p in parent_fn.parameters})
        return defs

    T = TypeVar('T', bound = Expr)

    def find_items_of_type(self, typ: Type[T], outs: List[Expr] = None) -> List[T]:
        outs = outs or []
        for i in self.items:
            if isinstance(i, typ):
                outs.append(i)
            elif isinstance(i, BlockExpr):
                i.find_items_of_type(typ, outs)
                child_blocks = getmembers(i, lambda o: isinstance(o, BlockExpr))
                for b in child_blocks:
                    b.find_items_of_type(typ, outs)
        return outs

    def parse(self, block_type: BlockType) -> BlockExpr:
        if block_type == BlockType.ast:
            terminator = TokenType.end
        elif not self.stream.peek_is(*spec.block_start_types):
            return self
        else:
            terminator = self.parse_start()
        if BlockType.anon_fn in block_type:
            if terminator == TokenType.outdent:
                self.source.blame(BlameType.lambda_cannot_have_indented_body, self)
            elif terminator == TokenType.newline:
                self.items.append(parse_any(self))
                return self
        while not self.stream.maybe_eat(terminator) \
                and not self.stream.peek_is(TokenType.end) \
                and not (terminator == TokenType.newline and self.stream.token.of_type(TokenType.newline)):
            self.items.append(parse_any(self))
        return self

    def parse_start(self) -> TokenType:
        has_colon = self.stream.maybe_eat(TokenType.colon)
        block_start = self.stream.token
        has_newline = self.stream.maybe_eat(TokenType.newline) if has_colon \
            else block_start.of_type(TokenType.newline)
        if self.stream.maybe_eat(TokenType.open_brace):
            if has_colon:
                self.source.blame(BlameType.redundant_colon_with_braces, self.stream.peek)
            return TokenType.close_brace
        if self.stream.maybe_eat(TokenType.indent):
            return TokenType.outdent
        if has_newline:
            self.source.blame(BlameType.expected_block_declaration, self.stream.peek)
            return TokenType.newline
        if not has_colon:
            self.source.blame("expected ':'", self.stream.peek, BlameSeverity.error)
            return TokenType.newline
        return TokenType.newline

    def to_axion(self, c: CodeBuilder):
        c.indent()
        c += self.items
        c.outdent()

    def to_csharp(self, c: CodeBuilder):
        from processing.syntactic.expressions.conditional_expr import ConditionalExpr
        from processing.syntactic.expressions.while_expr import WhileExpr
        c.write_line()
        c.write_line('{')
        c.indent()
        for item in self.items:
            c += item
            if isinstance(item, (DefinitionExpression, ConditionalExpr, WhileExpr)) \
                    and not isinstance(item, VarDef):
                c.write_line()
            else:
                c.write_line(';')
        c.outdent()
        c += '}'

    def to_python(self, c: CodeBuilder):
        c += ':'
        c.indent()
        c.write_line()
        for item in self.items:
            c += item
            c.write_line()
        c.outdent()
