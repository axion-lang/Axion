from typing import List, Optional

from errors.blame import BlameType, BlameSeverity
from processing.lexical.tokens.token import Token
from processing.lexical.tokens.token_type import TokenType
from source import SourceUnit


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

    def peek_is(self, *ttypes: TokenType) -> bool:
        return self.peek_by_is(1, *ttypes)

    def peek_by_is(self, by_idx: int, *ttypes: TokenType) -> bool:
        start_idx = self.token_idx
        self.skip_trivial(*ttypes)
        if self.token_idx + by_idx < len(self.tokens):
            peek_n = self.tokens[self.token_idx + by_idx]
            return peek_n.of_type(*ttypes)
        self.move_abs(start_idx)
        return False

    def eat_any(self, by: int = 1) -> Token:
        if 0 <= self.token_idx + by < len(self.tokens):
            self.token_idx += by
        return self.token

    def eat(self, *ttypes: TokenType, on_error: BlameType = None) -> Optional[Token]:
        self.skip_trivial(*ttypes)
        self.eat_any()
        if self.token.of_type(*ttypes):
            return self.token
        elif on_error is None:
            self.source.blame(
                f"expected a {' | '.join(tt.name for tt in ttypes)}, but got {repr(self.peek.ttype.name)}",
                self.peek,
                BlameSeverity.error
            )
        else:
            self.source.blame(on_error, self.peek)
        return None

    def maybe_eat(self, *ttypes: TokenType, exact = False) -> bool:
        start_idx = self.token_idx
        if not exact:
            self.skip_trivial(*ttypes)
        if self.exact_peek.of_type(*ttypes):
            self.eat_any()
            return True
        self.move_abs(start_idx)
        return False

    def move_abs(self, idx: int):
        assert -1 <= idx < len(self.tokens)
        self.token_idx = idx

    def skip_trivial(self, *ttypes: TokenType):
        while self.exact_peek.of_type(TokenType.comment) \
                or (self.exact_peek.of_type(TokenType.newline)
                    and TokenType.newline not in ttypes):
            self.eat_any()
