import logging
import sys
from pathlib import Path

import colorlog

import cli

# region Initializing logger

formatter = colorlog.ColoredFormatter(
    "%(log_color)s%(message)s%(reset)s",
    log_colors = {
        'DEBUG':    'cyan',
        'INFO':     'green',
        'WARNING':  'yellow',
        'ERROR':    'red',
        'CRITICAL': 'red,bg_white',
    }
)

handler = colorlog.StreamHandler(sys.stdout)
handler.setFormatter(formatter)

logger = colorlog.getLogger(__name__)
logger.setLevel(logging.DEBUG)
logger.addHandler(handler)

# endregion

interactive_mode = False


def main():
    if len(sys.argv) < 2:
        # no arguments
        cli.print_greeting()
        while True:
            args_list = cli.request_args()
            try:
                args = cli.parser.parse_args(args_list)
            except SystemExit:
                # arg parser automatically exits program when he's done.
                continue
            end = process_args(args)
            if end:
                break
    else:
        # processing launch arguments
        pass


def process_args(args) -> bool:
    from compiler import Compiler
    from source import SourceUnit
    global interactive_mode

    if len(args.files) > 0:
        if args.files[0].startswith('"'):
            args.files[0] = args.files[0][1:-1]
        Compiler.process_source(SourceUnit.from_file(Path(args.files[0])))

    if args.code:
        args.code = ' '.join(args.code)[1:-1]
        # TODO: processing mode, options
        Compiler.process_source(SourceUnit.from_code(args.code))

    if args.exit:
        if interactive_mode:
            interactive_mode = False
            return False
        else:
            return True

    if args.clear:
        cli.clear_screen()
        cli.print_greeting()
        return False

    if args.help:
        cli.print_help()
        return False

    if args.interactive:
        interactive_mode = True
        return False

    # interactive commands
    if interactive_mode:
        pass


if __name__ == "__main__":
    main()
