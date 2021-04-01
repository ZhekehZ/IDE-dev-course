using NUnit.Framework;
using PascalLexer.Lexer;

namespace PascalLexer.tests.Lexer
{
    
    [TestFixture]
    public class TestIdentifierLex : TestHelper
    {
        protected override ILexer GetTok() => new IdentifierLex();

        [Test]
        public void CommentFull()
        {
            TestAllPositiveFull(
                "HelloWorld",
                "x",
                "_abc",
                "___",
                "a999",
                "&begin"
            );
            
            TestAllNegativeFull(
                "9a123",
                "&&aab",
                "",
                "a bc",
                "sd^d"
            );
        }

        [Test]
        public void CommentPartial()
        {
            TestAllPartial(
                ("abc def", 3),
                ("&du hast", 3),
                ("_asd55+5", 6),
                ("A&V", 1)
            );
        }

        [Test]
        public void CommentStressFull()
        {
            StressTestAll(@"[a-zA-Z0-9_\&]{0,10}", @"(\&)?[a-zA-Z_][a-zA-Z0-9_]{0,10}");
        }
    }
}