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

            switch (s[pos])
            {
                case CommentBraceOpen: // { comment }
                    while (++pos < len && s[pos] != CommentBraceClose) {}
                    return pos == len ? Fail("Unclosed comment '{'", startPosition + 1) 
                                      : Success(s, startPosition, pos + 1, TokenType.Comment);
                case CommentBiOpen1: // (* comment *)
                    if (++pos >= len || s[pos] != CommentBiOpen2) return Fail("Invalid comment '(*'", pos);
                    for (;;) { // // comment
                        while (++pos < len && s[pos] != CommentBiClose1) {}
                        if (pos + 1 >= len) return Fail("Unclosed comment '(*", startPosition + 1);
                        if (s[pos + 1] == CommentBiClose2) return Success(s, startPosition, pos + 2, TokenType.Comment);
                    }
                case LineComment:
                    if (++pos >= len || s[pos] != LineComment) return Fail("Is not a comment '//'", startPosition);
                    while (++pos < len && s[pos] != '\n') {}
                    if (pos < len) pos++;
                    return Success(s, startPosition, pos, TokenType.Comment);
            }

            return Fail("Is not a comment", startPosition);
        }
    }
}