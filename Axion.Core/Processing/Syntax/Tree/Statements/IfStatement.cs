using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class IfStatement : Statement {
        [JsonProperty]
        public IfStatementBranch[] Branches { get; }

        [JsonProperty]
        public Statement Else { get; }

        public IfStatement(List<IfStatementBranch> branches, Statement elseBody) {
            if (branches.Count == 0) {
                throw new ArgumentException("Value cannot be an empty collection.", nameof(branches));
            }

            Branches = branches.ToArray();
            Else     = elseBody;

            MarkStart(branches[0]);
            MarkEnd(Else ?? Branches[Branches.Length - 1]);
        }
    }
}