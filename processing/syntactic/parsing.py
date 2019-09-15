from typing import Union, Callable, Type, Optional, List

import processing.syntactic.expressions.expr as e
import specification as spec
from errors.blame import BlameSeverity
from processing.lexical.tokens.operator import InputSide
from processing.lexical.tokens.token_type import TokenType


def make_parse_fn(
        parse_fn: Union[Callable[[], e.Expr], Type]
) -> Optional[Callable[[], e.Expr]]:
    if isinstance(parse_fn, type):
        expr_type: Type = parse_fn

        def inner(p: e.Expr):
            return expr_type(p).parse()

        return inner
    elif parse_fn is not None:
        return parse_fn
    else:
        return None


def maybe_tuple(parent: e.Expr, exprs: List[e.Expr]) -> e.Expr:
    from processing.syntactic.expressions.tuple_expr import TupleExpr
    if len(exprs) == 1:
        return exprs[0]
    return TupleExpr(parent, expressions = exprs)


def parse_multiple(
        parent: e.Expr,
        obj_to_parse: Union[Callable[[], e.Expr], Type] = None,
        allow_generator = True
) -> e.Expr:
    # region imports
    from processing.syntactic.expressions.for_compr_expr import ForComprehensionExpr
    from processing.syntactic.expressions.generator_expr import GeneratorExpr
    from processing.syntactic.expressions.parenthesized_expr import ParenthesizedExpr
    # endregion
    s = parent.stream

    parse_fn = make_parse_fn(obj_to_parse) or parse_any
    parens = s.maybe_eat(TokenType.open_parenthesis)
    lst = [parse_fn(parent)]  # len(lst) >= 1
    # tuple
    if s.maybe_eat(TokenType.comma):
        while True:
            lst.append(parse_fn(parent))
            if not s.maybe_eat(TokenType.comma):
                break
    elif allow_generator and parens and isinstance(lst[0], ForComprehensionExpr):
        s.eat(TokenType.close_parenthesis)
        return GeneratorExpr(parent, lst[0])
    # Expr.assert_type(lst, InfixExpression)  # TODO
    if parens:
        s.eat(TokenType.close_parenthesis)
        if len(lst) == 1:
            return ParenthesizedExpr(parent, lst[0])
    return maybe_tuple(parent, lst)


