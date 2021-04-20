using System;
using JetBrains.ReSharper.Psi.TreeBuilder;
using static JetBrains.ReSharper.Plugins.Spring.Parser.Combinators;
using static JetBrains.ReSharper.Plugins.Spring.SpringTokenType;

namespace JetBrains.ReSharper.Plugins.Spring.Parser
{
    public class Keywords
    {
        public static Func<PsiBuilder, bool> KwGoto = KeyWord("goto");
        public static Func<PsiBuilder, bool> KwLabel = KeyWord("label");
        public static Func<PsiBuilder, bool> KwBegin = KeyWord("begin");
        public static Func<PsiBuilder, bool> KwEnd = KeyWord("end");
        public static Func<PsiBuilder, bool> KwCase = KeyWord("case");
        public static Func<PsiBuilder, bool> KwOf = KeyWord("of");
        public static Func<PsiBuilder, bool> KwElse = KeyWord("else");
        public static Func<PsiBuilder, bool> KwOtherwise = KeyWord("otherwise");
        public static Func<PsiBuilder, bool> KwIf = KeyWord("if");
        public static Func<PsiBuilder, bool> KwThen = KeyWord("then");
        public static Func<PsiBuilder, bool> KwFor = KeyWord("for");
        public static Func<PsiBuilder, bool> KwTo = KeyWord("to");
        public static Func<PsiBuilder, bool> KwDownto = KeyWord("downto");
        public static Func<PsiBuilder, bool> KwDo = KeyWord("do");
        public static Func<PsiBuilder, bool> KwIn = KeyWord("in");
        public static Func<PsiBuilder, bool> KwRepeat = KeyWord("repeat");
        public static Func<PsiBuilder, bool> KwUntil = KeyWord("until");
        public static Func<PsiBuilder, bool> KwWhile = KeyWord("while");
        public static Func<PsiBuilder, bool> KwWith = KeyWord("with");
        public static Func<PsiBuilder, bool> KwRaise = KeyWord("raise");
        public static Func<PsiBuilder, bool> KwTry = KeyWord("try");
        public static Func<PsiBuilder, bool> KwExcept = KeyWord("except");
        public static Func<PsiBuilder, bool> KwFinally = KeyWord("finally");
        public static Func<PsiBuilder, bool> KwAsm = KeyWord("asm");
        public static Func<PsiBuilder, bool> KwDiv = KeyWord("div");
        public static Func<PsiBuilder, bool> KwMod = KeyWord("mod");
        public static Func<PsiBuilder, bool> KwAnd = KeyWord("and");
        public static Func<PsiBuilder, bool> KwShl = KeyWord("shl");
        public static Func<PsiBuilder, bool> KwShr = KeyWord("shr");
        public static Func<PsiBuilder, bool> KwAs = KeyWord("as");
        public static Func<PsiBuilder, bool> KwOr = KeyWord("or");
        public static Func<PsiBuilder, bool> KwXor = KeyWord("xor");
        public static Func<PsiBuilder, bool> KwNil = KeyWord("nil");
        public static Func<PsiBuilder, bool> KwIs = KeyWord("is");
        public static Func<PsiBuilder, bool> KwNot = KeyWord("not");
    }

    class OperatorsAndSymbols
    {
        public static Func<PsiBuilder, bool> OPlus = Sym("+");
        public static Func<PsiBuilder, bool> OMinus = Sym("-");
        public static Func<PsiBuilder, bool> OMult = Sym("*");
        public static Func<PsiBuilder, bool> ODiv = Sym("/");
        public static Func<PsiBuilder, bool> OGt = Sym(">");
        public static Func<PsiBuilder, bool> OLt = Sym("<");
        public static Func<PsiBuilder, bool> OEq = Sym("=");
        public static Func<PsiBuilder, bool> OSemic = Sym(";");
        public static Func<PsiBuilder, bool> OColon = Sym(":");
        public static Func<PsiBuilder, bool> OComma = Sym(",");
        public static Func<PsiBuilder, bool> OPOpen = Sym("(");
        public static Func<PsiBuilder, bool> OPClose = Sym(")");
        public static Func<PsiBuilder, bool> OBOpen = Sym("[");
        public static Func<PsiBuilder, bool> OBClose = Sym("]");
        public static Func<PsiBuilder, bool> OAt = Sym("@");
        
        public static Func<PsiBuilder, bool> OAssign = SymPair(":=");
        public static Func<PsiBuilder, bool> OAsgMul = SymPair("*=");
        public static Func<PsiBuilder, bool> OAsgDiv = SymPair("/=");
        public static Func<PsiBuilder, bool> OAsgPls = SymPair("+=");
        public static Func<PsiBuilder, bool> OAsgMns = SymPair("-=");
        public static Func<PsiBuilder, bool> OGe = SymPair(">=");
        public static Func<PsiBuilder, bool> ONeq = SymPair("<>");
        public static Func<PsiBuilder, bool> OLe = SymPair("<=");
        public static Func<PsiBuilder, bool> ODots = SymPair("..");
    }


    class Tokens
    {
        public static Func<PsiBuilder, bool> TInt = Tok(INTEGER);
        public static Func<PsiBuilder, bool> TReal = Tok(REAL);
        public static Func<PsiBuilder, bool> TStr = Tok(STRING);
        public static Func<PsiBuilder, bool> TIdent = Tok(IDENTIFIER);
    }
}
