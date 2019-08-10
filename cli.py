import os
from argparse import ArgumentParser
from typing import List

import colorama
from tabulate import tabulate
from termcolor import colored

help_text = tabulate(
    tablefmt = 'fancy_grid',
    headers = ("Short", "Full", "Usage description"),
    tabular_data = [
        ['-i', '--interactive', "Launch compiler's interactive interpreter mode"],
        ['-c', '--code "<code>"', "Input code to process"],
        ['-f', '--files "<path>"', "Input files to process"],
        ['-p', '--project "<path>"', "Input Axion project to process [N/A]"],
        ["-m", "--mode <value>", "Launch compiler's interactive interpreter mode"],
        ["", "--interactive", "Source code processing mode (Default: compile) Available:"],
        ['', 'lex', ":    Create tokens (lexemes) list from source"],
        ['', 'parse', ":    Create tokens list and Abstract Syntax Tree from source"],
        ['', 'interpret', ":    Interpret source code"],
        ['', 'convertPy', ":    Convert source to 'Python' language"],
        ['-d', '--debug', "Save debug information to '<compilerDir>\\output' directory"],
        ['-j', '--astJson', "Show resulting AST in JSON format in the console"],
        ['', '--cls, --clear', "Clear terminal screen"],
        ['-h', '--help', "Display this help screen"],
        ['-x', '--exit', "Exit the compiler"],
    ]
)


def print_greeting():
    import compiler
    print(
        colored('Axion', 'green'), 'programming language compiler toolset v.',
        colored(compiler.Compiler.version, 'green')
    )
    print('Working in', colored(compiler.Compiler.compiler_dir, 'green'))
    print('Type', colored('-h', 'green'), 'to get documentation about launch arguments.')
    print()


def print_help():
    print(help_text)


def request_args() -> List[str]:
    return input(">>> ").split()


def clear_screen():
    # for windows
    if os.name == 'nt':
        os.system('cls')
    # for mac and linux (os.name is 'posix')
    else:
        os.system('clear')


parser = ArgumentParser(add_help = False)

# enable colors in CLI
colorama.init()

# region Parser arguments creation

parser.add_argument(
    '-c',
    '--code',
    nargs = '+'
)
parser.add_argument(
    '-f',
    '--files',
    action = 'append',
    default = []
)
parser.add_argument(
    '--cls',
    '--clear',
    dest = 'clear',
    action = 'store_true',
    default = False
)
parser.add_argument(
    '-i',
    '--interactive',
    action = 'store_true',
    default = False
)
parser.add_argument(
    '-d',
    '--debug',
    action = 'store_true',
    default = False
)
parser.add_argument(
    '-h',
    '--help',
    action = 'store_true',
    default = False
)
parser.add_argument(
    '-x',
    '--exit',
    dest = 'exit',
    action = 'store_true',
    default = False
)

# endregion
