from typing import Optional, List

import processing.syntactic.expressions.block_expr as block_file
import source as src
import specification as spec
from errors.blame import BlameType
from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token_type import TokenType
from processing.mode import ProcessingMode
from processing.syntactic.expressions.atomic.name_expr import NameExpr
from processing.syntactic.expressions.macro_patterns import TokenPattern


class Ast(block_file.BlockExpr):
    def __init__(self, source: src.SourceUnit):
        # region imports
        from processing.syntactic.expressions.definitions.macro_def import MacroDef
        from processing.syntactic.expressions.expr import Expr
        # endregion

        super().__init__(None)
        self.source = source
        self.stream = source.token_stream
        self.macros: List[MacroDef] = []
        self.macro_expect_type: Optional[Expr] = None
        self.macro_application_parts = []

    def __repr__(self):
        return f"AST of {repr(self.source)}"

    def parse_ast(self):
        # region imports
        from processing.syntactic.expressions.macro_patterns import CascadePattern
        from processing.syntactic.expressions.macro_patterns import ExpressionPattern
        from processing.syntactic.expressions.macro_patterns import MultiplePattern
        from processing.syntactic.expressions.macro_patterns import OptionalPattern
        from processing.syntactic.expressions.macro_patterns import OrPattern
        from processing.syntactic.expressions.type_names import TypeName
        from processing.syntactic.expressions.definitions.macro_def import MacroDef
        from processing.syntactic.expressions.expr import Expr
        # endregion
        self.macros = [
            MacroDef(
                name = NameExpr(name = 'do-while'),
                patterns = [
                    self.token_pattern("do"),
                    ExpressionPattern(block_file.BlockExpr),
                    OrPattern(self.token_pattern("while"), self.token_pattern("until")),
                    ExpressionPattern(parse_fn = 'parse_infix')
                ]),
            MacroDef(
                name = NameExpr(name = 'until'),
                patterns = [
                    self.token_pattern("until"),
                    ExpressionPattern(parse_fn = 'parse_infix'),
                    ExpressionPattern(block_file.BlockExpr)
                ]),
            MacroDef(
                name = NameExpr(name = 'for-in'),
                patterns = [
                    self.token_pattern("for"),
                    ExpressionPattern(parse_fn = 'parse_atom'),
                    self.token_pattern("in"),
                    ExpressionPattern(parse_fn = 'parse_infix'),
                    ExpressionPattern(block_file.BlockExpr)
                ]),
            MacroDef(
                name = NameExpr(name = 'unless-condition'),
                patterns = [
                    self.token_pattern("unless"),
                    ExpressionPattern(parse_fn = 'parse_infix'),
                    ExpressionPattern(block_file.BlockExpr),
                    OptionalPattern(
                        OptionalPattern(
                            MultiplePattern(
                                self.token_pattern("elif"),
                                ExpressionPattern(parse_fn = 'parse_infix'),
                                ExpressionPattern(block_file.BlockExpr)
                            )
                        ),
                        CascadePattern(
                            self.token_pattern("else"),
                            ExpressionPattern(block_file.BlockExpr)
                        )
                    )
                ]),
            MacroDef(
                name = NameExpr(name = 'list-initializer'),
                patterns = [
                    self.token_pattern("["),
                    ExpressionPattern(Expr),
                    OptionalPattern(MultiplePattern(self.token_pattern(','), ExpressionPattern(Expr))),
                    self.token_pattern("]")
                ]),
            MacroDef(
                name = NameExpr(name = 'new'),
                patterns = [
                    self.token_pattern("new"),
                    ExpressionPattern(TypeName),
                    OptionalPattern(
                        self.token_pattern("("),
                        ExpressionPattern(Expr),
                        self.token_pattern(")")
                    ),
                    OptionalPattern(
                        self.token_pattern("{"),
                        OptionalPattern(
                            ExpressionPattern(Expr),
                            OptionalPattern(
                                MultiplePattern(
                                    self.token_pattern(","),
                                    ExpressionPattern(Expr)
                                )
                            ),
                            ExpressionPattern(Expr)
                        ),
                        self.token_pattern("}")
                    )
                ]),
            MacroDef(
                name = NameExpr(name = 'match'),
                patterns = [
                    ExpressionPattern(Expr),
                    self.token_pattern("match"),
                    MultiplePattern(
                        ExpressionPattern(parse_fn = 'parse_infix'),
                        self.token_pattern(":"),
                        ExpressionPattern(Expr)
                    )
                ]),
        ]
        self.parse(block_file.BlockType.ast)

    def token_pattern(self, keyword: str) -> TokenPattern:
        from processing.lexical.tokens.operator import OperatorToken

        i = max(0, self.stream.token_idx)
        while i < len(self.stream.tokens):
            token = self.stream.tokens[i]
            if token.value == keyword \
                    and token.ttype not in spec.keywords.values() \
                    and token.ttype not in spec.punctuation.values() \
                    and not isinstance(token, OperatorToken):
                token.ttype = TokenType.custom_keyword
            i += 1
        return TokenPattern(keyword)

    def to_axion(self, c: CodeBuilder):
        for i, item in enumerate(self.items):
            c += item
            if i != len(self.items) - 1:
                c += '\n'

    def to_csharp(self, c: CodeBuilder):
        from processing.syntactic.expressions.definitions.module_def import ModuleDef
        from processing.syntactic.expressions.definitions.class_def import ClassDef
        from processing.syntactic.expressions.definitions.func_def import FuncDef
        from processing.syntactic.expressions.type_names import SimpleTypeName
        from processing.syntactic.expressions.atomic.name_expr import NameExpr

        if len(self.items) == 0:
            return

        if self.source.mode == ProcessingMode.interpret:
            for e in self.items:
                if isinstance(e, ModuleDef):
                    self.source.blame(BlameType.module_not_supported_in_interpretation_mode, e)
            c += self
            return

        default_imports = [
            'System',
            'System.IO',
            'System.Linq',
            'System.Text',
            'System.Numerics',
            'System.Threading',
            'System.Diagnostics',
            'System.Collections',
            'System.Collections.Generic'
        ]
        for using in default_imports:
            c.write_line(f'using {using};')

        root_items = []
        root_classes = []
        root_functions = []
        for e in self.items:
            if isinstance(e, ModuleDef):
                c += e
            # wrap root-level classes in one namespace
            elif isinstance(e, ClassDef):
                root_classes.append(e)
            # and root-level functions in one class
            elif isinstance(e, FuncDef):
                root_functions.append(e)
            else:
                root_items.append(e)

        # wrap root-level expressions in 'Main' method
        c += ModuleDef(
            self,
            name = NameExpr(name = '__RootModule__'),
            block = block_file.BlockExpr(
                items =
                [
                    ClassDef(
                        name = NameExpr(name = '__RootClass__'),
                        block = block_file.BlockExpr(
                            items =
                            [
                                FuncDef(
                                    name = NameExpr(name = 'Main'),
                                    block = block_file.BlockExpr(items = root_items),
                                    return_type = SimpleTypeName(name = 'void')
                                )
                            ] + root_functions
                        )
                    )
                ] + root_classes
            )
        )

    def to_python(self, c: CodeBuilder):
        for item in self.items:
            c += item, '\n'
