from termcolor import cprint

import processing.location
from errors.blame import BlameSeverity


class LanguageError(Exception):
    def __init__(self, message: str, severity: BlameSeverity, span: processing.location.Span):
        self.message = message
        self.severity = severity
        self.span = span

    def print(self):
        if self.severity == BlameSeverity.error:
            cprint(self.severity.name, 'red', end = '')
        elif self.severity == BlameSeverity.warning:
            cprint(self.severity.name, 'yellow', end = '')
        elif self.severity == BlameSeverity.info:
            cprint(self.severity.name, 'cyan', end = '')
        # TODO: print path relative to project root.
        print(': ' + self.message)
        cprint('--> ', 'cyan', end = '')
        print(self.span.source.source_path, end = '')
        cprint(':', 'cyan', end = '')
        print(self.span.start.line, end = '')
        cprint(':', 'cyan', end = '')
        print(self.span.start.column)

    def __repr__(self):
        return f"{self.severity.name.capitalize()}: {self.message}, from {self.span.start} to {self.span.end}"
