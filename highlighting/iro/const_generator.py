""" This script generates 'constants' section for IRO highlighting grammar file.
"""

import re

import specification as spec

operator_re = '|'.join(re.escape(op) for op in spec.operators_keys)

constants = {
    'identifier':       r'\b(' r'[a-zA-Z_](?:[\w\-]*[\?!]?(?<!-))' r')\b',
    'keyword':          r'\b(' + '|'.join(spec.keywords) + r')\b',
    'constant_keyword': r'\b(' + '|'.join([tt.name[8:]
                                           for tt in spec.constants
                                           if tt.name.startswith('keyword_')]) + r')\b',
    'operator':         f'{operator_re}(?!{operator_re})',
    'punctuation':      '|'.join(re.escape(pn) for pn in spec.punctuation_keys),
    'numeric':          r'\b(\d+)\b'
}

for k, v in constants.items():
    print(f'__{k} \\= ({v})\n')
