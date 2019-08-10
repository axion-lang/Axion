from __future__ import annotations

from pathlib import Path
from typing import List, Optional, Union

import processing.text_location
from errors.blame import BlameType, BlameSeverity
from processing.mode import ProcessingMode
from processing.options import ProcessingOptions
from utils import resolve_path


class SourceUnit:
    """Container of Axion source code.
    Different kinds of code processing
    are performed with that class.
    """

    def __init__(self, source_path: Path = None, output_path: Path = None, debug_path: Path = None):
        """
        Should not be used to instantiate class.
        """

        from processing.syntactic.expressions.ast import Ast
        from compiler import Compiler
        from errors.language_error import LanguageError
        from processing.syntactic.token_stream import TokenStream
        from processing.lexical.text_stream import TextStream

        self.options = ProcessingOptions.default
        self.mode = ProcessingMode.default

        self.text_stream: Optional[TextStream] = None

        from processing.lexical.tokens.token import Token, TokenType
        self.token_stream = TokenStream(self)
        self.mismatching_pairs: List[Token] = []
        self.process_terminators: List[TokenType] = [TokenType.end]

        self.indent_size = 0
        self.indent_char = '\0'
        self.last_indent_len = 0
        self.indent_level = 0

        self.blames: List[LanguageError] = []

        if source_path is None:
            source_path = Compiler.temp_source_path()
        self.source_path = resolve_path(source_path)

        if source_path.suffix != Compiler.source_file_ext:
            raise ValueError(f"Expected a '{Compiler.source_file_ext}' extension of '{source_path}' file.")

        if output_path is None:
            output_path = self.source_path.parent / "out" / (self.source_path.stem + Compiler.output_file_ext)
        self.output_path = resolve_path(output_path)

        if debug_path is None:
            debug_path = self.source_path.parent / "debug" / (self.source_path.stem + Compiler.debug_file_ext)
        self.debug_path = resolve_path(debug_path)

        self.ast = Ast(self)

    # region Constructors

    @staticmethod
    def from_code(source_code: str, out_file_path: Path = None) -> SourceUnit:
        """
        Initializes SourceUnit with source code.

        :param source_code: Axion source code to process.
        :param out_file_path: Path to file with compiler output.
        """
        from processing.lexical.text_stream import TextStream

        unit = SourceUnit(output_path = out_file_path)
        unit.text_stream = TextStream(source_code)
        return unit

    @staticmethod
    def from_lines(source_lines: List[str], out_file_path: Path = None) -> SourceUnit:
        """
        Initializes SourceUnit with source code lines.

        :param source_lines: Lines of Axion source code.
        :param out_file_path: Path to file with compiler output.
        """
        from processing.lexical.text_stream import TextStream

        unit = SourceUnit(output_path = out_file_path)
        unit.text_stream = TextStream("\n".join(source_lines))
        return unit

    @staticmethod
    def from_file(source_file_path: Path, out_file_path: Path = None) -> SourceUnit:
        """
        Initializes SourceUnit with source code from file.

        :param source_file_path: Path to file with source code.
        :param out_file_path: Path to file with compiler output.
        """
        from processing.lexical.text_stream import TextStream

        if not source_file_path.exists:
            raise FileNotFoundError(source_file_path)

        unit = SourceUnit(source_file_path, out_file_path)
        unit.text_stream = TextStream(source_file_path.read_text())
        return unit

    @staticmethod
    def from_interpolation(stream) -> SourceUnit:
        unit = SourceUnit()
        unit.text_stream = stream
        return unit

    # endregion

    def __repr__(self):
        return f"Source in '{self.source_path.name}' -> '{self.output_path.name}' [{self.debug_path.name}]"

    def blame(self, message: Union[str, BlameType], span: processing.text_location.Span,
              severity: BlameSeverity = None):
        from errors.language_error import LanguageError

        if isinstance(message, BlameType):
            if severity is None:
                severity = message.severity
            message = message.description
        self.blames.append(LanguageError(message, severity, span))
