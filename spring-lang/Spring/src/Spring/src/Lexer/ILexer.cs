namespace JetBrains.ReSharper.Plugins.Spring.Lexer
{
    
    public interface ILexer
    {
        public Token Go(string s, int startPosition);
    }
}