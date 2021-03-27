using System.Linq;

namespace PascalLexer.Lexer
{
    public interface IConfiguration
    {
        private static readonly string[] SpecialPairs =
        {
            "<<", ">>", "**", "<>", "><", "<=", ">=", ":=", "+=", "-=", "*=", "/=", "(*", "*)", "(.", ".)", "//"
        };
        
        public static bool IsLetter(char c) => 'a' <= c && c <= 'z' || 'A' <= c && c <= 'Z';
        public static bool IsDigit(char c) => '0' <= c && c <= '9';
        public static bool IsSpecial(char c) => "+-*/=<>[].,():;^@".Contains(c);
        public static bool IsSpecialPair(char c1, char c2) => SpecialPairs.Any(p => c1 == p[0] && c2 == p[1]);

        public static bool IsHexDit(char c) => '0' <= c && c <= '9' || 'a' <= c && c <= 'f' || 'A' <= c && c <= 'F';
        public static bool IsOctDit(char c) => '0' <= c && c <= '7';
        public static bool IsBinDit(char c) => c == '0' || c == '1';
        public static bool IsSign(char c) => c == '+' || c == '-';
        public static bool IsExpScale(char c) => c == 'e' || c == 'E';
        
        
        public static bool IsCommentBanned(char c) => c == '\n' || c == '\r';
        
        const char Underscore = '_';
        const char ReservedOverrideSymbol = '&';
        const char HexPrefix = '$';
        const char OctPrefix = '&';
        const char BinPrefix = '%';
        const char StringExtraSymPrefix = '#';
        const char RealDelimiter = '.';
        const char Quote = '\'';
        const char LineComment = '/';
        const char CommentBraceOpen = '{';
        const char CommentBraceClose = '}';
        const char CommentBiOpen1 = '(';
        const char CommentBiOpen2 = '*';
        const char CommentBiClose1 = '*';
        const char CommentBiClose2 = ')';
        
        const string WhiteSpaceChars = " \t\n\r";
    }
}