using System.Collections.Generic;
using System.Linq;
using System.Text;
using static PascalLexer.Lexer.IConfiguration;
using static PascalLexer.Lexer.Token;

namespace PascalLexer.Lexer
{
    public class CommentLex : ILexer
    {
        public Token Go(string s, int startPosition)
        {
            var pos = startPosition;
            var len = s.Length;
            
            if (pos >= len) return Fail("Start position is out of string", pos);
            
            Stack<TokenBuilder> stack = new(); // For nested comments

            if (!CommentStartSymbols.Contains(s[pos]))
            {
                return Fail("Not a comment", startPosition, pos);
            }

            if (s[pos] == LineComment)
            {
                if (++pos >= len || s[pos] != LineComment) return Fail("Invalid comment", startPosition, pos);
                stack.Push(new TokenBuilder(s, startPosition, TokenType.CommentLine));
            }
            pos--;
            
            while (++pos < len)
            {
                switch (s[pos])
                {
                    case CommentBraceOpen: // { comment }
                        stack.Push(new TokenBuilder(s, pos, TokenType.Comment));
                        continue;
                    case CommentBiOpen1: // (* comment *)
                        if (++pos >= len) continue;
                        if (s[pos] == CommentBiOpen2) {
                            stack.Push(new TokenBuilder(s, pos - 1, TokenType.CommentBi));
                        }  
                        if (stack.Count == 0) return Fail("Not a comment", startPosition, pos);
                        continue;
                    case CommentBraceClose:
                        if (stack.Count == 0) goto BuildFailReport;
                        if (stack.Peek().Type != TokenType.Comment) continue;
                        var tokB = stack.Pop().Build(pos + 1);
                        if (stack.Count == 0) return tokB;
                        stack.Peek().AddToken(tokB);
                        continue;
                    case NewLine:
                        if (stack.Count == 0) goto BuildFailReport;
                        if (stack.Last().Type != TokenType.CommentLine) continue;
                        return stack.Last().Build(pos + 1);
                    case CommentBiClose1:
                        if (stack.Count == 0) goto BuildFailReport;
                        if (pos + 1 >= len || s[pos + 1] != CommentBiClose2) continue;
                        if (stack.Peek().Type != TokenType.CommentBi) continue;
                        pos++;
                        var tok = stack.Pop().Build(pos + 1);
                        if (stack.Count == 0) return tok;
                        stack.Peek().AddToken(tok);
                        continue;     
                }
            }

            pos = pos < len ? pos : len;
            if (stack.Count > 0 && stack.Last().Type == TokenType.CommentLine) return stack.Last().Build(pos);

            BuildFailReport:
            if (stack.Count == 0) return Fail("Is not a comment", startPosition, pos);
            
            
            var level = 0; 
            var sb = new StringBuilder("Comment error\n");
            foreach (var tok in stack.Reverse())
            {
                sb.Append(" :: Unfinished comment (")
                    .Append(tok.Type)
                    .Append(") at pos ")
                    .Append(tok.StartPosition)
                    .Append(" level ").Append(level++).AppendLine();
            }

            return Fail(sb.ToString(), startPosition, pos);
        }
    }
}