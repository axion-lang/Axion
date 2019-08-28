from pathlib import Path
from typing import Optional


def rmdir(directory: Path):
    for item in directory.iterdir():
        if item.is_dir():
            rmdir(item)
        else:
            item.unlink()
    directory.rmdir()


def resolve_path(p: Path) -> Path:
    """ Makes path absolute and creates it, if it's not exists.
    """
    if not p.is_absolute():
        p = p.absolute()

    p.parent.mkdir(parents = True, exist_ok = True)
    return p


def parse_int(value: str, radix: int) -> Optional[int]:
    result = 0
    for c in value:
        one_char = hex_value(c)
        if one_char and one_char < radix:
            result = result * radix + one_char
        else:
            return None
    return result


def hex_value(c: str) -> Optional[int]:
    if c.isdigit():
        return int(c)
    elif int('a') <= int(c) <= int('z'):
        return int(c) - int('a') + 10
    elif int('A') <= int(c) <= int('Z'):
        return int(c) - int('A') + 10
    else:
        return None


def ignore_in_repr(fn):
    def wrapper(*args, **kwargs):
        obj = fn(*args, **kwargs)
        setattr(obj, '__ignore_in_repr', True)
        return obj

    return wrapper


class AutoRepr:
    def __repr__(self):
        from processing.lexical.text_stream import TextStream
        from processing.lexical.tokens.token import Token
        from processing.syntactic.token_stream import TokenStream
        from processing.location import Location
        from source import SourceUnit

        attributes = [
            f"{k} = {repr(v)}"
            for k, v in self.__dict__.items()
            # exclude private, '@ignore_in_repr'-ed and attributes with '<object >' - formatted reprs.
            # @formatter:off
            if v and not (
                k.startswith('_')
                or isinstance(v, (SourceUnit, TextStream, TokenStream, Location))
                or (isinstance(self, Token) and (k in ['tokens', 'content', 'ending_white']))
            ) \
            and (not hasattr(v, '__dict__') or ('__ignore_in_repr' not in v.__dict__))
            # @formatter:on
        ]
        result = self.__class__.__name__
        if len(attributes) > 0:
            result += " (" + ', '.join(attributes) + ")"
        return result
