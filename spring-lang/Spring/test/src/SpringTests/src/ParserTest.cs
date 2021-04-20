using JetBrains.ReSharper.Plugins.Spring;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;

namespace JetBrains.ReSharper.Plugins.SpringTests
{
    [TestFixture]
    [TestFileExtension(".spring")]
    public class ParserTest : ParserTestBase<SpringLanguage>
    {
        protected override string RelativeTestDataPath => "parser";

        [TestCase("test01")]
        [Test]
        public void Test1(string filename)
        {
            DoOneTest(filename);
        }
    }
}