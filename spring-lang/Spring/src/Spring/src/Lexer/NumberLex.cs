using static JetBrains.ReSharper.Plugins.Spring.Lexer.IConfiguration;
using static JetBrains.ReSharper.Plugins.Spring.Lexer.Token;

namespace JetBrains.ReSharper.Plugins.Spring.Lexer
{
    public class NumberLex : ILexer
    {
        public Token Go(string s, int startPosition)
        {
            var pos = startPosition;
            var len = s.Length;

            if (pos >= len) return Fail("Start position is out of string", pos);

            if (IsSign(s[pos]))
            {
                if (++pos >= len) return Fail("There must be numbers after the sign", pos);
            }

            var startUnsigned = pos + 1;
            var tokenType = TokenType.Integer;
            switch (s[pos])
            {
                case HexPrefix: // Hexadecimal
                    while (++pos < len && IsHexDit(s[pos]))
                    {
                    }

                    tokenType = TokenType.HexInt;
                    goto switch_result;
                case OctPrefix: // Octal
                    while (++pos < len && IsOctDit(s[pos]))
                    {
                    }

                    tokenType = TokenType.OctInt;
                    goto switch_result;
                case BinPrefix: // Binary
                    tokenType = TokenType.BinInt;
                    while (++pos < len && IsBinDit(s[pos]))
                    {
                    }

                    switch_result:
                    return pos == startUnsigned
                        ? Fail("The number must contain at least one digit", startUnsigned)
                        : Success(s, startPosition, pos, tokenType);
            }

            var startDigitSequence = pos;
            while (pos < len && IsDigit(s[pos])) pos++; // Digit sequence
            if (pos == startDigitSequence)
                return Fail("The number must contain at least one digit",
                    startDigitSequence); // Empty number

            if (pos >= len) return Success(s, startPosition, pos, TokenType.Integer);

            if (s[pos] == RealDelimiter) // Real part 
            {
                var startSequence = ++pos;
                while (pos < len && IsDigit(s[pos])) pos++;
                if (startSequence >= pos)
                    return Success(s, startPosition, startSequence - 1, TokenType.Integer);
                tokenType = TokenType.Real;
            }

            if (pos >= len) return Success(s, startPosition, pos, tokenType);

            if (IsExpScale(s[pos])) // Exponential part
            {
                pos++;
                if (pos < len && IsSign(s[pos])) pos++;

                var startSequence = pos;
                while (pos < len && IsDigit(s[pos])) pos++;
                if (startSequence == pos)
                    return Fail("There must be at least one digit after the " + s[startSequence - 1], startSequence);
                tokenType = TokenType.Real;
            }

            return pos == startPosition ? Fail("Not a number", pos) : Success(s, startPosition, pos, tokenType);
        }
    }
}