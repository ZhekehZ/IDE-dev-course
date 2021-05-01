using System.Collections.Generic;
using System.Text;

namespace JetBrains.ReSharper.Plugins.Spring.Lexer
{
    public enum TokenType
    {
        ExtraSymbol, // + * = ...
        ExtraSymbolPair, // += ++ ...
        Comment, // { comment }
        CommentBi, // (* comment *)
        CommentLine, // // comment
        String, // 'abc'#12'def'
        Identifier, // abc
        ReservedIdentifierOverriding, // &program
        Integer, // 123
        Real, // 123.456e789
        HexInt, // $beef
        OctInt, // &777
        BinInt, // %101
        White, // \t\n \r 
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
        public override int ParsedPos { get; }

        public readonly List<Token> InnerTokens;

        private Token(
            string originalString,
            int startPosition,
            int endPosition,
            TokenType type,
            List<Token> innerTokens = null,
            int parsedPos = -1)
        {
            StartPosition = startPosition;
            EndPosition = endPosition;
            Type = type;
            OriginalString = originalString;
            ErrorMessage = originalString;
            InnerTokens = innerTokens ?? new List<Token>();
            ParsedPos = parsedPos < 0 ? endPosition : parsedPos;
        }

        public static Token Success(
            string originalString,
            int startPosition,
            int endPosition,
            TokenType type,
            List<Token> innerTokens = null)
        {
            return new(originalString, startPosition, endPosition, type, innerTokens);
        }

        public static Token Fail(string errorMessage, int pos, int parsedPos = -1)
        {
            parsedPos = parsedPos < 0 ? pos : parsedPos;
            return new(errorMessage + " at pos " + pos, -1, pos, 0, null, parsedPos);
        }

        public override string ToString()
        {
            return ToString("");
        }

        public string ToString(string prefix)
        {
            if (Status != Status.Ok) return "Token{ FAIL, pos=" + EndPosition + " message=" + ErrorMessage + " }";

            var str = "Token{ OK, type=" + Type + " range=" + (StartPosition, EndPosition) + " str=" + Value + " }";
            if (InnerTokens.Count == 0) return str;
            var sb = new StringBuilder(str).AppendLine().Append(prefix + "Inner tokens:");
            InnerTokens.ForEach(t => sb.AppendLine().Append(prefix + '\t').Append(t.ToString(prefix + '\t')));
            return sb.ToString();
        }
    }

    public class TokenBuilder
    {
        private readonly string _originalString;
        public readonly int StartPosition;
        public readonly TokenType Type;
        public readonly List<Token> InnerTokens = new();

        public TokenBuilder(string originalString, int startPosition, TokenType type)
        {
            _originalString = originalString;
            StartPosition = startPosition;
            Type = type;
        }

        public void AddToken(Token tok)
        {
            InnerTokens.Add(tok);
        }

        public Token Build(int endPosition)
        {
            return Token.Success(_originalString, StartPosition, endPosition, Type, InnerTokens);
        }
    }
}