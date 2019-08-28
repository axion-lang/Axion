import glob
import inspect
from os.path import dirname, realpath
from pathlib import Path
from typing import Generator

from compiler import Compiler
from processing.mode import ProcessingMode
from source import SourceUnit
from utils import resolve_path, rmdir

tests_ext = '.unit'
tests_dir = Path(dirname(realpath(__file__)))

out_dir = tests_dir / 'files' / 'out'
if not out_dir.exists():
    resolve_path(out_dir)

in_dir = tests_dir / 'files' / 'in'
if not in_dir.exists():
    resolve_path(in_dir)


def clear_output_dir():
    dbg: Path = out_dir / 'debug'
    if dbg.exists():
        rmdir(dbg)
    resolve_path(dbg)


def find_source_files(directory: Path) -> Generator[Path, None, None]:
    return (Path(f) for f in glob.glob(str(directory) + "\\**/*" + Compiler.source_file_ext, recursive = True))


def parse_test_file(test_name: str) -> SourceUnit:
    src = source_from_file(test_name)
    parse(src)
    return src


def source_from_file(file_name: str) -> SourceUnit:
    return SourceUnit.from_file(
        in_dir / (file_name + Compiler.source_file_ext),
        out_dir / (file_name + tests_ext)
    )


def source_from_code(code: str):
    cur_frame = inspect.currentframe()
    cal_frame = inspect.getouterframes(cur_frame, 2)
    out_file_name = cal_frame[1][3]
    return SourceUnit.from_code(
        code,
        out_dir / (out_file_name + tests_ext)
    )


def parse(source: SourceUnit):
    Compiler.process_source(source, ProcessingMode.parsing)
