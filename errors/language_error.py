import errors.blame
import processing.text_location


class LanguageError(Exception):
    def __init__(self, message: str, severity: errors.blame.BlameSeverity, span: processing.text_location.Span):
        self.message = message
        self.severity = severity
        self.span = span

    def __repr__(self):
        return f"{self.severity.name.capitalize()}: {self.message}, from {self.span.start} to {self.span.end}"
