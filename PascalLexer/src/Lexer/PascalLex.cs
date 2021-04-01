using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using NUnit.Framework;
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
        public override int ParsedPos => Length;

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

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("STATUS = ").Append(Status).Append(" Length = ").Append(Length).AppendLine();
            Result.ForEach(
                tok => sb.Append('\t').Append(tok.ToString("\t")).AppendLine()
            );
            return sb.ToString();
        }
    }
    
    public class PascalLex {
        private static readonly CommentLex LexComment = new();
        private static readonly StringLex LexString = new();
        private static readonly NumberLex LexNumber = new();
        private static readonly IdentifierLex LexIdentifier = new();
        private static readonly SymbolLex LexSymbol = new();

        private readonly List<ILexer> _lexOrder = new()
        {
            LexComment, LexString, LexIdentifier, LexNumber, LexSymbol 
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
                
                foreach (var tok in _lexOrder)
                {
                    var res = tok.Go(s, pos);
                    if (res.Status != Status.Ok)
                    {
                        if (closestPos < res.ParsedPos)
                        {
                            closestPos = res.ParsedPos;
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