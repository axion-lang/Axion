from collections import Collection
from io import StringIO

from aenum import AutoNumberEnum


class OutputLang(AutoNumberEnum):
    axion = None
    csharp = None


class CodeBuilder:
    def __init__(self, out_lang: OutputLang, formatting: bool = False):
        self.writer = StringIO()
        self.out_lang = out_lang
        self.formatting = formatting or out_lang != OutputLang.axion
        self.indent_level = 0
        self.pending_indents = False

    @property
    def code(self) -> str:
        return str(self.writer.getvalue())

    def indent(self):
        self.indent_level += 1

    def outdent(self):
        self.indent_level -= 1

    def write(self, *values):
        from processing.text_location import Span
        from processing.lexical.tokens.token import Token

        if self.pending_indents:
            self.writer.write('    ' * self.indent_level)
            self.pending_indents = False
        for v in values:
            if isinstance(v, Span):
                getattr(v, 'to_' + self.out_lang.name)(self)
                if not self.formatting and isinstance(v, Token):
                    self.writer.write(v.ending_white)
            else:
                self.writer.write(v)

    def write_joined(self, separator: str, values: Collection):
        for i, v in enumerate(values):
            self.__iadd__(v)
            if i != len(values) - 1:
                self.__iadd__(separator)

    def write_line(self, *values):
        self.write(*values)
        self.writer.write("\n")
        self.pending_indents = True

    def __iadd__(self, other):
        if isinstance(other, tuple):
            for element in other:
                self.__append(element)
        else:
            self.__append(other)
        return self

    def __append(self, other):
        if other is None:
            return
        try:
            if isinstance(other, str):
                raise TypeError()
            values = [e for e in other]
            for v in values:
                self.write(v)
        except TypeError:
            self.write(other)
