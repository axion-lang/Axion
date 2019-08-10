import processing.syntactic.expressions.block_expr as block
import source_unit as src
import specification as spec
from errors.blame import BlameType
from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token_type import TokenType
from processing.mode import ProcessingMode
from processing.syntactic.expressions.macro_patterns import TokenPattern


class Ast(block.BlockExpr):
    def __init__(self, source: src.SourceUnit):
        # region imports
        from processing.syntactic.expressions.definitions.macro_def import MacroDef
        from processing.syntactic.expressions.expr import Expr
        from processing.syntactic.expressions.macro_patterns import CascadePattern
        from processing.syntactic.expressions.macro_patterns import ExpressionPattern
        from processing.syntactic.expressions.macro_patterns import MultiplePattern
        from processing.syntactic.expressions.macro_patterns import OptionalPattern
        from processing.syntactic.expressions.macro_patterns import OrPattern
        from processing.syntactic.expressions.type_names import TypeName
        # endregion

        self.source = source
        self.stream = source.token_stream
        self.macros = [
            MacroDef(patterns = [
                self.token_pattern("do"),
                ExpressionPattern(type(block.BlockExpr)),
                OrPattern(self.token_pattern("while"), self.token_pattern("until")),
                ExpressionPattern(parse_fn = self.parse_infix)
            ]),
            MacroDef(patterns = [
                self.token_pattern("until"),
                ExpressionPattern(parse_fn = self.parse_infix),
                ExpressionPattern(type(block.BlockExpr))
            ]),
            MacroDef(patterns = [
                self.token_pattern("for"),
                ExpressionPattern(self.parse_atom),
                self.token_pattern("in"),
                ExpressionPattern(parse_fn = self.parse_infix),
                ExpressionPattern(type(block.BlockExpr))
            ]),
            MacroDef(patterns = [
                self.token_pattern("unless"),
                ExpressionPattern(parse_fn = self.parse_infix),
                ExpressionPattern(type(block.BlockExpr)),
                OptionalPattern(
                    OptionalPattern(
                        MultiplePattern(
                            self.token_pattern("elif"),
                            ExpressionPattern(parse_fn = self.parse_infix),
                            ExpressionPattern(type(block.BlockExpr))
                        )
                    ),
                    CascadePattern(
                        self.token_pattern("else"),
                        ExpressionPattern(type(block.BlockExpr))
                    )
                )
            ]),
            MacroDef(patterns = [
                self.token_pattern("["),
                OptionalPattern(ExpressionPattern(type(Expr))),
                self.token_pattern("]")
            ]),
            MacroDef(patterns = [
                self.token_pattern("new"),
                ExpressionPattern(type(TypeName)),
                OptionalPattern(
                    self.token_pattern("("),
                    ExpressionPattern(type(Expr)),
                    self.token_pattern(")")
                ),
                OptionalPattern(
                    self.token_pattern("{"),
                    OptionalPattern(
                        ExpressionPattern(type(Expr)),
                        OptionalPattern(
                            MultiplePattern(
                                self.token_pattern(","),
                                ExpressionPattern(type(Expr))
                            )
                        ),
                        ExpressionPattern(type(Expr))
                    ),
                    self.token_pattern("}")
                )
            ]),
            MacroDef(patterns = [
                ExpressionPattern(type(Expr)),
                self.token_pattern("match"),
                MultiplePattern(
                    self.token_pattern("|"),
                    ExpressionPattern(type(Expr)),
                    self.token_pattern("=>"),
                    ExpressionPattern(type(Expr))
                )
            ]),
        ]
        self.macro_expect_type = None
        self.macro_application_parts = []

    def parse(self):
        while not self.stream.maybe_eat(TokenType.end):
            self.items.extend(self.parse_cascade())

    def token_pattern(self, keyword: str) -> TokenPattern:
        from processing.lexical.tokens.operator import OperatorToken

        i = max(0, self.stream.token_idx)
        while i < len(self.stream.tokens):
            token = self.stream.tokens[i]
            if token.value == keyword \
                    and token.ttype not in spec.keywords.values() \
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
        from processing.syntactic.expressions.definitions.function_def import FunctionDef
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
            elif isinstance(e, FunctionDef):
                root_functions.append(e)
            else:
                root_items.append(e)

        # wrap root-level expressions in 'Main' method
        c += ModuleDef(
            self,
            name = NameExpr(name = '__RootModule__'),
            block = block.BlockExpr(items = [
                ClassDef(
                    name = NameExpr(name = '__RootClass__'),
                    block = block.BlockExpr(items = [
                        FunctionDef(
                            name = NameExpr(name = 'Main'),
                            block = block.BlockExpr(items = root_items),
                            return_type = SimpleTypeName(name = 'void')
                        )
                    ] + root_functions)
                )
            ] + root_classes)
        )
