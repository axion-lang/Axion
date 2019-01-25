using System;
using System.Collections.Generic;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class TypeNameExpression : Expression {
        public Expression Expression { get; }

        public TypeNameExpression Generic { get; }

        public TypeNameExpression Union { get; }

        public bool IsUnion;
        public bool IsGeneric;
        public bool IsSimple;

        private List<TypeNameExpression> tuple;

        public List<TypeNameExpression> Tuple {
            get {
                if (tuple != null) {
                    return tuple;
                }
                throw new InvalidOperationException("Cannot get tuple argument for non-tuple type.");
            }
            set => tuple = value;
        }

        public TypeNameExpression(
            NameExpression           name,
            TypeNameExpression       generic = null,
            TypeNameExpression       union   = null,
            List<TypeNameExpression> tuple   = null
        ) {
            Expression = name;
            Generic    = generic;
            Union      = union;
            Tuple      = tuple;

            IsUnion   = Union != null;
            IsGeneric = Generic != null;
            IsSimple  = Generic == null && Union == null;
        }
    }
}