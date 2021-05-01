using static JetBrains.ReSharper.Plugins.Spring.Lexer.IConfiguration;
using static JetBrains.ReSharper.Plugins.Spring.Lexer.Token;

namespace JetBrains.ReSharper.Plugins.Spring.Lexer
{
    public class IdentifierLex : ILexer
    {
        public Token Go(string s, int startPosition)
        {
            var pos = startPosition;
            var len = s.Length;

            if (pos >= len) return Fail("Start position is out of string", pos);
            if (s[pos] == ReservedOverrideSymbol) pos++;
            if (pos >= len || !IsLetter(s[pos]) && s[pos] != Underscore)
            {
                return Fail("Identifier must start with [a-zA-Z_]", startPosition);
            }

            while (++pos < len && (IsLetter(s[pos]) || IsDigit(s[pos]) || s[pos] == Underscore))
            {
            }

            return Success(s, startPosition, pos,
                s[startPosition] == ReservedOverrideSymbol
                    ? TokenType.ReservedIdentifierOverriding
                    : TokenType.Identifier);
        }
    }
}