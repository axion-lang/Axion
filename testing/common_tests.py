from unittest import TestCase

import specification as spec
from processing.lexical.tokens.token_type import TokenType


class CommonTests(TestCase):
    def test_spec_validation(self):
        # keywords completeness
        defined_kws = [
            tpl[1] for tpl in TokenType.__members__.items()
            if tpl[0].lower().startswith("keyword_")
        ]
        for kw in defined_kws:
            self.assertTrue(
                kw in spec.keywords.values(),
                f"Keyword '{kw}' is not defined in specification."
            )
