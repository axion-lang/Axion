# import fileinput
import re
from pathlib import Path

project_path = Path(__file__).parent.parent
token_type_filename = str(project_path.absolute()) + r"\Axion.Core\Tokens\TokenType.cs"

generation_start_pattern = r'\s*#region\s+GENERATION_(\w+)\s*'
generation_end_pattern = r'\s*#endregion\s+GENERATION_(\w+)\s*'
keyword_def_pattern = r"\s*Keyword(.+?)(\s+)(.+?),"
operator_def_pattern = r"\s*Op(.+?)(\s+)(.+?),"

indent = " " * 8
keywords_region = "keywords"
operators_region = "operators"
keywords = []
operators = []

# start
match_start = current_region = current_def_pattern = ""


def process_def():
    global match_def
    if current_region == keywords_region:
        match_def = re.match(keyword_def_pattern, line)
        if match_def:
            keywords.append(match_def.group(1))
    elif current_region == operators_region:
        match_def = re.match(operator_def_pattern, line)
        if match_def:
            operators.append(match_def.group(1))


# fileinput.input(token_type_filename, inplace=1):
for line in open(token_type_filename):
    match_end = re.match(generation_end_pattern, line)
    if match_end and match_end.group(1) == current_region:
        match_start = ""
        current_region = ""
    if match_start:
        # do replacement
        match_def = ""
        process_def()
    else:
        match_start = re.match(generation_start_pattern, line)
        if match_start:
            current_region = match_start.group(1)
            process_def()

print(keywords)
print(operators)
