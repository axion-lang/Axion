using System.Globalization;
using System.Linq;
using Axion.Enums;
using Axion.Processing.Tokens;

namespace Axion.Processing {
   public static class Lexer {
      /// <summary>
      ///    'End of file' mark.
      /// </summary>
      private const char eof = '\uffff';

      private static string[] lines;

      /// <summary>
      ///    Current character in source line.
      ///    <para />
      ///    Always update this field's value through <see cref="AdvanceChar" /> method.
      /// </summary>
      private static char c;

      /// <summary>
      ///    Current line index in <see cref="lines"/>.
      ///    <para />
      ///    This field's value is updated automatically through <see cref="AdvanceChar" /> method.
      /// </summary>
      private static int lnI;

      /// <summary>
      ///    Column index in current processing source line.
      ///    <para />
      ///    This field's value is updated automatically through <see cref="AdvanceChar" /> method.
      /// </summary>
      private static int clI;

      /// <summary>
      ///    Previous source code line indentation length.
      /// </summary>
      private static int lastIndentLength;

      private static Chain<Token> tokens;

      /// <summary>
      ///    Splits characters stream into tokens chain (linked list).
      /// </summary>
      public static void Tokenize(string[] sourceLines, Chain<Token> tokensChain) {
         // remove carriage returns (in Windows),
         // append newline statement
         lines = sourceLines;
         for (int i = 0; i < lines.Length - 1; i++) {
            lines[i] += "\n";
         }
         // append end of file mark to last source line.
         if (lines.Length > 0) {
            lines[lines.Length - 1] += eof;
         }
         tokens = tokensChain;
         for (lnI = 0; lnI < lines.Length; lnI++) {
            while (lnI < lines.Length && clI < lines[lnI].Length) {
               c = lines[lnI][clI];
               Token token = MakeNextToken();
               if (token != null) {
                  tokens.AddLast(token);
               }
               AdvanceChar();
            }
         }

         // reset all values to work with next file
         c = '\0';
         clI = 0;
         lnI = 0;
         lastIndentLength = 0;
      }

