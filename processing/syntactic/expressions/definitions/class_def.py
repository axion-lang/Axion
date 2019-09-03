from __future__ import annotations

from typing import List

from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token_type import TokenType
from processing.syntactic.expressions.atomic.name_expr import NameExpr
from processing.syntactic.expressions.block_expr import BlockExpr, BlockType
from processing.syntactic.expressions.definitions.name_def import NameDef
from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.groups import DefinitionExpression, AtomExpression
from processing.syntactic.parsing import parse_multiple


class ClassDef(DefinitionExpression, AtomExpression):
    """ class_def:
        'class' simple_name [type_args] ['<' type_arg_list] block;
    """

    @child_property
    def name(self) -> NameExpr:
        pass

    @child_property
    def bases(self) -> List[Expr]:
        pass

    @child_property
    def keywords(self) -> List[Expr]:
        pass

    @child_property
    def block(self) -> BlockExpr:
        pass

    @child_property
    def modifiers(self) -> List[Expr]:
        pass

    @child_property
    def data_members(self) -> Expr:
        pass

    def __init__(
            self,
            parent: Expr = None,
            name: NameExpr = None,
            bases: List[Expr] = None,
            keywords: List[Expr] = None,
            block: BlockExpr = None,
            modifiers: List[Expr] = None
    ):
        super().__init__(parent)
        self.name = name
        self.bases = bases
        self.keywords = keywords
        self.block = block
        self.modifiers = modifiers
        self.data_members = None

    def parse(self) -> ClassDef:
        self.stream.eat(TokenType.keyword_class)
        self.name = NameExpr(self).parse(must_be_simple = True)
        # data class members
        if self.stream.peek.of_type(TokenType.open_parenthesis):
            self.data_members = parse_multiple(self, NameDef)
        # inheritance
        if self.stream.maybe_eat(TokenType.op_less):
            types = self.parse_named_type_args()
            for typ, typ_label in types:
                if typ_label is None:
                    self.bases.append(typ)
                else:
                    self.keywords.append(typ)

        self.block = BlockExpr(self).parse(BlockType.default)
        return self

    def to_axion(self, c: CodeBuilder):
        c += 'class ', self.name
        if len(self.bases) + len(self.keywords) > 0:
            c += ' < ', self.bases, self.keywords
        c += self.block

    def to_csharp(self, c: CodeBuilder):
        c += 'public class ', self.name
        if len(self.bases) > 0:
            c += ' : ', self.bases
        c += self.block

    def to_python(self, c: CodeBuilder):
        c += 'class ', self.name
        if len(self.bases) + len(self.keywords) > 0:
            c += '(', self.bases, self.keywords, ')'
        c += self.block
