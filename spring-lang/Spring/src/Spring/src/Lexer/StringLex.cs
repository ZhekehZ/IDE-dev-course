using static JetBrains.ReSharper.Plugins.Spring.Lexer.IConfiguration;
using static JetBrains.ReSharper.Plugins.Spring.Lexer.Token;

namespace JetBrains.ReSharper.Plugins.Spring.Lexer
{
    public class StringLex : ILexer
    {
        public Token Go(string s, int startPosition)
        {
            var pos = startPosition;
            var len = s.Length;

            if (pos >= len) return Fail("Start position is out of string", pos);

            while (pos < len)
            {
                switch (s[pos])
                {
                    case Quote:
                        pos++;
                        while (pos < len && s[pos] != Quote && !IsCommentBanned(s[pos])) pos++;
                        if (pos >= len || s[pos] != Quote) return Fail("Unclosed quote", startPosition);
                        pos++;
                        break;
                    case StringExtraSymPrefix:
                        var startUint = ++pos;
                        while (pos < len && IsDigit(s[pos])) pos++;
                        if (pos == startUint) return Fail("An unsigned integer expected", pos);
                        break;
                    default:
                        goto EndPoint;
                }
            }

            EndPoint:
            return pos == startPosition
                ? Fail("Not a string", startPosition)
                : Success(s, startPosition, pos, TokenType.String);
        }
    }
}