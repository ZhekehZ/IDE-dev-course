using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace ArithmeticExpressionParser.Tests
{
    [TestFixture]
    public class CodeGenerationTests
    {
        private readonly ArithmeticExpressionParser _parser = new(
            new Dictionary<string, ushort>
            {
                {"*", 7},
                {"+", 6},
                {">", 4}, {"<", 4},
            },
            functionNames: new List<string>{"inc", "sqr", "ifThenElse", "pow"});

        private readonly CodeGenerationVisitor _coGenVisitor = new(
            operators:
                new Dictionary<string, BiFunctionGenerator>
                {
                    { "+", BasicFunctions.Plus },
                    { "*", BasicFunctions.Mul },
                    { ">", BasicFunctions.Gt },
                    { "<", BasicFunctions.Lt },
                },
            functions:
                new Dictionary<string, FunctionGenerator>
                {
                    { "inc", BasicFunctions.Inc },
                    { "sqr", BasicFunctions.Sqr },
                },
            biFunctions:
                new Dictionary<string, BiFunctionGenerator>
                {
                    { "pow", BasicFunctions.Pow },
                },
            triFunctions:
                new Dictionary<string, TriFunctionGenerator>
                {
                    { "ifThenElse", BasicFunctions.IfThenElse }
                }
            );

        private readonly ArithmeticExpressionCompiler _compiler;

        public CodeGenerationTests()
        {
            _compiler = new ArithmeticExpressionCompiler(_parser, _coGenVisitor);
        }

        private Func<string, VarargFunc<int, int>> Compile => _compiler.Compile;
        
        [Test]
        public void TestPlus() {
            Assert.AreEqual(2, Compile("0 + 2")());
            Assert.AreEqual(19, Compile("1 + 18")());
            Assert.AreEqual(0, Compile("0 + 0")());
            Assert.AreEqual(100500, Compile("100000 + 500")());

            var random = new Random(37);
            for (int i = 0; i < 1000; i++)
            {
                var a = random.Next();
                var b = random.Next();
                Assert.AreEqual(a + b, Compile(a + " + " + b)(), "values: " + a + " + " + b);
            }
        }
        
        [Test]
        public void TestMul() {
            Assert.AreEqual(0, Compile("0 * 2")());
            Assert.AreEqual(18, Compile("1 * 18")());
            Assert.AreEqual(10, Compile("2 * 5")());
            Assert.AreEqual(60000, Compile("200 * 300")());
            
            var random = new Random(37);
            for (int i = 0; i < 1000; i++)
            {
                var a = random.Next();
                var b = random.Next();
                Assert.AreEqual(a * b, Compile(a + " * " + b)(), "values: " + a + " * " + b);
            }
        }

        [Test]
        public void TestGt() {
            Assert.AreEqual(0, Compile("0 > 2")());
            Assert.AreEqual(0, Compile("2 < 0")());
            Assert.AreEqual(0, Compile("1 > 1")());
            Assert.AreEqual(0, Compile("1 < 1")());
            Assert.AreEqual(1, Compile("123 > 45")());
            Assert.AreEqual(1, Compile("45 < 123")());
            
            var random = new Random(37);
            for (int i = 0; i < 1000; i++)
            {
                var a = random.Next();
                var b = random.Next();
                Assert.AreEqual(a > b ? 1 : 0, Compile(a + " > " + b)(), "values: " + a + " > " + b);
                Assert.AreEqual(a < b ? 1 : 0, Compile(a + " < " + b)(), "values: " + a + " < " + b);
            }
        }
        
        [Test]
        public void TestInc() {
            Assert.AreEqual(1, Compile("inc(0)")());
            Assert.AreEqual(123, Compile("inc(122)")());
            
            var random = new Random(37);
            for (int i = 0; i < 500; i++)
            {
                var a = random.Next();
                Assert.AreEqual(a + 1, Compile("inc(" + a + ")")(), "value: " + a);
            }
        }

        [Test]
        public void TestSqr() {
            Assert.AreEqual(0, Compile("sqr(0)")());
            Assert.AreEqual(1, Compile("sqr(1)")());
            Assert.AreEqual(144, Compile("sqr(12)")());
            
            var random = new Random(37);
            for (int i = 0; i < 500; i++)
            {
                var a = random.Next();
                Assert.AreEqual(a * a, Compile("sqr(" + a + ")")(), "value: sqr(" + a + ")");
            }
        }
        
        [Test]
        public void TestPow() {
            Assert.AreEqual(1, Compile("pow(1, 20)")());
            Assert.AreEqual(123, Compile("pow(123, 1)")());
            Assert.AreEqual(1, Compile("pow(123, 0)")());
            Assert.AreEqual(81, Compile("pow(3, 4)")());
            
            var random = new Random(37);
            for (int i = 0; i < 50; i++)
            {
                var a = random.Next(6);
                var b = random.Next(6);
                Assert.AreEqual(Math.Round(Math.Pow(a, b)), Compile("pow(" + a + ", " + b + ")")(), 
                    "values: pow(" + a + ", " + b + ")");
            }
        }
        
        [Test]
        public void TestIfThenElse() {
            Assert.AreEqual(14, Compile("ifThenElse(1, 14, 65)")());
            Assert.AreEqual(65, Compile("ifThenElse(0, 14, 65)")());
            
            var random = new Random(37);
            for (int i = 0; i < 500; i++)
            {
                var a = random.Next(10);
                var b = random.Next();
                var c = random.Next();
                Assert.AreEqual(a > 0 ? b : c, Compile("ifThenElse(" + a + ", " + b + ", " + c + ")")(), 
                    "value: ifThenElse(" + a + ", " + b + ", " + c + ")");
            }
        }
        
        [Test]
        public void TestFunctionWithArguments() {
            Assert.AreEqual(5, Compile("a + sqr(b)")(1, 2));
            Assert.AreEqual(16, Compile("a + sqr(b)")(7, 3));
            Assert.AreEqual(8, Compile("inc(a + sqr(b))")(3, 2));
            Assert.AreEqual(124, Compile("inc(a + sqr(b))")(2, 11));
        }
        
                
        [Test]
        public void TestSamveVariable()
        {
            var f1 = Compile("a * a + 2 * a + 1");
            var f2 = Compile("sqr(inc(a))");
            for (int i = -100; i < 100; i+=7)
            {
                Assert.AreEqual(0, f1(i) - f2(i));
            }
        }
        
        [Test]
        public void TestComplexFunction()
        {
            var func = Compile("ifThenElse( " +
                                            "       a + sqr(b) > sqr(c) " +
                                            "           , (inc(a) + 12) * 2" +
                                            "           , pow(c, 4) " +
                                            ")");
            Func<int, int, int, int> eFunc = (a, b, c) => a + b * b > c * c ? (a + 1 + 12) * 2 : c * c * c * c;
            var random = new Random(31);
            for (int i = 0; i < 1000; i++)
            {
                var a = random.Next(100);
                var b = random.Next(100);
                var c = random.Next(100);
                Assert.AreEqual(eFunc(a, b, c), func(c, a, b), "values: " + (a, b, c));
            }
        }
        
        [Test]
        public void TestInvalidArity()
        {
            var e = Assert.Throws<CogenInvalidFunctionArity>(() => Compile("ifThenElse(a, b, c, d)"));
            if (e != null) Assert.AreEqual(4, e.Arity);
        }
    }
    
}