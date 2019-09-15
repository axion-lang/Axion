from aenum import Enum


class ProcessingMode(Enum):
    lex = "Perform lexical analysis on source and generate tokens list from it."
    parse = "Perform lexical analysis on source and generate AST from tokens list."
    interpret = "Do interpretation of source and output result."
    compile = "Compile Axion source into machine code."
    convert_cs = "Convert Axion source into C# source code."
    convert_py = "Convert Axion source into Python source code."
    default = compile

    def __init__(self, description: str):
        self.description = description
