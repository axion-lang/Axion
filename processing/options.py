from aenum import Flag, unique, auto


@unique
class ProcessingOptions(Flag):
    default = auto()
    check_indentation_consistency = auto()
    debug_tokens = auto()
    debug_ast = auto()
