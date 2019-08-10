import functools
from pathlib import Path
from typing import Optional


class StaticLazyProperty:
    def __get__(self, owner):
        return functools.lru_cache(staticmethod(self.fget).__get__(owner)())


def static_lazy_property(f):
    return property(staticmethod(functools.lru_cache()(f)))


def resolve_path(p: Path) -> Path:
    """Makes path absolute and creates it, if it's not exists.
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
        from source_unit import SourceUnit
        from processing.lexical.text_stream import TextStream
        from processing.syntactic.token_stream import TokenStream
        from processing.text_location import Location
        from processing.lexical.tokens.token import Token

        attributes = dict()
        for k, v in self.__dict__.items():
            # exclude private, '@ignore_in_repr'-ed and attributes with '<object >' - formatted reprs.
            if v \
                    and not isinstance(v, (SourceUnit, TextStream, TokenStream, Location)) \
                    and not (isinstance(self, Token) and k == 'tokens') \
                    and not k.startswith('_') \
                    and (not hasattr(v, '__dict__')
                         or ('__ignore_in_repr' not in v.__dict__)) \
                    and not str(v).startswith('<'):
                attributes[k] = repr(v)

        items = (f"{k} = {v}" for k, v in attributes.items())
        result = self.__class__.__name__
        if len(attributes) > 0:
            result += " (" + ', '.join(items) + ")"
        return result
