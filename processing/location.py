from __future__ import annotations

import abc
from dataclasses import dataclass
from functools import wraps
from typing import Optional

import source as src
import utils
from processing.codegen.code_builder import CodeBuilder


def span_marker(fn):
    """
    Decorator - automatically marks start and end
    positions of span, that is currently reading.
    :param fn: Function that reads whole(!)
    span (from it's start to it's end)
    """

    @wraps(fn)
    def wrapper(*args, **kwargs):
        if args[0].start == Location(0, 0):
            args[0].mark_start()
        span = fn(*args, **kwargs)
        if args[0].end == Location(0, 0):
            args[0].mark_end()
        return span

    return wrapper


@dataclass
class Location:
    def __init__(self, line: int, column: int):
        self.line = line + 1
        self.column = column + 1

    def __repr__(self):
        return f"{self.line}:{self.column}"


class Span(utils.AutoRepr, metaclass = abc.ABCMeta):
    def __init__(self, source: Optional[src.SourceUnit], start = Location(0, 0), end = Location(0, 0)):
        self.source = source
        self.start = start
        self.end = end

    @property
    def is_zero(self):
        return self.start == self.end == Location(0, 0)

    def mark_start(self):
        self.start = self.source.text_stream.location

    def mark_end(self):
        self.end = self.source.text_stream.location

    @abc.abstractmethod
    def to_axion(self, c: CodeBuilder):
        pass

    @abc.abstractmethod
    def to_csharp(self, c: CodeBuilder):
        pass

    @abc.abstractmethod
    def to_python(self, c: CodeBuilder):
        pass


class FreeSpan(Span):
    def to_axion(self, c: CodeBuilder):
        pass

    def to_csharp(self, c: CodeBuilder):
        pass

    def to_python(self, c: CodeBuilder):
        pass
