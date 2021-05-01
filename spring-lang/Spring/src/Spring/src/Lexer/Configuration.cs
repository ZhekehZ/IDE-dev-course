using System.Collections.Generic;
using System.Linq;

namespace JetBrains.ReSharper.Plugins.Spring.Lexer
{
    public class IConfiguration
    {
        private static readonly string[] SpecialPairs =
        {
            "<<", ">>", "**", "<>", "><", "<=", ">=", ":=", "+=", "-=", "*=", "/=", "(*", "*)", "(.", ".)", "//", ".."
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

        public const char Underscore = '_';
        public const char ReservedOverrideSymbol = '&';
        public const char HexPrefix = '$';
        public const char OctPrefix = '&';
        public const char BinPrefix = '%';
        public const char StringExtraSymPrefix = '#';
        public const char RealDelimiter = '.';
        public const char Quote = '\'';
        public const char LineComment = '/';
        public const char CommentBraceOpen = '{';
        public const char CommentBraceClose = '}';
        public const char CommentBiOpen1 = '(';
        public const char CommentBiOpen2 = '*';
        public const char CommentBiClose1 = '*';
        public const char CommentBiClose2 = ')';
        public const char NewLine = '\n';

        public const string CommentStartSymbols = "({/";
        public const string WhiteSpaceChars = " \t\n\r";

        public static readonly List<string> Keywords = new()
        {
            "goto", "label", "begin", "end", "case", "of", "else", "otherwise", "if", "then",
            "for", "to", "downto", "do", "in", "repeat", "until", "while", "with", "raise",
            "try", "except", "finally", "asm", "div", "mod", "and", "shl", "shr", "as", "or",
            "xor", "nil", "not", "is", "procedure", "forward", "array", "const", "var", "out",
            "absolute", "export", "cvar", "external", "name"
        };
    }
}