def parse_any(parent: e.Expr) -> e.Expr:
    # region imports
    from processing.syntactic.expressions.binary_expr import BinaryExpr
    from processing.syntactic.expressions.definitions.var_def import VarDef
    from processing.syntactic.expressions.type_names import TypeName
    from processing.syntactic.expressions.groups import VarTargetExpression
    from processing.syntactic.expressions.atomic.empty_expr import EmptyExpr
    from processing.syntactic.expressions.atomic.break_expr import BreakExpr
    from processing.syntactic.expressions.atomic.continue_expr import ContinueExpr
    from processing.syntactic.expressions.atomic.return_expr import ReturnExpr
    from processing.syntactic.expressions.atomic.yield_expr import YieldExpr
    from processing.syntactic.expressions.conditional_expr import ConditionalExpr
    from processing.syntactic.expressions.while_expr import WhileExpr
    from processing.syntactic.expressions.definitions.module_def import ModuleDef
    from processing.syntactic.expressions.definitions.class_def import ClassDef
    from processing.syntactic.expressions.definitions.enum_def import EnumDef
    from processing.syntactic.expressions.definitions.func_def import FuncDef
    from processing.syntactic.expressions.block_expr import BlockExpr, BlockType
    # endregion
    s = parent.stream

    if s.peek_is(TokenType.semicolon, TokenType.keyword_pass):
        return EmptyExpr(parent).parse()
    elif s.peek_is(TokenType.keyword_break):
        return BreakExpr(parent).parse()
    elif s.peek_is(TokenType.keyword_continue):
        return ContinueExpr(parent).parse()
    elif s.peek_is(TokenType.keyword_return):
        return ReturnExpr(parent).parse()
    elif s.peek_is(TokenType.keyword_yield):
        return YieldExpr(parent).parse()
    elif s.peek_is(TokenType.keyword_if):
        return ConditionalExpr(parent).parse()
    elif s.peek_is(TokenType.keyword_while):
        return WhileExpr(parent).parse()
    elif s.peek_is(TokenType.keyword_module):
        return ModuleDef(parent).parse()
    elif s.peek_is(TokenType.keyword_class):
        return ClassDef(parent).parse()
    elif s.peek_is(TokenType.keyword_enum):
        return EnumDef(parent).parse()
    elif s.peek_is(TokenType.keyword_fn):
        return FuncDef(parent).parse()
    elif s.peek_is(TokenType.indent, TokenType.open_brace, TokenType.colon) \
            and parent.ast.macro_expect_type is not None \
            and issubclass(BlockExpr, parent.ast.macro_expect_type):
        return BlockExpr(parent).parse(BlockType.default)

    immutable = s.maybe_eat(TokenType.keyword_let)
    let_token = s.token if immutable else None

    exp = parse_infix(parent)
    if isinstance(exp, BinaryExpr) \
            and exp.operator.of_type(TokenType.op_assign) \
            and exp.left.of_type(VarTargetExpression):
        if exp.get_parent_of_type(BlockExpr).is_defined(exp.left):
            return exp
        return VarDef(
            parent,
            let_token = let_token,
            name = exp.left,
            equals_token = exp.operator,
            value = exp.right
        )
    if not immutable and not s.maybe_eat(TokenType.colon):
        return exp
    if not isinstance(exp, VarTargetExpression):
        parent.source.blame('cannot use this expression as variable target', exp, BlameSeverity.error)
        return exp

    colon_token = s.token if s.token.of_type(TokenType.colon) else None
    var_type = TypeName(parent).parse()

    assign_token = None
    var_value = None
    if s.maybe_eat(TokenType.op_assign):
        assign_token = s.token
        var_value = parse_infix(parent)
    return VarDef(
        parent,
        let_token = let_token,
        name = exp,
        colon_token = colon_token,
        value_type = var_type,
        equals_token = assign_token,
        value = var_value
    )


def parse_infix(parent: e.Expr, precedence = 0) -> e.Expr:
    # region imports
    from processing.syntactic.expressions.binary_expr import BinaryExpr
    from processing.syntactic.expressions.conditional_infix_expr import ConditionalInfixExpr
    from processing.syntactic.expressions.macro_application import MacroApplication
    from processing.syntactic.expressions.groups import DefinitionExpression
    from processing.syntactic.expressions.for_compr_expr import ForComprehensionExpr
    # endregion
    s = parent.stream

    left_e = parse_prefix(parent)
    if isinstance(left_e, DefinitionExpression):
        return left_e

    macro = MacroApplication(parent).parse_infix_macro(left_e)
    if macro.macro_definition is not None:
        return macro

    while True:
        from processing.lexical.tokens.operator import OperatorToken
        if isinstance(s.peek, OperatorToken):
            new_precedence = s.peek.precedence
            s.peek.input_side = InputSide.both
        elif not s.token.of_type(TokenType.newline, TokenType.outdent) \
                and s.peek_is(TokenType.identifier):
            new_precedence = 4
        else:
            break
        if new_precedence < precedence:
            break
        s.eat_any()
        left_e = BinaryExpr(
            parent,
            left_e,
            s.token,
            parse_infix(parent, new_precedence + 1)
        )
    if not s.token.of_type(TokenType.newline, TokenType.outdent):
        if s.peek_is(TokenType.keyword_for):
            left_e = ForComprehensionExpr(parent, left_e).parse()
        if s.peek_is(TokenType.keyword_if, TokenType.keyword_unless):
            left_e = ConditionalInfixExpr(parent, true_expression = left_e).parse()
    return left_e


