using System;
using System.Text.RegularExpressions;
using NUnit.Framework;
using PascalLexer.Lexer;
using Regseed;

namespace PascalLexer.tests.Lexer
{
    public abstract class TestHelper
    {
        protected abstract ILexer GetTok();
        
        protected void TestPositiveFull(string word)
        {
            var decr = "Case '" + word + "'";
            Token res = null;
            Assert.DoesNotThrow(() => { res = GetTok().Go(word, 0); }, decr);
            Assert.AreEqual(Status.Ok, res.Status, decr);
            Assert.AreEqual(word.Length, res.EndPosition, decr);
            Assert.AreEqual(0, res.StartPosition, decr);
            Assert.AreEqual(word.Length, res.Length, decr);
        }

        protected void TestNegativeFull(string word)
        {
            var decr = "Case '" + word + "'";
            Token res = null;
            Assert.DoesNotThrow(() => { res = GetTok().Go(word, 0); }, decr);
            if (res.Status != Status.Ok) return;
            Assert.True(word.Length > res.EndPosition, decr + " finished at " + res.EndPosition + " with " + res.Status);
            Assert.AreEqual(0, res.StartPosition);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        protected void TestPositivePartial(string word, int expectedPos)
        {
            var decr = "Case '" + word + "'";
            Token res = null;
            Assert.DoesNotThrow(() => { res = GetTok().Go(word, 0); }, decr);
            Assert.AreEqual(Status.Ok, res.Status, decr);
            Assert.AreEqual(0, res.StartPosition, decr);
            Assert.AreEqual(expectedPos, res.EndPosition, decr);
        }

        protected void TestAllPositiveFull(params string[] cases)
        {
            foreach (var curr in cases) TestPositiveFull(curr);
        }
        
        protected void TestAllNegativeFull(params string[] cases)
        {
            foreach (var curr in cases) TestNegativeFull(curr);
        }
        
        protected void TestAllPartial(params (string, int)[] cases)
        {
            foreach (var (str, pos) in cases) TestPositivePartial(str, pos);
        }

        protected void StressTestAll(string regexAll, string regexGood, int seed = 17, 
            int positiveThreshold = 1000, int negativeThreshold = 1000, int cycles = 10000)
        {
            var reg = new Regex("^" + regexGood + "$"); 
            var gen = new RegSeed(regexAll, new Random(seed));
            var (pos, neg) = (0, 0);
            for (var i = 0; i < cycles; i++)
            {
                var s = gen.Generate();
                if (reg.IsMatch(s))
                {
                    pos++;
                    TestPositiveFull(s);
                } else
                {
                    neg++;
                    TestNegativeFull(s);
                }
            }
            Assert.True(pos > positiveThreshold && neg > negativeThreshold, 
                "Positive cases: " + pos + ", negative cases: " + neg);
        }
    }
}