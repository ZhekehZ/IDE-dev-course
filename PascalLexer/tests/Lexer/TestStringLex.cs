using System;
using NUnit.Framework;
using PascalLexer.Lexer;
using Regseed;

namespace PascalLexer.tests.Lexer
{
    [TestFixture]
    public class TestStringLex : TestHelper
    {
        protected override ILexer GetTok() => new StringLex();
        
        [Test]
        public void TestString()
        {
            TestAllPositiveFull(
                "'This is a pascal string'",
                "''", 
                "'a'",
                "'A tabulator character: '#9' is easy to embed'",
                "'the string starts here'#13#10'   and continues here'"
            );
            
            TestAllNegativeFull(
                "",
                "'the string starts here\nand continues here'",
                "'the string starts here\r\nand continues here'",
                "'",
                "'''"
            );
        }

        [Test]
        public void StringStressPositiveFull()
        {
            const string pat = @"(\'[a-zA-Z\#0-9\t _\!\@\#\$\%\^\&\*\(]{1,10}\'|\#[0-9]{1,10}){1,10}";
            var rand  = new RegSeed(pat, new Random(17));
            for (var i = 0; i < 100000; i++)
            {
                var s = rand.Generate();
                TestPositiveFull(s);
                TestNegativeFull(s + '\'');
            }
        }
        
        
        [Test]
        public void StringStressFull()
        {
            const string reg = @"^(\'[^\r\n\']+\'|\#[0-9]+)+$";
            const string all = @"(\'[\+\r\na-zA-Z\#0-9\t _\!\@\#\$\%\^\&\*\(]{1,10}\'|\#[0-9]{0,10}){1,10}";
            StressTestAll(all, reg, cycles: 100000);
        }
        
        [Test]
        public void TestStringPartial()
        {
            TestAllPartial(
                ("'aaa'123", 5),
                ("''\"", 2), 
                ("'a'#123.123", 7),
                ("#12'1'2", 6),
                ("#11$12", 3)
            );
        }
    }
}