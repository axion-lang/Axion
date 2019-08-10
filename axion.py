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

def main():
    from compiler import Compiler
    from source_unit import SourceUnit

    interactive_mode: bool = False
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
                    continue
                else:
                    break

            if args.clear:
                cli.clear_screen()
                cli.print_greeting()
                continue

            if args.help:
                cli.print_help()
                continue

            if args.interactive:
                interactive_mode = True
                continue

            # interactive commands
            if interactive_mode:
                pass
    else:
        # processing launch arguments
        pass


if __name__ == "__main__":
    main()
