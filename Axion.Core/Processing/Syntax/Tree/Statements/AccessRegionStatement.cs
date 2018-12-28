using System;
using Axion.Core.Processing.Lexical.Tokens;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class AccessRegionStatement : Statement {
        [JsonProperty]
        public TokenType Modifier { get; set; }

        [JsonProperty]
        public Statement Region { get; set; }

        public AccessRegionStatement(Token accessMod, Statement region) {
            Modifier = accessMod?.Type ?? throw new ArgumentNullException(nameof(accessMod));
            Region   = region ?? throw new ArgumentNullException(nameof(region));

            MarkStart(accessMod);
            MarkEnd(Region);
        }
    }
}