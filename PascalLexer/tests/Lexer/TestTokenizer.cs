using System;
using System.Linq;
using NUnit.Framework;
using PascalLexer.Lexer;

namespace PascalLexer.tests.Lexer
{
    [TestFixture]
    public class TestTokenizer
    {
        private static PascalLex<CommentLex, StringLex, NumberLex, IdentifierLex, SymbolLex> Tok => new();

        [Test]
        public void TestPositiveFull()
        {
            (string, string[])[] cases =
            {
                ("{ comment }   identifier'string'",
                    new[]{  "Token{ OK, type=Comment range=(0, 11) str={ comment } }",
                            "Token{ OK, type=Identifier range=(14, 24) str=identifier }",
                            "Token{ OK, type=String range=(24, 32) str='string' }"
                    }),
                ("{ comment }#1#2#3&34",
                    new[]{  "Token{ OK, type=Comment range=(0, 11) str={ comment } }",
                            "Token{ OK, type=String range=(11, 17) str=#1#2#3 }",
                            "Token{ OK, type=OctInt range=(17, 20) str=&34 }"
                    }),
                ("program pg+\n" +
                 "procedure &do+ // Overriding 'do' word",
                    new[]{  "Token{ OK, type=Identifier range=(0, 7) str=program }",
                            "Token{ OK, type=Identifier range=(8, 10) str=pg }",
                            "Token{ OK, type=ExtraSymbol range=(10, 11) str=+ }",
                            "Token{ OK, type=Identifier range=(12, 21) str=procedure }",
                            "Token{ OK, type=ReservedIdentifierOverriding range=(22, 25) str=&do }",
                            "Token{ OK, type=ExtraSymbol range=(25, 26) str=+ }",
                            "Token{ OK, type=Comment range=(27, 50) str=// Overriding 'do' word }"
                    }),
                ("  'this''is''a c'#0'm_plex string' 'And other string'",
                    new[]{  "Token{ OK, type=String range=(2, 34) str='this''is''a c'#0'm_plex string' }",
                            "Token{ OK, type=String range=(35, 53) str='And other string' }"
                    }),
                ("&11&procedure (* This is number &11 redefined identifier procedure *) ",
                    new[]{  "Token{ OK, type=OctInt range=(0, 3) str=&11 }",
                            "Token{ OK, type=ReservedIdentifierOverriding range=(3, 13) str=&procedure }",
                            "Token{ OK, type=Comment range=(14, 69) str=(* This is number &11 redefined identifier procedure *) }"
                    }),
                ("11 + 12 <> 13",
                    new[]{  "Token{ OK, type=Integer range=(0, 2) str=11 }",
                            "Token{ OK, type=ExtraSymbol range=(3, 4) str=+ }",
                            "Token{ OK, type=Integer range=(5, 7) str=12 }",
                            "Token{ OK, type=ExtraSymbolPair range=(8, 10) str=<> }",
                            "Token{ OK, type=Integer range=(11, 13) str=13 }",
                    }),
                ("1 1e1 1.1 +1 +1.2e2",
                    new[]{  "Token{ OK, type=Integer range=(0, 1) str=1 }",
                            "Token{ OK, type=Real range=(2, 5) str=1e1 }",
                            "Token{ OK, type=Real range=(6, 9) str=1.1 }",
                            "Token{ OK, type=Integer range=(10, 12) str=+1 }",
                            "Token{ OK, type=Real range=(13, 19) str=+1.2e2 }",
                    }),
            };

            foreach (var (str, expected) in cases)
            {
                Assert.AreEqual(expected, Tok.Go(str, 0).Result.Select(x => x.ToString()).ToArray());
            }
        }
        
        [Test]
        public void TestNegative()
        {
            (string, string[], string, int)[] cases =
            {
                ("{ comment }  штэ??", new[]{"Token{ OK, type=Comment range=(0, 11) str={ comment } }" },
                    "Invalid sequence at pos 13", 13),
                ("123 123.", new[]{"Token{ OK, type=Integer range=(0, 3) str=123 }"}, 
                    "There must be at least one digit after the . at pos 8", 4),
                ("{unfinished} { comment", new[]{"Token{ OK, type=Comment range=(0, 12) str={unfinished} }"}, 
                    "Unclosed comment '{' at pos 14", 13),
            };

            foreach (var (str, expected, msg, pos) in cases)
            {
                var res = Tok.Go(str, 0);
                Assert.AreEqual(expected, res.Result.Select(x => x.ToString()).ToArray());
                Assert.AreEqual(Status.Fail, res.Status);
                Assert.AreEqual(msg, res.ErrorMessage);
                Assert.AreEqual(pos, res.EndPosition);
            }
        }
    }
}