from unittest import TestCase

from testing.helpers import parse_test_file


class SyntaxTests(TestCase):
    def test_bisect_algorithm(self):
        src = parse_test_file(self.test_bisect_algorithm.__name__)
        self.assertEqual(0, len(src.blames))

    def test_huffman_compression(self):
        src = parse_test_file(self.test_huffman_compression.__name__)
        self.assertEqual(0, len(src.blames))

    def test_fail_huffman_compression(self):
        src = parse_test_file(self.test_fail_huffman_compression.__name__)
        self.assertEqual(len(src.blames), 31)

    def test_console_2048(self):
        src = parse_test_file(self.test_console_2048.__name__)
        self.assertEqual(0, len(src.blames))
