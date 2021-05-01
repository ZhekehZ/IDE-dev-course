namespace JetBrains.ReSharper.Plugins.Spring.Lexer
{
    public class WhitespaceLex : ILexer
    {
        public Token Go(string s, int startPosition)
        {
            var pos = startPosition;
            var len = s.Length;

            while (pos < len && IConfiguration.WhiteSpaceChars.Contains(s[pos])) ++pos;
            return pos == startPosition
                ? Token.Fail("Empty string", pos)
                : Token.Success(s, startPosition, pos, TokenType.White);
        }
    }
}