using JetBrains.ReSharper.Daemon.SyntaxHighlighting;
using JetBrains.ReSharper.Host.Features.SyntaxHighlighting;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Parsing;

namespace JetBrains.ReSharper.Plugins.Spring
{
    [Language(typeof(SpringLanguage))]
    internal class SpringSyntaxHighlightingManager : RiderSyntaxHighlightingManager
    {
        public override SyntaxHighlightingProcessor CreateProcessor()
        {
            return new SpringSyntaxHighlightingProcessor();
        }
    }
    
    class SpringSyntaxHighlightingProcessor : SyntaxHighlightingProcessor
    {
        protected override bool IsBlockComment(TokenNodeType tokenType)
        {
            return ((SpringTokenType) tokenType).IsBlockComment;
        }

        protected override bool IsLineComment(TokenNodeType tokenType)
        {
            return ((SpringTokenType) tokenType).IsLineComment;
        }

        protected override bool IsString(TokenNodeType tokenType)
        {
            return tokenType.IsStringLiteral;
        }

        protected override bool IsNumber(TokenNodeType tokenType)
        {
            return tokenType.IsConstantLiteral;
        }

        protected override bool IsKeyword(TokenNodeType tokenType)
        {
            return tokenType.IsKeyword;
        }
    }
}