      private static Token MakeNextToken() {
         while (true) {
            // end of file
            if (c == eof) {
               return new Token(TokenID.EndOfFile, "", lnI, clI);
            }

            // newline
            else if (c == '\n') {
               // don't create multiple newline tokens
               // and don't add newline as first token.
               if (tokens.Count >= 1 && tokens.Last.Value.ID != TokenID.Newline) {
                  return new Token(TokenID.Newline, "\n", lnI, clI);
               }
               return null;
            }

            // whitespaces are skipped
            else if (c == ' ' || c == '\t') {
               AdvanceChar();
            }

            // indentation
            else if (tokens.Count > 0 && tokens.Last.Value.ID == TokenID.Newline
                     && (c == ' ' || c == '\t')) {
               int indentLength = 0;
               // count indentation
               while (true) {
                  if (c == ' ') {
                     indentLength++;
                  }
                  else if (c == '\t') {
                     indentLength += 8 - indentLength % 8;
                  }
                  else {
                     break;
                  }
                  if (PeekChar() == c) {
                     AdvanceChar();
                  }
               }

               // skip indentation if line is blank or commented
               Token[] next = PeekNextTokens(1);
               if (next.Length > 1 &&
                   // next token is multiline comment
                   (next[0].Value.StartsWith("/*") &&
                   // check that comment is not INline
                   (next[1].ID == TokenID.Newline || next[1].ID == TokenID.Comment)
                   // or next token is one-line comment
                   || next[0].Value.StartsWith("//"))) {
                  return null;
               }
               Token indentationToken = null;
               // indent increased
               if (indentLength > lastIndentLength) {
                  indentationToken = new Token(TokenID.Indent, "", lnI, 0) { EndClPos = indentLength - lastIndentLength };
                  lastIndentLength = indentLength;
               }
               // indent decreased

               if (indentLength < lastIndentLength) {
                  indentationToken = new Token(TokenID.Outdent, "", lnI, 0) { EndClPos = lastIndentLength - indentLength };
                  lastIndentLength = indentLength;
               }
               return indentationToken;
            }

            // comment
            else if (c == '/' &&
                    (PeekChar() == '/' || PeekChar() == '*')) {
               int startLn = lnI;
               int startCl = clI;
               string comment = "/";
               AdvanceChar();
               // "/*" - multiline comment
               if (c == '*') {
                  // multiline comment level variable.
                  // (support for nested multiline comments)
                  int mlcLevel = 1;
                  while (true) {
                     if (c == eof) {
                        throw new ProcessingException(
                           "Multiline comment went through end of file",
                           ErrorOrigin.Lexer,
                           lnI, clI);
                     }

                     comment += c;
                     AdvanceChar();
                     // found nested multiline comment
                     if (c == '/') {
                        comment += c;
                        AdvanceChar();
                        if (c == '*') {
                           // increase indentation level
                           mlcLevel++;
                           comment += c;
                           AdvanceChar();
                        }
                     }
                     else if (c == '*') {
                        comment += c;
                        AdvanceChar();
                        if (c == '/') {
                           comment += c;
                           mlcLevel--;
                           if (mlcLevel == 0) {
                              // found final comment end "*/"
                              break;
                           }
                           AdvanceChar();
                        }
                     }
                  }
               }
               // "//" - one-line comment
               else {
                  comment += "/";
                  // until end of line or file
                  while (true) {
                     char p = PeekChar();
                     if (p == '\n' || p == eof) {
                        break;
                     }
                     comment += p;
                     AdvanceChar();
                  }
               }
               return new Token(TokenID.Comment, comment,
                                startLn, startCl);
            }

            // string
            else if (c == '"' || c == '\'' || c == '`' ||
                     // check for prefixes
                     (c == 'f' && Specification.StringQuotes.Contains(PeekChar()))) {
               //bool strFprefix = false;
               if (c == 'f') {
                  //strFprefix = true;
                  AdvanceChar();
               }
               char firstQuote = c;
               string delimiter = c.ToString();
               AdvanceChar();
               // add next 2 quotes for multiline strings
               while (c == firstQuote && delimiter.Length < 3) {
                  delimiter += c;
                  // if got "" or '' or ``
                  if (delimiter.Length == 2 && PeekChar() != firstQuote) {
                     return new Token(TokenID.String, "", clI - 2, lnI);
                  }
                  AdvanceChar();
               }
               // TODO add formatted string processing
               int stringStartLine = lnI;
               // "c" now is a first string's character
               // that's why we use "- 1 - delimiter.Length".
               int stringStartColumn = clI - delimiter.Length;
               string str = c.ToString();

               while (true) {
                  string nextChars = "";
                  char charBeforeDelimiter = c;
                  // get next piece of string
                  for (int i = 0; i < delimiter.Length; i++) {
                     nextChars += c;
                     AdvanceChar();
                  }
                  // compare with non-escaped delimiter
                  if (nextChars == delimiter &&
                      charBeforeDelimiter != '\\') {
                     return new Token(TokenID.String, str, stringStartLine, stringStartColumn);
                  }
                  // if not matched, check for end of line/file
                  if (c == '\n' && delimiter.Length == 1 || c == eof) {
                     throw new ProcessingException(
                        "Unclosed string",
                        ErrorOrigin.Lexer,
                        lnI, clI);
                  }
                  str += nextChars;
               }
            }

            // number
            else if (char.IsDigit(c)) {
               int startCl = clI;
               // build value
               string numStr = c.ToString();
               while (true) {
                  char p = PeekChar();
                  if (!char.IsLetterOrDigit(p)) {
                     break;
                  }
                  numStr += p;
                  AdvanceChar();
               }
               string numValue = char.IsDigit(numStr[numStr.Length - 1])
                  ? numStr
                  : numStr.Remove(numStr.Length - 1);

               // TODO add check if number not in it's type values range

               #region Float number

               if (numStr.Contains(".")) {
                  // float
                  if (!numStr.EndsWith("l") &&
                      float.TryParse(numValue, out var float32)) {
                     return new NumberToken(NumberType.Float32,
                                            float32.ToString(CultureInfo.InvariantCulture),
                                            lnI, startCl);
                  }
                  // long float
                  if (double.TryParse(numValue, out var float64)) {
                     return new NumberToken(NumberType.Float64,
                                            float64.ToString(CultureInfo.InvariantCulture),
                                            lnI, startCl);
                  }
               }

               #endregion

               #region Integer

               else {
                  // byte
                  if (numStr.EndsWith("b") &&
                      byte.TryParse(numValue, out var int8)) {
                     return new NumberToken(NumberType.Byte, int8.ToString(),
                                            lnI, startCl);
                  }
                  // short int
                  if (numStr.EndsWith("s") &&
                      short.TryParse(numValue, out var int16)) {
                     return new NumberToken(NumberType.Int16, int16.ToString(),
                                            lnI, startCl);
                  }
                  // integer
                  if (numStr == numValue &&
                      int.TryParse(numValue, out var int32)) {
                     return new NumberToken(NumberType.Int32, int32.ToString(),
                                            lnI, startCl);
                  }
                  // long int
                  if (long.TryParse(numValue, out var int64)) {
                     return new NumberToken(NumberType.Int64, int64.ToString(),
                                            lnI, startCl);
                  }
               }

               #endregion

               throw new ProcessingException(
                  $"Invalid number: '{numStr}'",
                  ErrorOrigin.Lexer,
                  lnI, startCl);
            }

            // identifier
            else if (char.IsLetter(c) || c == '_') {
               // "c " now is a first word's letter
               int wordStartColumn = clI;
               string word = c.ToString();
               while (true) {
                  char next = PeekChar();
                  if (!char.IsLetterOrDigit(next) && next != '_') {
                     break;
                  }
                  word += next;
                  AdvanceChar();
               }
               // TODO IS LANGUAGE CASE-SENSITIVE?
               word = word.ToLower();
               TokenID id;
               // check for literal operators 'and', 'or', 'in'...
               if (Specification.BoolOperators.Contains(word)) id = TokenID.Operator;
               else if (Specification.BuiltInTypes.Contains(word)) id = TokenID.BuiltIn;
               else if (Specification.Keywords.Contains(word)) id = TokenID.Keyword;
               else id = TokenID.Identifier;

               return new Token(id, word, lnI, wordStartColumn);
            }

            // operator
            else if (Specification.OperatorChars.Contains(c)) {
               int operatorStartColumn = clI;
               string op = c.ToString();
               // when c is operating sign and
               // op + c is valid operator
               while (true) {
                  char p = PeekChar();
                  if (p == '\n' || p == eof
                      || !Specification.OperatorChars.Contains(c)
                      || !(op == "" || Specification.Operators.ContainsKey(op + p))) {
                     break;
                  }
                  op += char.ToLower(p);
                  AdvanceChar();
               }

               if (Specification.Operators.TryGetValue(op, out Operator @operator)) {
                  return new OperatorToken(@operator, lnI, operatorStartColumn);
               }
               throw new ProcessingException(
                  $"Invalid operator: '{op}'",
                  ErrorOrigin.Lexer,
                  lnI, operatorStartColumn);
            }

            // invalid
            else {
               throw new ProcessingException(
                  $"Invalid symbol: '{c}'",
                  ErrorOrigin.Lexer,
                  lnI, clI);
            }
         }
      }

