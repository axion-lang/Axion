import logging
from datetime import datetime
from os.path import dirname, realpath
from pathlib import Path
from pprint import pprint

from anytree import RenderTree
from anytree.render import ContStyle

from axion import logger
from processing.codegen.code_builder import OutputLang, CodeBuilder
from processing.mode import ProcessingMode
from processing.options import ProcessingOptions
from source_unit import SourceUnit
from utils import resolve_path


class Compiler:
    version = "0.4.0.0-alpha"

    source_file_ext = ".ax"
    output_file_ext = ".out"
    debug_file_ext = ".dbg.json"

    compiler_dir = Path(dirname(realpath(__file__)))

    @staticmethod
    def temp_source_path() -> Path:
        path = Path(
            Compiler.compiler_dir / "temp" /
            ("tmp-" + datetime.now().strftime("%b-%d_%H-%M-%S-%f") + Compiler.source_file_ext).lower()
        )
        return resolve_path(path)

    @staticmethod
    def process_source(
            source: SourceUnit,
            mode: ProcessingMode = ProcessingMode.compile,
            options: ProcessingOptions = ProcessingOptions.default
    ):
        logger.info(f"Processing '{source.source_path.name}'")

        def process():
            Compiler.lexical_analysis(source)
            if logger.isEnabledFor(logging.DEBUG) and False:
                Compiler.lexical_debug_output(source)

            if mode == ProcessingMode.lex:
                return

            Compiler.syntax_parsing(source)
            if logger.isEnabledFor(logging.DEBUG):
                Compiler.syntax_debug_output(source)

            if mode == ProcessingMode.parsing:
                return

            transpiled = Compiler.transpile(source, OutputLang.csharp)
            logger.debug(f"-- Generated code")
            print(transpiled)

        process()
        logger.info('-- Errors')
        pprint(source.blames)

    @staticmethod
    def lexical_analysis(source: SourceUnit):
        from processing.lexical.tokens.token import Token
        from errors.blame import BlameType

        logger.debug('-- Lexical analysis')
        while True:
            token = Token(source = source).read()
            if token is not None:
                source.token_stream.tokens.append(token)
                if token.ttype in source.process_terminators:
                    break
        for mismatch in source.mismatching_pairs:
            source.blame(
                BlameType[
                    'mismatched_' + str(mismatch.ttype)
                        .replace('TokenType.', '')
                        .replace('open_', '')
                        .replace('close_', '')
                    ],
                mismatch
            )

    @staticmethod
    def lexical_debug_output(source: SourceUnit):
        logger.debug('-- Tokens')
        print('\n'.join([repr(t) for t in source.token_stream.tokens]))

    @staticmethod
    def syntax_parsing(source: SourceUnit):
        logger.debug('-- Syntax parsing')
        source.ast.parse()

    @staticmethod
    def syntax_debug_output(source: SourceUnit):
        logger.debug('-- AST')
        print(RenderTree(source.ast, ContStyle()))

    @staticmethod
    def transpile(source: SourceUnit, out_lang: OutputLang) -> str:
        logger.debug(f"-- Code generation for '{out_lang.name}' target")
        builder = CodeBuilder(out_lang)
        builder += source.ast
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
        using_str = ''
        for using in default_imports:
            using_str += f'using {using};\n'
        return using_str + '\n' + builder.code
