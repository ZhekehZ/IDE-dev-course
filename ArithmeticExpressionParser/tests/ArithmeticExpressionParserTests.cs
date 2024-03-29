using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace ArithmeticExpressionParser.Tests
{
    [TestFixture]
    public class ArithmeticExpressionParserTests
    {
        private readonly IParser<IExpression> _simpleParser = new ArithmeticExpressionParser(
             new Dictionary<string, ushort> {{"*", 6}, {"+", 5}});

        private readonly IParser<IExpression> _complexParser = new ArithmeticExpressionParser(
            new Dictionary<string, ushort>
            {
                {"^", 8}, 
                {"*", 7}, {"/", 7}, {"%", 7},  
                {"+", 6}, {"-", 6}, 
                {"==", 4}, {">", 4}, {"<", 4}, 
                {">>=", 4}, 
                {"$", 0}
            },
            "()[]{}", 
            new List<string> {"const", "head", "tail"});
        
        private readonly IExpressionVisitor _visitor = new DumpVisitor();
        
        [Test]
        public void TestSimple()
        {
            _simpleParser.Parse("1").Accept(_visitor);
            Assert.AreEqual("Lit(1)", _visitor.ToString());   
        }
        
        [Test]
        public void TestBinOpSimple()
        {
            _simpleParser.Parse("1+a").Accept(_visitor);
            Assert.AreEqual("Bin(Lit(1) + Var(a))", _visitor.ToString());   
        }
        
        [Test]
        public void TestAssociativity()
        {
            _simpleParser.Parse("1+a+b").Accept(_visitor);
            Assert.AreEqual("Bin(Lit(1) + Bin(Var(a) + Var(b)))", _visitor.ToString());   
        }
        
        [Test]
        public void TestPriority()
        {
            _simpleParser.Parse("a+b*c").Accept(_visitor);
            Assert.AreEqual("Bin(Var(a) + Bin(Var(b) * Var(c)))", _visitor.ToString());
            _simpleParser.Parse("a*b+c").Accept(_visitor);
            Assert.AreEqual("Bin(Bin(Var(a) * Var(b)) + Var(c))", _visitor.ToString());   
        }
        
        [Test]
        public void TestParentheses()
        {
            _simpleParser.Parse("a*b+c*d").Accept(_visitor);
            Assert.AreEqual("Bin(Bin(Var(a) * Var(b)) + Bin(Var(c) * Var(d)))", _visitor.ToString());
            _simpleParser.Parse("a*(b+c)*d").Accept(_visitor);
            Assert.AreEqual("Bin(Var(a) * Bin(Bin(Var(b) + Var(c)) * Var(d)))", _visitor.ToString());   
        }
        
        [Test]
        public void TestLongerExpression()
        {
            _simpleParser.Parse("(a*b)+11*(d+e)*(12*(13))").Accept(_visitor);
            Assert.AreEqual("Bin(Bin(Var(a) * Var(b)) + Bin(Lit(11) * " +
                            "Bin(Bin(Var(d) + Var(e)) * Bin(Lit(12) * Lit(13)))))", _visitor.ToString());   
        }
        
        [Test]
        public void TestLongNames()
        {
            _simpleParser.Parse("hello+(world1*abc)").Accept(_visitor);
            Assert.AreEqual("Bin(Var(hello) + Bin(Var(world1) * Var(abc)))", _visitor.ToString());   
        }
                
        [Test]
        public void TestSpaces()
        {
            _simpleParser.Parse("a       + s          * 11").Accept(_visitor);
            Assert.AreEqual("Bin(Var(a) + Bin(Var(s) * Lit(11)))", _visitor.ToString());   
        }

        [Test]
        public void TestComplexOperators()
        {
            _complexParser.Parse("a >>= b $ 1 + 1").Accept(_visitor);
            Assert.AreEqual("Bin(Bin(Var(a) >>= Var(b)) $ Bin(Lit(1) + Lit(1)))", _visitor.ToString());
        }
        
        [Test]
        public void TestDifferentBraces()
        {
            _complexParser.Parse("c * {(a + 1) + [b % 2]}").Accept(_visitor);
            Assert.AreEqual("Bin(Var(c) * Bin(Bin(Var(a) + Lit(1)) + Bin(Var(b) % Lit(2))))", 
                _visitor.ToString());
        }
        
        [Test]
        public void TestFunctionSimpleCall()
        {
            _complexParser.Parse("const()").Accept(_visitor);
            Assert.AreEqual("Fun<const>()", _visitor.ToString());

            _complexParser.Parse("const(1, 2)").Accept(_visitor);
            Assert.AreEqual("Fun<const>(Lit(1), Lit(2))", _visitor.ToString());
            
            _complexParser.Parse("1 + head(a, b * c) + 1").Accept(_visitor);
            Assert.AreEqual("Bin(Lit(1) + Bin(Fun<head>(Var(a), Bin(Var(b) * Var(c))) + Lit(1)))", 
                _visitor.ToString());   
        }
        
        [Test]
        public void TestFunctionComplexCall()
        {
            _complexParser.Parse("const(head(a, b), d, tail(b, c))").Accept(_visitor);
            Assert.AreEqual("Fun<const>(Fun<head>(Var(a), Var(b)), Var(d), Fun<tail>(Var(b), Var(c)))", 
                _visitor.ToString());
            
        }

        [Test]
        public void TestInvalidExpression()
        {
            Assert.Throws<ParserInvalidExpression>(() => _complexParser.Parse("c +").Accept(_visitor));
            Assert.Throws<ParserInvalidExpression>(() => _complexParser.Parse("c (+) d").Accept(_visitor));
            Assert.Throws<ParserInvalidExpression>(() => _complexParser.Parse("c + ()").Accept(_visitor));
            Assert.Throws<ParserInvalidExpression>(() => _complexParser.Parse("c b a").Accept(_visitor));
        }

        [Test]
        public void TestIncorrectBrackets()
        {
            Assert.Throws<ParserInvalidBrackets>(() => _complexParser.Parse("( a + b  ").Accept(_visitor));
            Assert.Throws<ParserInvalidBrackets>(() => _complexParser.Parse("( a + b ]").Accept(_visitor));
            Assert.Throws<ParserInvalidBrackets>(() => _complexParser.Parse(") a + b (").Accept(_visitor));
            Assert.Throws<ParserInvalidBrackets>(() => _complexParser.Parse(" [{(1)]} ").Accept(_visitor));
            Assert.Throws<ParserInvalidBrackets>(() => _complexParser.Parse("const[1)").Accept(_visitor));
        }

        [Test]
        public void TestInvalidOperator()
        {
            var ex = Assert.Throws<ParserInvalidOperatorName>(() => _complexParser.Parse("a >= b").Accept(_visitor));
            Assert.AreEqual(">=", ex?.OperatorName);
            ex = Assert.Throws<ParserInvalidOperatorName>(() => _complexParser.Parse("a & b").Accept(_visitor));
            Assert.AreEqual("&", ex?.OperatorName);
        }

        [Test]
        public void TestInvalidCharacter()
        {
            var ex = Assert.Throws<ParserInvalidCharacter>(() => _complexParser.Parse("a 😋 b").Accept(_visitor));
            Assert.AreEqual("😋"[0], ex?.Character);
        }
        
        [Test]
        public void TestInvalidFunctionCall()
        {
            Assert.Throws<ParserInvalidFunctionCall>(() => _complexParser.Parse("tail").Accept(_visitor));
            Assert.Throws<ParserInvalidFunctionCall>(() => _complexParser.Parse("const + 1").Accept(_visitor));
        }
        
        [Test]
        public void StressTest()
        {
            var gen = new RandomExpressionGenerator(37);
            gen.SetFunctions("const", "head", "tail");
            gen.SetOperators("+", "*", "-", "/");
            
            var testHeadCount = 0;
            var testHead = new Action<IExpression>(ex =>
            {
                ex.Accept(_visitor);
                var expected = _visitor.ToString();
                var toParse = expected?
                    .Replace("Lit", "")
                    .Replace("Var", "")
                    .Replace("Bin", "")
                    .Replace("Fun<", "")
                    .Replace(">", "");

                _complexParser.Parse(toParse).Accept(_visitor);
                Assert.AreEqual(expected, _visitor.ToString());
                testHeadCount++;
            });

            var e = gen.GetRandom(50_000, "+", testHead);
            testHead(e);
            Assert.True(testHeadCount > 1000);
        }
    }
}