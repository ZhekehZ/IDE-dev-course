using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.TreeBuilder;
using static JetBrains.ReSharper.Plugins.Spring.SpringTokenType;


namespace JetBrains.ReSharper.Plugins.Spring.Parser
{
    class Combinators
    {
        public static void Next(PsiBuilder builder)
        {
            if (!builder.Eof()) builder.AdvanceLexer();
            while (!builder.Eof() && (builder.GetTokenType().IsWhitespace || builder.GetTokenType().IsComment))
            {
                builder.AdvanceLexer();
            }
        }

        public static Func<PsiBuilder, bool> Sym(string tok)
        {
            return Tok(EXTRA_SYM, tok);
        }
        
        public static Func<PsiBuilder, bool> SymPair(string tok)
        {
            return Tok(EXTRA_SYM_PAIR, tok);
        }
        
        public static Func<PsiBuilder, bool> KeyWord(string tok)
        {
            return Tok(KEYWORD, tok);
        }

        public static Func<PsiBuilder, bool> Seq(params Func<PsiBuilder, bool>[] args)
        {
            return builder =>
            {
                var m = builder.Mark();

                if (args.Any(arg => !arg(builder)))
                {
                    builder.Done(m, SpringCompositeNodeType.BLOCK, null);
                    return false;
                }
                
                builder.Done(m, SpringCompositeNodeType.BLOCK, null);
                return true;
            };
        }

        public static Func<PsiBuilder, bool> Alt(params Func<PsiBuilder, bool>[] args)
        {
            return Alt(false, args);
        }
        
        public static Func<PsiBuilder, bool> AltForce(params Func<PsiBuilder, bool>[] args)
        {
            return Alt(true, args);
        }
        
        private static Func<PsiBuilder, bool> Alt(bool forceContinue, IReadOnlyList<Func<PsiBuilder, bool>> args)
        {
            return builder =>
            {
                var initialOffer = builder.GetTokenOffset();
                var m = builder.Mark();

                for (var i = 0; i < args.Count; i++)
                {
                    var arg = args[i];
                    if (arg(builder))
                    {
                        builder.Done(m, SpringCompositeNodeType.BLOCK, null);
                        return true;
                    }

                    if (builder.GetTokenOffset() > initialOffer && forceContinue)
                    {
                        builder.Done(m, SpringCompositeNodeType.BLOCK, null);
                        return false;
                    }

                    if (i != args.Count - 1)
                    {
                        builder.RollbackTo(m);
                        m = builder.Mark();
                    }
                }
                
                builder.Done(m, SpringCompositeNodeType.BLOCK, null);
                return false;
            };
        }
        
        public static Func<PsiBuilder, bool> Opt(Func<PsiBuilder, bool> arg)
        {
            return Opt("", false, arg);
        }
        
        public static Func<PsiBuilder, bool> OptForce(string error, Func<PsiBuilder, bool> arg)
        {
            return Opt(error, true, arg);
        }


        private static Func<PsiBuilder, bool> Opt(string error, bool force, Func<PsiBuilder, bool> arg)
        {
            return builder =>
            {
                var m = builder.Mark();
                var initialOffset = builder.GetTokenOffset();

                if (arg(builder))
                {
                    builder.Drop(m);
                }
                else
                {
                    if (force && initialOffset < builder.GetTokenOffset()) {
                        builder.Error(m, error);
                        return false;
                    }
                    builder.RollbackTo(m);
                }
                
                return true;
            };
        }
        
        public static Func<PsiBuilder, bool> Tok(TokenNodeType tt, string text = null)
        {
            return builder =>
            {
                if (builder.Eof())
                {
                    builder.Error("Unexpected end of file");
                    return false;
                }

                if (tt.Index != builder.GetTokenType().Index)
                {
                    builder.Error("Invalid token type: " + tt + " expected");
                    return false;
                }

                if (text != null)
                {
                    if (builder.GetTokenText().ToLower() != text)
                    {
                        builder.Error(text + " expected , but got " + builder.GetTokenText());
                        return false;
                    }
                }

                Next(builder);
                return true;
            };
        }

        public static Func<PsiBuilder, bool> Many(
            Func<PsiBuilder, bool> arg,
            Func<PsiBuilder, bool> delimiter = null,
            bool atLeastOne = false
        )
        {
            return ManyForce(null, arg, delimiter, atLeastOne);
        }
        
        public static Func<PsiBuilder, bool> ManyForce(
            string message,
            Func<PsiBuilder, bool> arg, 
            Func<PsiBuilder, bool> delimiter = null,
            bool atLeastOne = false
        )
        {
            return builder =>
            {
                var m = builder.Mark();
                var m2 = builder.Mark();
                
                while (true)
                {
                    if (arg(builder))
                    {
                        builder.Drop(m2);
                        m2 = builder.Mark();
                        
                        if (delimiter != null && !delimiter(builder))
                        {
                            builder.RollbackTo(m2);
                            break;
                        }

                        atLeastOne = message != null;
                    }
                    else
                    {
                        if (atLeastOne)
                        {
                            builder.Drop(m2);
                            if (message != null)
                            {
                                // builder.Done(m, SpringCompositeNodeType.BLOCK, null);
                                builder.Error(m, message);
                            }
                            else
                            {
                                builder.Error("At least one arg expected");
                                builder.Done(m, SpringCompositeNodeType.BLOCK, null);
                            }
                            return false;
                        }
                        builder.RollbackTo(m2);
                        break;
                    }
                }
                
                builder.Done(m, SpringCompositeNodeType.BLOCK, null);
                return true;
            };
        }
    }
}
