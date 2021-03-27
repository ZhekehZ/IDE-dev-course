using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using static PascalLexer.Lexer.IConfiguration;

namespace PascalLexer.Lexer
{
    using static Token;

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class PascalLexResult : ALexerResult<List<Token>>
    {
        public override int StartPosition { get; }
        public override int EndPosition { get; }
        public override string OriginalString { get; }
        public override string ErrorMessage { get; }
        public override List<Token> Result { get; }
        public override Status Status { get; }
        
        public PascalLexResult(
            string originalString, 
            List<Token> tokens, 
            Status status, 
            string message, 
            int startPosition, 
            int endPosition)
        {
            OriginalString = originalString;
            Result = tokens;
            Status = status;
            ErrorMessage = message;
            StartPosition = startPosition;
            EndPosition = endPosition;
        }
    }
    
    public class PascalLex<
        TComment, TString, TNumber, TIdentifier, TSymbol   
    > 
        where TComment : ILexer, new()
        where TString  : ILexer, new()
        where TNumber : ILexer, new()
        where TIdentifier : ILexer, new()
        where TSymbol : ILexer, new()
    {
        private readonly ILexer[] _tokOrder =
        {
            new TComment(),
            new TString(),
            new TNumber(),
            new TIdentifier(),
            new TSymbol()
        };
        
        public PascalLexResult Go(string s, int startPosition)
        {
            var pos = startPosition;
            var len = s.Length;
            
            List<Token> result = new();
            NextToken: while (pos < len)
            {
                var closestRes = Fail("Invalid sequence", pos);
                var closestPos = pos;
                
                foreach (var tok in _tokOrder)
                {
                    var res = tok.Go(s, pos);
                    if (res.Status != Status.Ok)
                    {
                        if (closestPos < res.EndPosition)
                        {
                            closestPos = res.EndPosition;
                            closestRes = res;
                        }
                        continue;
                    }
                    result.Add(res);
                    pos = res.NextPosition;
                    goto NextToken;
                }

                var resW = GoWhiteSpace(s, pos);
                if (resW.Status != Status.Ok)
                {
                    return new PascalLexResult(s, result, Status.Fail, closestRes?.ErrorMessage,
                            startPosition, pos);
                }
                pos = resW.EndPosition;
            }
            
            return new PascalLexResult(s, result, Status.Ok, "", startPosition, pos);
        }
        
        private static Token GoWhiteSpace(string s, int startPosition)
        {
            var pos = startPosition;
            var len = s.Length;
            while (pos < len && WhiteSpaceChars.Contains(s[pos])) pos++;
            return pos == startPosition ? Fail("Unknown symbol " + s[pos], pos) : Success(s, startPosition, pos, 0);
        }
    }

}