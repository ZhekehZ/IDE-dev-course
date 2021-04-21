using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.Text;

namespace JetBrains.ReSharper.Plugins.Spring.Lexer
{
    public class PascalLexer : Psi.Parsing.ILexer
    {
        private readonly string _text;
        private int _startPos;
        private int _pos;

        private static readonly CommentLex LexComment = new();
        private static readonly StringLex LexString = new();
        private static readonly NumberLex LexNumber = new();
        private static readonly IdentifierLex LexIdentifier = new();
        private static readonly SymbolLex LexSymbol = new();
        private static readonly WhitespaceLex WhiteLex = new();
        private Token _token;
        
        private readonly List<ILexer> _lexOrder = new()
        {
            LexComment, LexString, LexIdentifier, LexNumber, LexSymbol, WhiteLex 
        };
        
        public PascalLexer([NotNull] IBuffer buffer)
        {
            Buffer = buffer;
            _text = buffer.GetText();
        }

        public void Start()
        {
            _startPos = 0;
            _pos = 0;
        }

        public void Advance()
        {
            _startPos = _pos;
            foreach (var tok in _lexOrder)
            {
                _token = tok.Go(_text, _pos);
                if (_token.Status == Status.Ok) break;
            }
            _pos = _token.EndPosition == _pos && _pos < Buffer.Length? _pos + 1 : _token.EndPosition;
        }

        public object CurrentPosition
        {
            get => _pos;
            set
            {
                _startPos = (int) value; 
                _pos = _startPos;
            }
        }

        public TokenNodeType TokenType
        {
            get
            {
                if (_token == null) Advance();
                if (_token.Status == Status.Fail)
                {
                    return _pos == _startPos ? null : SpringTokenType.ERROR;
                }
                
                switch (_token.Type)
                {
                    case Lexer.TokenType.ExtraSymbol:
                        return SpringTokenType.EXTRA_SYM;
                    case Lexer.TokenType.ExtraSymbolPair:
                        return SpringTokenType.EXTRA_SYM_PAIR;
                    case Lexer.TokenType.Comment:
                        return SpringTokenType.BCOMMENT;
                    case Lexer.TokenType.CommentBi:
                        return SpringTokenType.PCOMMENT;
                    case Lexer.TokenType.CommentLine:
                        return SpringTokenType.LCOMMENT;
                    case Lexer.TokenType.String:
                        return SpringTokenType.STRING;
                    case Lexer.TokenType.Identifier:
                        return IConfiguration.Keywords.Contains(_token.Result.ToLower()) 
                            ? SpringTokenType.KEYWORD 
                            : SpringTokenType.IDENTIFIER;
                    case Lexer.TokenType.ReservedIdentifierOverriding:
                        return SpringTokenType.IDENTIFIER;
                    case Lexer.TokenType.Real:
                        return SpringTokenType.REAL;
                    case Lexer.TokenType.Integer:
                    case Lexer.TokenType.HexInt:
                    case Lexer.TokenType.OctInt:
                    case Lexer.TokenType.BinInt:
                        return SpringTokenType.INTEGER;
                    case Lexer.TokenType.White:
                        return SpringTokenType.WHITE;
                    default:
                        return SpringTokenType.ERROR;
                }
            }
        }

        public int TokenStart
        {
            get
            {
                if (_token == null) Advance();
                return _startPos;
            }
        }

        public int TokenEnd
        {
            get
            {
                if (_token == null) Advance();
                return _pos;
            }
        }

        public IBuffer Buffer { get; }
    }
}