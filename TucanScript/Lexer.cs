using System.Collections.Generic;
using System.Text;

namespace TucanScript
{
static class Lexer
    {
        public static bool IsOperator(this char source, string allTokens, int index)
        {
            switch (source)
            {
                case '=' :
                case '*' :
                case '/' :
                case '%' :
                case '+' :
                case '-' :
                case '>' :
                case '<' :
                case '|' :
                case '&' :
                case '(' :
                case ')' :
                case '{' :
                case '}' :
                case ';' :
                case ',' :
                case '\'' :
                    return true;
                case '.' :
                    if (index >= allTokens.Length)
                    {
                        return true;
                    }
                    
                    var next = allTokens[index + 1];
                    return !char.IsNumber(next);
            }

            return false;
        }

        public static string[] Separate(string source)
        {
            var tokenStringBuilder = new StringBuilder();
            var tokenList = new List<string>();

            for (var tokenIndex = 0; tokenIndex < source.Length; tokenIndex++)
            {
                var sym = source[tokenIndex];
                var token = tokenStringBuilder.ToString();
                if (sym == Shell.TokenSeparator)
                {
                    if (token.Length > 0)
                    {
                        tokenList.Add(token);
                    }

                    tokenStringBuilder.Clear();
                    continue;
                }

                if (sym.IsOperator(source, tokenIndex))
                {
                    if (token.Length > 0)
                    {
                        tokenList.Add(token);
                    }

                    tokenStringBuilder.Clear();
                    tokenList.Add(sym.ToString());
                    continue;
                }

                if (!char.IsControl(sym))
                {
                    tokenStringBuilder.Append(sym.ToString());
                }
            }

            if (tokenStringBuilder.Length > 0)
            {
                tokenList.Add(tokenStringBuilder.ToString());
            }

            return tokenList.ToArray();
        }
    }
}