import specification as spec
from processing.lexical.tokens.token_type import TokenType


def test_spec_validation():
    # keywords completeness
    defined_kws = [
        tpl[1] for tpl in TokenType.__members__.items()
        if tpl[0].lower().startswith("keyword_")
    ]
    for kw in defined_kws:
        assert kw in spec.keywords.values(), f"Keyword '{kw}' is not defined in specification."
