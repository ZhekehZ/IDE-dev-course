namespace PascalLexer.Lexer
{

    public enum TokenType
    {
        ExtraSymbol,
        ExtraSymbolPair,
        Comment,
        String,
        Identifier,
        ReservedIdentifierOverriding,
        Integer,
        Real,
        HexInt,
        OctInt,
        BinInt,
    }
    
    public class Token : ALexerResult<string>
    {
        public override string ErrorMessage { get; }
        public override string Result => Value;
        public override Status Status => StartPosition < 0 ? Status.Fail : Status.Ok;
        public override int StartPosition { get; }
        public override int EndPosition { get; }
        public TokenType Type { get; }
        public override string OriginalString { get; }

        private Token(string originalString, int startPosition, int endPosition, TokenType type)
        {
            StartPosition = startPosition;
            EndPosition = endPosition;
            Type = type;
            OriginalString = originalString;
            ErrorMessage = originalString;
        }

        public static Token Success(string originalString, int startPosition, int endPosition, TokenType type)
        {
            return new(originalString, startPosition, endPosition, type);
        }
        
        public static Token Fail(string errorMessage, int pos)
        {
            return new(errorMessage + " at pos " + pos, -1, pos, 0);
        }

        public override string ToString()
        {
            if (Status == Status.Ok)
            {
                return "Token{ OK, type=" + Type + " range=" + (StartPosition, EndPosition) + " str=" + Value + " }";
            }
            return "Token{ FAIL, pos=" + EndPosition + " message=" + ErrorMessage + " }";
        }
    }
}