def parse_prefix(parent: e.Expr) -> e.Expr:
    from processing.syntactic.expressions.unary_expr import UnaryExpr
    s = parent.stream

    if s.maybe_eat(*spec.prefix_operators):
        from processing.lexical.tokens.operator import OperatorToken
        operator: OperatorToken = s.token
        operator.input_side = InputSide.right
        return UnaryExpr(parent, operator, parse_prefix(parent))
    return parse_postfix(parent)


def parse_postfix(parent: e.Expr) -> e.Expr:
    # region imports
    from processing.syntactic.expressions.member_access_expr import MemberAccessExpr
    from processing.syntactic.expressions.func_call_expr import FuncCallExpr
    from processing.syntactic.expressions.func_call_expr import FuncCallArg
    from processing.syntactic.expressions.unary_expr import UnaryExpr
    from processing.syntactic.expressions.indexer_expr import IndexerExpr
    from processing.syntactic.expressions.groups import DefinitionExpression
    from processing.syntactic.expressions.atomic.constant_expr import ConstantExpr
    # endregion
    s = parent.stream

    def parse_tight_postfix(result: e.Expr) -> e.Expr:
        while True:
            if s.peek_is(TokenType.op_dot):
                result = MemberAccessExpr(parent, target = result).parse()
            elif s.peek_is(TokenType.open_parenthesis) and not isinstance(result, ConstantExpr):
                result = FuncCallExpr(parent, target = result).parse(allow_generator = True)
            elif s.peek_is(TokenType.open_bracket):
                result = IndexerExpr(parent, target = result).parse()
            else:
                break
        if s.maybe_eat(TokenType.op_increment, TokenType.op_decrement):
            from processing.lexical.tokens.operator import OperatorToken
            operator: OperatorToken = result.stream.token
            operator.input_side = InputSide.right
            result = UnaryExpr(parent, operator, result)
        return result

    exp = parse_atom(parent)
    if isinstance(exp, DefinitionExpression):
        return exp
    if s.maybe_eat(TokenType.right_pipeline):
        while True:
            exp = FuncCallExpr(
                parent,
                parse_tight_postfix(parse_atom(parent)),
                args = [FuncCallArg(parent, value = exp)]
            )
            if not s.maybe_eat(TokenType.right_pipeline):
                break
    return parse_tight_postfix(exp)


def parse_atom(parent: e.Expr) -> e.Expr:
    # region imports
    from processing.syntactic.expressions.invalid_expr import InvalidExpr
    from processing.syntactic.expressions.atomic.await_expr import AwaitExpr
    from processing.syntactic.expressions.atomic.code_quote import CodeQuoteExpr
    from processing.syntactic.expressions.atomic.constant_expr import ConstantExpr
    from processing.syntactic.expressions.atomic.name_expr import NameExpr
    from processing.syntactic.expressions.tuple_expr import TupleExpr
    from processing.syntactic.expressions.definitions.func_def import FuncDef
    # endregion
    s = parent.stream

    if s.peek_is(TokenType.identifier):
        return NameExpr(parent).parse(must_be_simple = True)
    elif s.peek_is(*spec.constants):
        return ConstantExpr(parent).parse()
    elif s.peek_is(TokenType.keyword_await):
        return AwaitExpr(parent).parse()
    elif s.peek_is(TokenType.open_double_brace):
        return CodeQuoteExpr(parent).parse()
    elif s.peek_is(TokenType.open_parenthesis):
        if s.peek_by_is(2, TokenType.close_parenthesis):
            return TupleExpr(parent).parse_empty()
        return parse_multiple(parent)
    elif s.peek_is(TokenType.keyword_fn):
        return FuncDef(parent).parse(is_anonymous = True)
    else:
        from processing.syntactic.expressions.macro_application import MacroApplication
        macro = MacroApplication(parent).parse_macro()
        if macro.macro_definition is not None:
            return macro
    return InvalidExpr(parent).parse()
