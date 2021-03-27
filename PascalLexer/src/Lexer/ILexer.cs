
namespace PascalLexer.Lexer
{
    
    public interface ILexer
    {
        public Token Go(string s, int startPosition);
    }
}