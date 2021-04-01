using System;
using System.Linq;
using NUnit.Framework;
using PascalLexer.Lexer;

namespace PascalLexer.tests.Lexer
{
    [TestFixture]
    public class TestTokenizer
    {
        private static PascalLex Tok => new();

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
                            "Token{ OK, type=CommentLine range=(27, 50) str=// Overriding 'do' word }"
                    }),
                ("  'this''is''a c'#0'm_plex string' 'And other string'",
                    new[]{  "Token{ OK, type=String range=(2, 34) str='this''is''a c'#0'm_plex string' }",
                            "Token{ OK, type=String range=(35, 53) str='And other string' }"
                    }),
                ("&11&procedure (* This is number &11 redefined identifier procedure *) ",
                    new[]{  "Token{ OK, type=OctInt range=(0, 3) str=&11 }",
                            "Token{ OK, type=ReservedIdentifierOverriding range=(3, 13) str=&procedure }",
                            "Token{ OK, type=CommentBi range=(14, 69) str=(* This is number &11 redefined identifier procedure *) }"
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
                    "Comment error\n :: Unfinished comment (Comment) at pos 13 level 0\n at pos 13", 13),
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

        [Test]
        public void TestProgram()
        {
            var result = Tok.Go(@"
program main;

var x : Integer;
begin; // Comment {WHAT {Level 2} (*Level2 {Level 3 comment}*)}
    for x := &0 to 10 do
    Begin        
        Writeln('Lol kek'#13#10);
        Writeln(x + $DEAD);
        {  UNEXPECTED COMMENT }    
    END
End.

", 0);
            const string expected = @"STATUS = Ok Length = 252
	Token{ OK, type=Identifier range=(1, 8) str=program }
	Token{ OK, type=Identifier range=(9, 13) str=main }
	Token{ OK, type=ExtraSymbol range=(13, 14) str=; }
	Token{ OK, type=Identifier range=(16, 19) str=var }
	Token{ OK, type=Identifier range=(20, 21) str=x }
	Token{ OK, type=ExtraSymbol range=(22, 23) str=: }
	Token{ OK, type=Identifier range=(24, 31) str=Integer }
	Token{ OK, type=ExtraSymbol range=(31, 32) str=; }
	Token{ OK, type=Identifier range=(33, 38) str=begin }
	Token{ OK, type=ExtraSymbol range=(38, 39) str=; }
	Token{ OK, type=CommentLine range=(40, 97) str=// Comment {WHAT {Level 2} (*Level2 {Level 3 comment}*)}
 }
	Inner tokens:
		Token{ OK, type=Comment range=(51, 96) str={WHAT {Level 2} (*Level2 {Level 3 comment}*)} }
		Inner tokens:
			Token{ OK, type=Comment range=(57, 66) str={Level 2} }
			Token{ OK, type=CommentBi range=(67, 95) str=(*Level2 {Level 3 comment}*) }
			Inner tokens:
				Token{ OK, type=Comment range=(76, 93) str={Level 3 comment} }
	Token{ OK, type=Identifier range=(101, 104) str=for }
	Token{ OK, type=Identifier range=(105, 106) str=x }
	Token{ OK, type=ExtraSymbolPair range=(107, 109) str=:= }
	Token{ OK, type=OctInt range=(110, 112) str=&0 }
	Token{ OK, type=Identifier range=(113, 115) str=to }
	Token{ OK, type=Integer range=(116, 118) str=10 }
	Token{ OK, type=Identifier range=(119, 121) str=do }
	Token{ OK, type=Identifier range=(126, 131) str=Begin }
	Token{ OK, type=Identifier range=(148, 155) str=Writeln }
	Token{ OK, type=ExtraSymbol range=(155, 156) str=( }
	Token{ OK, type=String range=(156, 171) str='Lol kek'#13#10 }
	Token{ OK, type=ExtraSymbol range=(171, 172) str=) }
	Token{ OK, type=ExtraSymbol range=(172, 173) str=; }
	Token{ OK, type=Identifier range=(182, 189) str=Writeln }
	Token{ OK, type=ExtraSymbol range=(189, 190) str=( }
	Token{ OK, type=Identifier range=(190, 191) str=x }
	Token{ OK, type=ExtraSymbol range=(192, 193) str=+ }
	Token{ OK, type=HexInt range=(194, 199) str=$DEAD }
	Token{ OK, type=ExtraSymbol range=(199, 200) str=) }
	Token{ OK, type=ExtraSymbol range=(200, 201) str=; }
	Token{ OK, type=Comment range=(210, 233) str={  UNEXPECTED COMMENT } }
	Token{ OK, type=Identifier range=(242, 245) str=END }
	Token{ OK, type=Identifier range=(246, 249) str=End }
	Token{ OK, type=ExtraSymbol range=(249, 250) str=. }
";
            Assert.AreEqual(expected, result.ToString());
        }
    }
}