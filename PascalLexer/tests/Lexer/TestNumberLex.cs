using System;
using System.Collections.Generic;
using NUnit.Framework;
using PascalLexer.Lexer;
using Regseed;

namespace PascalLexer.tests.Lexer
{
    [TestFixture]
    public class TestNumberLex : TestHelper
    {
        
        private const string HexDigit = @"\$[0-9a-fA-F]{1,12}";
        private const string OctDigit = @"\&[0-7]{1,12}";
        private const string BinDigit = @"\%[01]{1,12}";
        private const string RegDigitSeq = "[0-9]{1,12}";
        private const string UnsignedReal = RegDigitSeq + @"(\." + RegDigitSeq + @")?([Ee][\+\-]?" + RegDigitSeq + ")?";
        private const string UnsignedInt = "(" + RegDigitSeq + "|" + HexDigit + "|" + OctDigit + "|" + BinDigit + ")";
        private const string SignedNum = @"[\+\-]?(" + UnsignedInt + "|" + UnsignedReal + ")";
        
        private const string BadNum = @"[\$\&\%]?[0-9a-fA-F]{1,5}";
        private const string SignedBadNum = @"[\+\-]?" + BadNum + @"\.?[eE]?[\+\-]?" + BadNum;
        
        protected override ILexer GetTok() => new NumberLex();

        [Test]
        public void IntegralFull()
        {
            new List<string>{"0", "123", "1234567890"}
                .ForEach(s =>
                {
                    TestPositiveFull(s);
                    TestPositiveFull('+' + s);
                    TestPositiveFull('-' + s);
                });
            
            TestAllNegativeFull(
                "", "123f", "s123", "234324 234", "FFF",
                "-", "+", "-1+2", "--1123", "++1123", "+14324-"
            );
        }

        [Test]
        public void IntegralStress()
        {
            var random = new Random(13);
            for (var i = 0; i < 1000; i++)
            {
                var s = random.Next().ToString();
                TestPositiveFull(s);
                TestPositiveFull('+' + s);
                TestPositiveFull('-' + s);
            }
        }
        
        [Test]
        public void RealBasicFull()
        {
            new List<string>
                {
                    "0.0", "1.789", "235.5", "123.123", "194583725.2349875"
                }
                .ForEach(s =>
                {
                    TestPositiveFull(s);
                    TestPositiveFull('+' + s);
                    TestPositiveFull('-' + s);
                });
            
            TestAllNegativeFull(
                ".0", ".123", "123..456", "13.56.1", "1235.",
                "", "-.123", "+.123", "+123..456", "-123..456", "+13.56.1", "-13.56.1", 
                "+1235.", "123.+123", "123.-123", "123-.123", "+1.12+" 
            );
        }

        [Test]
        public void RealScaledFull()
        {
            new List<string>
                {
                    "0.0e0", "0123.03123e04", "235.22e+2345", "123.3e-34", "123E123", 
                    "123E+123", "123E-1123", "123E-1", "123E+1"
                }
                .ForEach(s =>
                {
                    TestPositiveFull(s);
                    TestPositiveFull('+' + s);
                    TestPositiveFull('-' + s);
                });
            
            TestAllNegativeFull(
                "123.e123", "123e12.3", "e666", "1EEE", "123Ee123",
                "+1234.324e123e123", "-e", "-e234", "+1E-", "12E+"
            );
        }

        [Test]
        public void HexIntFull()
        {
            new List<string>
                {
                    "$FFF", "$DeadBeef", "$B0B", "$D0", "$1234"
                }
                .ForEach(s =>
                {
                    TestPositiveFull(s);
                    TestPositiveFull('+' + s);
                    TestPositiveFull('-' + s);
                });
            
            TestAllNegativeFull(
                "$", "$FFF.123", "$DeadBeefE+123", "+1.$B0B", "$123.3e", "$LOL"
            );
        }
        
        [Test]
        public void OctIntFull()
        {
            new List<string>
                {
                    "&0777", "&123", "&0"
                }
                .ForEach(s =>
                {
                    TestPositiveFull(s);
                    TestPositiveFull('+' + s);
                    TestPositiveFull('-' + s);
                });
            
            TestAllNegativeFull(
                "&", "&0.777", "&123.", "&-0", "&1238"
            );
        }
        
        
        [Test]
        public void BinIntFull()
        {
            new List<string>
                {
                    "%01", "%0", "%1", "%11111001101"
                }
                .ForEach(s =>
                {
                    TestPositiveFull(s);
                    TestPositiveFull('+' + s);
                    TestPositiveFull('-' + s);
                });

            TestAllNegativeFull(
                "%", "%02", "%1E", "%1.23"
            );
        }
        
        [Test]
        public void NumberStressPositiveFull()
        {
            var rand  = new RegSeed(SignedNum, new Random(17));
            for (var i = 0; i < 100000; i++)
            {
                var s = rand.Generate();
                TestPositiveFull(s);
            }
        }
        
        [Test]
        public void NumberStressFull()
        {
            StressTestAll(SignedBadNum, SignedNum, cycles: 100000);
        }


        [Test]
        public void SignedPartial()
        {
            TestAllPartial(
                ("123z", 3), ("123+", 3), ("1H2O", 1), ("1e1e", 3), ("123asd13", 3), ("-123.33e+14e", 11)
            );
        }
    }
}