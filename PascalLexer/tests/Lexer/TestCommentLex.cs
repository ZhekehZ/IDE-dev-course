using NUnit.Framework;
using PascalLexer.Lexer;
using Regseed;

namespace PascalLexer.tests.Lexer
{
    [TestFixture]
    public class TestCommentLex : TestHelper
    {
        protected override ILexer GetTok() => new CommentLex();

        [Test]
        public void CommentFull()
        {
            TestAllPositiveFull(
                "{ comment }",
                "(* comment *)",
                "// comment",
                "{ { }",
                "// { comment\n",
                "/////////",
                "{multi\nline\ncomment}",
                "(*multi\nline\ncomment*)",
                "{}", "(**)", "//"
            );
            
            TestAllNegativeFull(
                "c{comment}",
                "{", "}", "(*", "*)", "/", "(**+)", "{c)", "/comm/",
                "{comm", "comment"
            );
        }

        [Test]
        public void CommentPartial()
        {
            TestAllPartial(
                ("{aa}1", 4),
                ("{aa}{}", 4),
                ("//{\n//", 4),
                ("(* \n*\n *))", 9)
            );
        }

        [Test]
        public void CommentStressFull()
        {
            var gen = new RegSeed(@"(\/\/|\(\*|\{)[a-zA-Z \(\*\{\/0-9\n]{1,20}(\*\)|\})");
            for (var i = 0; i < 10000; i++)
            {
                var s = gen.Generate();
                if (s[0] == '{' && s[^1] == '}' || s[0] == '(' && s[^1] == ')' || s[0] == '/') {
                    TestPositiveFull(s);
                } else {
                    TestNegativeFull(s);
                }
            }
        }
    }
}