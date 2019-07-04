import pathlib
import re
import sys

axion_dir = pathlib.Path(__file__).parent.parent
spec_dir = axion_dir / 'Axion.Core' / 'Specification'


def gen_never_expression_starters():
    region_name = sys._getframe().f_code.co_name

    file_ttype = spec_dir / 'TokenType.cs'
    with file_ttype.open() as f:
        ttype_content = f.read()

    raw_items = re.search(r'TokenType\s*{(.*?)}', ttype_content, re.M | re.S)
    print(raw_items.group(1))
    # write results to file
    file_syntactic = spec_dir / 'Spec.Syntactic.cs'
    with file_syntactic.open() as f:
        syntactic_content = f.read()


gen_never_expression_starters()
