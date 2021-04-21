using System;
using JetBrains.ReSharper.Psi.TreeBuilder;
using static JetBrains.ReSharper.Plugins.Spring.Parser.Combinators;
using static JetBrains.ReSharper.Plugins.Spring.SpringTokenType;

namespace JetBrains.ReSharper.Plugins.Spring.Parser
{
    public static class Keywords
    {
        public static readonly Parser KW_GOTO = KeyWord("goto");
        public static readonly Parser KW_LABEL = KeyWord("label");
        public static readonly Parser KW_BEGIN = KeyWord("begin");
        public static readonly Parser KW_END = KeyWord("end");
        public static readonly Parser KW_CASE = KeyWord("case");
        public static readonly Parser KW_OF = KeyWord("of");
        public static readonly Parser KW_ELSE = KeyWord("else");
        public static readonly Parser KW_OTHERWISE = KeyWord("otherwise");
        public static readonly Parser KW_IF = KeyWord("if");
        public static readonly Parser KW_THEN = KeyWord("then");
        public static readonly Parser KW_FOR = KeyWord("for");
        public static readonly Parser KW_TO = KeyWord("to");
        public static readonly Parser KW_DOWNTO = KeyWord("downto");
        public static readonly Parser KW_DO = KeyWord("do");
        public static readonly Parser KW_IN = KeyWord("in");
        public static readonly Parser KW_REPEAT = KeyWord("repeat");
        public static readonly Parser KW_UNTIL = KeyWord("until");
        public static readonly Parser KW_WHILE = KeyWord("while");
        public static readonly Parser KW_WITH = KeyWord("with");
        public static readonly Parser KW_RAISE = KeyWord("raise");
        public static readonly Parser KW_TRY = KeyWord("try");
        public static readonly Parser KW_EXCEPT = KeyWord("except");
        public static readonly Parser KW_FINALLY = KeyWord("finally");
        public static readonly Parser KW_ASM = KeyWord("asm");
        public static readonly Parser KW_DIV = KeyWord("div");
        public static readonly Parser KW_MOD = KeyWord("mod");
        public static readonly Parser KW_AND = KeyWord("and");
        public static readonly Parser KW_SHL = KeyWord("shl");
        public static readonly Parser KW_SHR = KeyWord("shr");
        public static readonly Parser KW_AS = KeyWord("as");
        public static readonly Parser KW_OR = KeyWord("or");
        public static readonly Parser KW_XOR = KeyWord("xor");
        public static readonly Parser KW_NIL = KeyWord("nil");
        public static readonly Parser KW_IS = KeyWord("is");
        public static readonly Parser KW_NOT = KeyWord("not");
    }

    public static class OperatorsAndSymbols
    {
        public static readonly Parser O_PLUS = Sym("+");
        public static readonly Parser O_MINUS = Sym("-");
        public static readonly Parser O_MULT = Sym("*");
        public static readonly Parser O_DIV = Sym("/");
        public static readonly Parser O_GT = Sym(">");
        public static readonly Parser O_LT = Sym("<");
        public static readonly Parser O_EQ = Sym("=");
        public static readonly Parser O_SEMIC = Sym(";");
        public static readonly Parser O_COLON = Sym(":");
        public static readonly Parser O_COMMA = Sym(",");
        public static readonly Parser O_POPEN = Sym("(");
        public static readonly Parser O_PCLOSE = Sym(")");
        public static readonly Parser O_BOPEN = Sym("[");
        public static readonly Parser O_BCLOSE = Sym("]");
        public static readonly Parser O_AT = Sym("@");
        public static readonly Parser O_DOT = Sym(".");
        
        public static readonly Parser O_ASSIGN = SymPair(":=");
        public static readonly Parser O_ASG_MUL = SymPair("*=");
        public static readonly Parser O_ASG_DIV = SymPair("/=");
        public static readonly Parser O_ASG_PLS = SymPair("+=");
        public static readonly Parser O_ASG_MNS = SymPair("-=");
        public static readonly Parser O_GE = SymPair(">=");
        public static readonly Parser O_NEQ = SymPair("<>");
        public static readonly Parser O_LE = SymPair("<=");
        public static readonly Parser O_DOTS = SymPair("..");
    }


    public static class Tokens
    {
        public static readonly Parser TOK_INT = Tok(INTEGER);
        public static readonly Parser TOK_REAL = Tok(REAL);
        public static readonly Parser TOK_STR = Tok(STRING);
        public static readonly Parser TOK_IDENT = Tok(IDENTIFIER);
    }
}
