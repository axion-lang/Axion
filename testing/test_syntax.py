from errors.blame import BlameType
from testing.helpers import parse_test_file


def test_bisect_algorithm():
    src = parse_test_file()
    assert len(src.blames) == 0


def test_huffman_compression():
    src = parse_test_file()
    assert len(src.blames) == 0


def test_fail_huffman_compression():
    src = parse_test_file()
    assert len(src.blames) == 32


def test_console_2048():
    src = parse_test_file()
    assert len(src.blames) == 0


def test_decision_tree():
    src = parse_test_file()
    for e in src.blames:
        assert e.message == BlameType.impossible_to_infer_type.description


def test_fast_fibonacci():
    src = parse_test_file()
    assert len(src.blames) == 0