      private static Token[] PeekNextTokens(uint count) {
         if (count == 0) {
            return new Token[0];
         }
         // save values
         char b_c = c;
         int b_lnI = lnI;
         int b_clI = clI;
         int b_lastIndentLength = lastIndentLength;
         // get next tokens
         var nextTokens = new Token[count];
         for (int i = 0; i < count; i++) {
            nextTokens[i] = MakeNextToken();
         }
         // restore values
         c = b_c;
         lnI = b_lnI;
         clI = b_clI;
         lastIndentLength = b_lastIndentLength;
         return nextTokens.Where(t => t != null).ToArray();
      }

      private static char PeekChar(int position = 1) {
         return clI + position >= lines[lnI].Length
            ? '\n'
            : lines[lnI][clI + position];
      }

      /// <summary>
      ///    Advances <see cref="c" /> value to next character in line,
      ///    then updates <see cref="clI" /> and <see cref="lnI" />.
      /// </summary>
      private static void AdvanceChar(uint position = 1) {
         for (int i = 0; i < position; i++) {
            if (c == '\n') {
               lnI++;
               clI = 0;
               if (lnI < lines.Length) {
                  // check if next line is empty
                  if (lines[lnI].Length == 0) {
                     c = '\n';
                  }
                  else {
                     c = lines[lnI][clI];
                  }
               }
               else {
                  c = eof;
               }
            }
            else {
               clI++;
               c = clI < lines[lnI].Length
                  ? lines[lnI][clI]
                  : '\n';
            }
         }
      }
   }
}