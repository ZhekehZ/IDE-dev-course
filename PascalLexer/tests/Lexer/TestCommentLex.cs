using System.Linq;
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
                "{ { } }",
                "// { comment } \n",
                "/////////",
                "{multi\nline\ncomment}",
                "(*multi\nline\ncomment*)",
                "{}", 
                "(**)", 
                "(***)", 
                "(****)", 
                "(*****)",
                "{(***)}",
                "{*()*}",
                "{*)*}",
                "//",
                "// {"
            );
            
            TestAllNegativeFull(
            "c{comment}",
            "{", "}", "(*", "*)", "/", "(**+)", "{c)", "/comm/",
            "{comm", "comment",
            "{*(*}",
            "// {\n}"
            );
        }

        [Test]
        public void CommentPartial()
        {
            TestAllPartial(
                ("{aa}1", 4),
                ("{aa}{}", 4),
                ("//{}\n//", 5),
                ("(* \n*\n *))", 9)
            );
        }

        [Test]
        public void TestInnerComments()
        {
            {
                const string str = "{ { } }";
                var token = GetTok().Go(str, 0);
                Assert.AreEqual(Status.Ok, token.Status);
                Assert.AreEqual(1, token.InnerTokens.Count);
                Assert.AreEqual(TokenType.Comment, token.Type);
                Assert.AreEqual(TokenType.Comment, token.InnerTokens.First().Type);
                Assert.AreEqual(0, token.StartPosition);
                Assert.AreEqual(7, token.EndPosition);
                Assert.AreEqual(2, token.InnerTokens.First().StartPosition);
                Assert.AreEqual(5, token.InnerTokens.First().EndPosition);
            }
            
            {
                const string str = "{ (* *) }";
                var token = GetTok().Go(str, 0);
                Assert.AreEqual(Status.Ok, token.Status);
                Assert.AreEqual(1, token.InnerTokens.Count);
                Assert.AreEqual(TokenType.Comment, token.Type);
                Assert.AreEqual(TokenType.CommentBi, token.InnerTokens.First().Type);
                Assert.AreEqual(0, token.StartPosition);
                Assert.AreEqual(9, token.EndPosition);
                Assert.AreEqual(2, token.InnerTokens.First().StartPosition);
                Assert.AreEqual(7, token.InnerTokens.First().EndPosition);
            }
            
            {
                const string str = "{ (* { } *) }";
                var token = GetTok().Go(str, 0);
                Assert.AreEqual(Status.Ok, token.Status);
                Assert.AreEqual(1, token.InnerTokens.Count);
                Assert.AreEqual(TokenType.Comment, token.Type);
                Assert.AreEqual(TokenType.CommentBi, token.InnerTokens.First().Type);
                Assert.AreEqual(0, token.StartPosition);
                Assert.AreEqual(13, token.EndPosition);
                Assert.AreEqual(2, token.InnerTokens.First().StartPosition);
                Assert.AreEqual(11, token.InnerTokens.First().EndPosition);
                Assert.AreEqual(1, token.InnerTokens.First().InnerTokens.Count);
            }
            
            {
                const string str = "(* {0} {1} {2} *)";
                var token = GetTok().Go(str, 0);
                Assert.AreEqual(Status.Ok, token.Status);
                Assert.AreEqual(3, token.InnerTokens.Count);
                Assert.AreEqual(TokenType.CommentBi, token.Type);
                Assert.AreEqual(0, token.StartPosition);
                Assert.AreEqual(17, token.EndPosition);
                Assert.True(token.InnerTokens.All(x => x.Type == TokenType.Comment));
                Assert.AreEqual(3, token.InnerTokens[0].StartPosition);
                Assert.AreEqual(7, token.InnerTokens[1].StartPosition);
                Assert.AreEqual(11, token.InnerTokens[2].StartPosition);
            }
        }
        
        
        [Test]
        public void TestInnerCommentError()
        {
            {
                const string str = "{ { }";
                var token = GetTok().Go(str, 0);
                Assert.AreEqual(Status.Fail, token.Status);
                Assert.AreEqual(
                    @"Comment error
 :: Unfinished comment (Comment) at pos 0 level 0
 at pos 0".Trim(), token.ErrorMessage);
            }
            
            {
                const string str = "{ (* }";
                var token = GetTok().Go(str, 0);
                Assert.AreEqual(Status.Fail, token.Status);
                Assert.AreEqual(
                    @"Comment error
 :: Unfinished comment (Comment) at pos 0 level 0
 :: Unfinished comment (CommentBi) at pos 2 level 1
 at pos 0".Trim(), token.ErrorMessage);
            }
            
            {
                const string str = "{ //*){ (* } }";
                var token = GetTok().Go(str, 0);
                Assert.AreEqual(Status.Fail, token.Status);
                Assert.AreEqual(
                    @"Comment error
 :: Unfinished comment (Comment) at pos 0 level 0
 :: Unfinished comment (Comment) at pos 6 level 1
 :: Unfinished comment (CommentBi) at pos 8 level 2
 at pos 0".Trim(), token.ErrorMessage);
            }
        }
    }
}