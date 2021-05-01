using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.TreeBuilder;
using static JetBrains.ReSharper.Plugins.Spring.SpringCompositeNodeType;
using static JetBrains.ReSharper.Plugins.Spring.SpringTokenType;


namespace JetBrains.ReSharper.Plugins.Spring.Parser
{
    public readonly struct ParserResult
    {
        public readonly bool Ok;
        public readonly int Length;

        public ParserResult(bool ok, int length)
        {
            Ok = ok;
            Length = length;
        }
    }

    public delegate ParserResult Parser(PsiBuilder builder);

    internal static class Combinators
    {
        public static void Next(PsiBuilder builder)
        {
            if (!builder.Eof()) builder.AdvanceLexer();
            SkipCommentsAndWhitespaces(builder);
        }

        public static void SkipCommentsAndWhitespaces(PsiBuilder builder)
        {
            while (!builder.Eof() && (builder.GetTokenType().IsWhitespace || builder.GetTokenType().IsComment))
            {
                builder.AdvanceLexer();
            }
        }

        public static Parser Sym(string tok) => Tok(EXTRA_SYM, tok);
        public static Parser SymPair(string tok) => Tok(EXTRA_SYM_PAIR, tok);
        public static Parser KeyWord(string tok) => Tok(KEYWORD, tok);

        public static Parser Seq(params Parser[] elems)
        {
            return builder =>
            {
                var start = builder.Mark();
                var initialOffset = builder.GetTokenOffset();

                if (elems.Any(elem => !elem(builder).Ok))
                {
                    builder.Done(start, BLOCK, null);
                    return new ParserResult(false, builder.GetTokenOffset() - initialOffset);
                }

                builder.Done(start, BLOCK, null);
                return new ParserResult(true, builder.GetTokenOffset() - initialOffset);
            };
        }

        public static Parser Mark(SpringCompositeNodeType tokenType, Parser parser)
        {
            return builder =>
            {
                var m = builder.Mark();
                var res = parser(builder);
                builder.Done(m, tokenType, null);
                return res;
            };
        }

        public static Parser Alt(params Parser[] alternatives) => Alt(null, false, alternatives);
        public static Parser AltStrong(params Parser[] alternatives) => Alt(null, true, alternatives);
        public static Parser AltStrong(string err, params Parser[] alternatives) => Alt(err, true, alternatives);

        private static Parser Alt(string err, bool strong, IReadOnlyList<Parser> alternatives)
        {
            return builder =>
            {
                var start = builder.Mark();

                var best = 0;
                var bestLen = 0;

                for (var i = 0; i < alternatives.Count; i++)
                {
                    var result = alternatives[i](builder);

                    if (result.Ok || strong && result.Length > 0)
                    {
                        builder.Done(start, BLOCK, null);
                        return result;
                    }

                    if (bestLen < builder.GetTokenOffset())
                    {
                        bestLen = builder.GetTokenOffset();
                        best = i;
                    }

                    builder.RollbackTo(start);
                    start = builder.Mark();
                }

                var bestRes = alternatives[best](builder);
                if (err != null)
                    builder.Error(start, err);
                else
                    builder.Done(start, BLOCK, null);
                return bestRes;
            };
        }

        public static Parser Opt(Parser arg) => Opt("", false, arg);
        public static Parser OptStrong(string error, Parser arg) => Opt(error, true, arg);


        private static Parser Opt(string error, bool strong, Parser arg)
        {
            return builder =>
            {
                var start = builder.Mark();

                var result = arg(builder); // try to parse
                if (result.Ok)
                {
                    builder.Drop(start); // remove mark
                    return result;
                }

                if (strong && result.Length > 0)
                {
                    // if strong and at least one token parsed
                    builder.Error(start, error);
                    return result;
                }

                builder.RollbackTo(start); // if failed and not strong, rollback changes
                return new ParserResult(true, 0);
            };
        }

        public static Parser Tok(
            TokenNodeType tt,
            string text = null,
            SpringCompositeNodeType type = null
        )
        {
            return builder =>
            {
                if (builder.Eof())
                {
                    builder.Error("Unexpected end of file");
                    return new ParserResult(false, 0);
                }


                if (tt != builder.GetTokenType())
                {
                    builder.Error((text ?? tt.ToString()) + " expected + got " + builder.GetTokenType());
                    return new ParserResult(false, 0);
                }

                if (text != null)
                {
                    if (builder.GetTokenText().ToLower() != text)
                    {
                        builder.Error(text + " expected " + " got " + builder.GetTokenText().ToLower());
                        return new ParserResult(false, 0);
                    }
                }

                var initialOffset = builder.GetTokenOffset();
                var marker = builder.Mark();

                builder.AdvanceLexer();
                builder.Done(marker, type ?? BLOCK, null);
                SkipCommentsAndWhitespaces(builder);

                return new ParserResult(true, builder.GetTokenOffset() - initialOffset);
            };
        }

        public static Parser Many(
            Parser elem,
            Parser delimiter = null,
            bool atLeastOne = false,
            bool strong = false,
            bool strongDelim = false,
            string err = null
        )
        {
            return builder =>
            {
                var start = builder.Mark();
                var afterLastElement = builder.Mark();
                var initialOffset = builder.GetTokenOffset();

                var expectElement = atLeastOne;

                while (true)
                {
                    var result = elem(builder); // parse item

                    if (result.Ok) // if success
                    {
                        builder.Drop(afterLastElement); // re-create mark
                        afterLastElement = builder.Mark();

                        var beforeDelimiter = builder.Mark();
                        if (delimiter != null && !delimiter(builder).Ok) // if next is not delimiter
                        {
                            builder.RollbackTo(beforeDelimiter); // rollback delimiter parsing state
                            builder.Drop(afterLastElement);
                            break; // go to finish
                        }

                        builder.Drop(beforeDelimiter); // delimiter parsed successfully

                        expectElement = strongDelim; // if message is set, then element is expected
                        atLeastOne = false;
                    }
                    else // if failed
                    {
                        if (strong && result.Length > 0)
                            builder.Done(afterLastElement, BLOCK, null);
                        else
                            builder.RollbackTo(afterLastElement); // rollback to last good position

                        if (expectElement || atLeastOne)
                        {
                            if (atLeastOne)
                            {
                                builder.Error(start, "At least one element expected");
                                return result;
                            }

                            builder.Error(start, err ?? "Error");
                            return result;
                        }

                        if (strong && result.Length > 0)
                        {
                            builder.Error(start, "At least one element expected");
                            return result;
                        }

                        // if next token is not an element and it's ok
                        break;
                    }
                }

                builder.Done(start, BLOCK, null);
                return new ParserResult(true, builder.GetTokenOffset() - initialOffset);
            };
        }
    }
}