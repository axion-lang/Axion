from testing.helpers import parse_test_file


def test_bisect_algorithm():
    src = parse_test_file(test_bisect_algorithm.__name__)
    assert len(src.blames) == 0


def test_huffman_compression():
    src = parse_test_file(test_huffman_compression.__name__)
    assert len(src.blames) == 0


def test_fail_huffman_compression():
    src = parse_test_file(test_fail_huffman_compression.__name__)
    assert len(src.blames) == 31


def test_console_2048():
    src = parse_test_file(test_console_2048.__name__)
    assert len(src.blames) == 0
