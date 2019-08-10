from typing import List

from errors.blame import BlameType
from processing.lexical.tokens.token import Token
from processing.lexical.tokens.token_type import TokenType
from source_unit import SourceUnit


class TokenStream:
    def __init__(self, source: SourceUnit):
        self.token_idx = -1
        self.source = source
        self.tokens: List[Token] = []

    @property
    def token(self) -> Token:
        if -1 < self.token_idx < len(self.tokens):
            return self.tokens[self.token_idx]
        return self.tokens[-1]

    @property
    def exact_peek(self) -> Token:
        if -1 < (self.token_idx + 1) < len(self.tokens):
            return self.tokens[self.token_idx + 1]
        return self.tokens[-1]

    @property
    def peek(self) -> Token:
        self.skip_trivial()
        return self.exact_peek

    def peek_by_is(self, by_idx: int, *ttypes: TokenType) -> bool:
        self.skip_trivial(*ttypes)
        if self.token_idx + by_idx < len(self.tokens):
            peek_n = self.tokens[self.token_idx + by_idx]
            return peek_n.of_type(*ttypes)
        return False

    def eat_any(self, by: int = 1) -> Token:
        if 0 <= self.token_idx + by < len(self.tokens):
            self.token_idx += by
        return self.token

    def eat(self, *ttypes: TokenType) -> Token:
        if self.peek.of_type(*ttypes):
            self.eat_any()
            return self.token
        self.source.blame(BlameType.x, self.peek)
        return self.peek

    def maybe_eat(self, *ttypes: TokenType) -> bool:
        self.skip_trivial(*ttypes)
        if self.exact_peek.of_type(*ttypes):
            self.eat_any()
            return True
        return False

    def move_abs(self, idx: int):
        assert -1 <= idx < len(self.tokens)
        self.token_idx = idx

    def skip_trivial(self, *ttypes: TokenType):
        while (self.exact_peek.of_type(TokenType.comment)
               or (self.exact_peek.of_type(TokenType.newline)
                   and TokenType.newline not in ttypes)):
            self.eat_any()
