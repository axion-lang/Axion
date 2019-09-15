from __future__ import annotations

from typing import List, Set, Optional, Collection

import specification as spec
from errors.blame import BlameType, BlameSeverity
from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token import Token
from processing.lexical.tokens.token_type import TokenType
from processing.syntactic.expressions.atomic.name_expr import NameExpr
from processing.syntactic.expressions.atomic.return_expr import ReturnExpr
from processing.syntactic.expressions.block_expr import BlockExpr, BlockType
from processing.syntactic.expressions.definitions.name_def import NameDef
from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.groups import DefinitionExpression, AtomExpression
from processing.syntactic.expressions.type_names import TypeName


class FuncParameter(NameDef):
    """ func_parameter:
        ID ':' type ['=' infix_expr];
    """

    def __init__(
            self,
            parent: Expr,
            name: NameExpr = None,
            colon_token: Token = None,
            value_type: TypeName = None,
            equals_token: Token = None,
            value: Expr = None
    ):
        super().__init__(parent)
        self.name = name
        self.colon_token = colon_token
        self.value_type = value_type
        self.equals_token = equals_token
        self.default_value = self.value = value

    def __str__(self):
        return str(self.name)

    # noinspection PyMethodOverriding
    def parse(self, names: Set[str]) -> FuncParameter:
        super().parse()

        def check_uniqueness(name):
            if str(name) in names:
                self.source.blame(BlameType.duplicated_parameter_in_function, name)
            names.add(str(name))

        if isinstance(self.name, Collection):
            for n in self.name:
                check_uniqueness(n)
        else:
            check_uniqueness(self.name)
        if self.value_type is None and self.default_value is None:
            self.source.blame(BlameType.impossible_to_infer_type, self)
        return self


class FuncDef(DefinitionExpression, AtomExpression):
    """ func_def:
        'fn' name ['(' [parameters_list] ')'] ['=>' type] block;
    """

    @child_property
    def name(self) -> NameExpr:
        pass

    @child_property
    def return_type(self) -> TypeName:
        pass

    @child_property
    def parameters(self) -> List[Expr]:
        pass

    @child_property
    def block(self) -> BlockExpr:
        pass

    @child_property
    def modifiers(self) -> List[Expr]:
        pass

    @property
    def value_type(self) -> Optional[TypeName]:
        if self.return_type is not None:
            return self.return_type
        returns = self.block.find_items_of_type(ReturnExpr)
        if len(returns) > 0:
            return returns[0].value_type
        else:
            return None

    def __init__(
            self,
            parent: Expr = None,
            fn_token: Token = None,
            name: NameExpr = None,
            parameters: List[Expr] = None,
            arrow_token: Token = None,
            return_type: TypeName = None,
            block: BlockExpr = None,
            modifiers: List[Expr] = None
    ):
        super().__init__(parent)
        self.fn_token = fn_token
        self.name = name
        self.parameters = parameters
        self.arrow_token = arrow_token
        self.return_type = return_type
        self.block = block
        self.modifiers = modifiers

    def parse(self, is_anonymous = False) -> FuncDef:
        self.fn_token = self.stream.eat(TokenType.keyword_fn)
        if not is_anonymous and self.stream.peek.of_type(TokenType.identifier):
            self.name = NameExpr(self).parse()
        if self.stream.maybe_eat(TokenType.open_parenthesis):
            self.parameters = FuncDef.parse_param_list(self, TokenType.close_parenthesis)
            self.stream.eat(TokenType.close_parenthesis)
        else:
            self.parameters = FuncDef.parse_param_list(self, *(spec.block_start_types + [TokenType.right_arrow]))
        if self.stream.maybe_eat(TokenType.right_arrow):
            self.arrow_token = self.stream.token
            self.return_type = TypeName(self).parse()
        self.block = BlockExpr(self).parse(BlockType.anon_fn if is_anonymous else BlockType.default)
        return self

    @staticmethod
    def parse_param_list(parent: Expr, *terminators: TokenType) -> List[Expr]:
        """ parameter_list:
            {named_parameter ","}
            ( "*" [parameter] ("," named_parameter)* ["," "**" parameter]
            | "**" parameter
            | named_parameter[","] )
        """
        # TODO rewrite syntax for fn parameters
        s = parent.stream
        parameters: List[FuncParameter] = []
        if s.peek_is(*terminators):
            return parameters
        names: Set[str] = set()
        list_parameter: Optional[FuncParameter] = None
        map_parameter: Optional[FuncParameter] = None
        have_keyword_only_parameter = False
        got_star = False
        require_optionals = False
        while True:
            if s.maybe_eat(TokenType.op_power):
                map_parameter = FuncParameter(parent).parse(names)
                s.eat(*terminators)
                break
            elif s.maybe_eat(TokenType.op_multiply):
                if list_parameter is not None:
                    parent.source.blame(
                        BlameType.cannot_have_more_than_1_list_parameter,
                        s.peek
                    )
                if s.peek_is(TokenType.comma):
                    # '*,' - end of positional params mark.
                    got_star = True
                else:
                    list_parameter = FuncParameter(parent).parse(names)
            else:
                if got_star:
                    param = FuncParameter(parent).parse(names)
                    have_keyword_only_parameter = True
                else:
                    param = FuncParameter(parent).parse(names)
                if param.default_value is not None:
                    require_optionals = True
                elif require_optionals and param.default_value is None:
                    param.source.blame(
                        BlameType.expected_default_parameter_value,
                        param
                    )
                parameters.append(param)
            if s.peek_is(*terminators) or not s.eat(TokenType.comma):
                break
        if got_star \
                and list_parameter is None \
                and map_parameter is not None \
                and not have_keyword_only_parameter:
            parent.source.blame(
                "Named arguments must follow '*'",
                map_parameter,
                BlameSeverity.error
            )
        if list_parameter is not None:
            parameters.append(list_parameter)
        if map_parameter is not None:
            parameters.append(map_parameter)
        return parameters

    def to_axion(self, c: CodeBuilder):
        c += self.fn_token, self.name
        if len(self.parameters) > 0:
            c += ' (', self.parameters, ')'
        c += self.arrow_token, self.return_type, self.block

    def to_csharp(self, c: CodeBuilder):
        if self.name is None:
            c += '(', self.parameters, ') => ', self.block
        else:
            c += 'public ', self.value_type, ' ', self.name, '(', self.parameters, ')', self.block

    def to_python(self, c: CodeBuilder):
        c += 'def ', self.name, '(', self.parameters, ')'
        if self.return_type:
            c += ' -> ', self.return_type
        c += self.block
