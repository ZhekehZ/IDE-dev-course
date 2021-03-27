using static PascalLexer.Lexer.IConfiguration;
using static PascalLexer.Lexer.Token;

namespace PascalLexer.Lexer
{
    public class SymbolLex : ILexer
    {
        public Token Go(string s, int startPosition)
        {
            var pos = startPosition;
            var len = s.Length;
            if (pos >= len) return Fail("Start position is out of string", pos);
            if (pos + 1 < len && IsSpecialPair(s[pos], s[pos + 1])) return Success(s, pos, pos + 2, TokenType.ExtraSymbolPair);
            return IsSpecial(s[pos]) ? Success(s, pos, pos + 1, TokenType.ExtraSymbol) : Fail("Unexpected symbol", pos);
        }
    }
     
}