using System.Text;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Text;

namespace JetBrains.ReSharper.Plugins.Spring
{
    class SpringTokenType : TokenNodeType
    {
        public static readonly SpringTokenType EXTRA_SYM = new("EXTRA_SYM", 0);
        public static readonly SpringTokenType EXTRA_SYM_PAIR = new("EXTRA_SYM_PAIR", 1);
        public static readonly SpringTokenType BCOMMENT = new("BCOMMENT", 2);
        public static readonly SpringTokenType PCOMMENT = new("PCOMMENT", 3);
        public static readonly SpringTokenType LCOMMENT = new("LCOMMENT", 4);
        public static readonly SpringTokenType STRING = new("STRING", 5);
        public static readonly SpringTokenType IDENTIFIER = new("IDENTIFIER", 6);
        public static readonly SpringTokenType KEYWORD = new("KEYWORD", 7);
        public static readonly SpringTokenType INTEGER = new("INTEGER", 8);
        public static readonly SpringTokenType REAL = new("REAL", 9);
        public static readonly SpringTokenType WHITE = new("WHITE", 10);
        public static readonly SpringTokenType ASM_TEXT = new("ASM_TEXT", 11);
        public static readonly SpringTokenType ERROR = new("ERROR", 12);

        private SpringTokenType(string s, int index, string representation="") : base(s, index)
        {
            TokenRepresentation = representation;
        }

        public override LeafElementBase Create(IBuffer buffer, TreeOffset startOffset, TreeOffset endOffset)
        {
            return new Leaf(new StringBuffer(
                    buffer.GetText().Substring(startOffset.Offset, endOffset.Offset - startOffset.Offset)), this);
        }

        public override bool IsWhitespace => this == WHITE;
        public override bool IsComment => this == BCOMMENT || this == PCOMMENT || this == LCOMMENT;
        public override bool IsStringLiteral => this == STRING || this == ASM_TEXT;
        public override bool IsConstantLiteral => this == INTEGER || this == REAL;
        public override bool IsIdentifier => this == IDENTIFIER;
        public override bool IsKeyword => this == KEYWORD;

        public bool IsBlockComment => this == BCOMMENT || this == PCOMMENT;
        public bool IsLineComment => this == LCOMMENT;
        public override string TokenRepresentation { get; }
    }


    internal class Leaf : LeafElementBase, ITokenNode
    {
        private readonly IBuffer _buffer;
        private readonly SpringTokenType _type;

        public Leaf(IBuffer buffer, SpringTokenType type)
        {
            _buffer = buffer;
            _type = type;
        }

        public override int GetTextLength()
        {
            return _buffer.Length;
        }

        public override StringBuilder GetText(StringBuilder to)
        {
            return to.Append(_buffer.GetText());
        }

        public override IBuffer GetTextAsBuffer()
        {
            return _buffer;
        }

        public override string GetText()
        {
            return _buffer.GetText();
        }

        public override NodeType NodeType => _type;

        public override PsiLanguageType Language => SpringLanguage.Instance;

        public TokenNodeType GetTokenType()
        {
            return _type;
        }
    }
}