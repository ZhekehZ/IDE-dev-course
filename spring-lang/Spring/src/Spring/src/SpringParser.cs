using System;
using System.Collections.Generic;
using JetBrains.Application.Settings;
using JetBrains.DocumentModel;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon.CSharp.Errors;
using JetBrains.ReSharper.Daemon.CSharp.Stages;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Feature.Services.SelectEmbracingConstruct;
using JetBrains.ReSharper.I18n.Services.Daemon;
using JetBrains.ReSharper.Plugins.Spring.Parser;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.TreeBuilder;
using JetBrains.Text;
using static JetBrains.ReSharper.Plugins.Spring.Parser.Combinators;
using static JetBrains.ReSharper.Plugins.Spring.Parser.Keywords;
using static JetBrains.ReSharper.Plugins.Spring.Parser.OperatorsAndSymbols;
using static JetBrains.ReSharper.Plugins.Spring.Parser.Tokens;
using static JetBrains.ReSharper.Plugins.Spring.SpringCompositeNodeType;
using static JetBrains.ReSharper.Plugins.Spring.SpringTokenType;

namespace JetBrains.ReSharper.Plugins.Spring
{
    internal class SpringParser : IParser
    {
        private readonly ILexer _myLexer;
        private static readonly Parser.Parser Expression = ExpressionParser();
        private static readonly (Parser.Parser, Parser.Parser) Statement = StatementParser(Expression);
        private static readonly Parser.Parser Procedure = ProcedureParser(Statement.Item2, Expression);

        public SpringParser(ILexer lexer)
        {
            _myLexer = lexer;
        }

        public IFile ParseFile()
        {
            using var def = Lifetime.Define();
            var builder = new PsiBuilder(_myLexer, SpringFileNodeType.Instance, new TokenFactory(), def.Lifetime);
            var fileMark = builder.Mark();

            SkipCommentsAndWhitespaces(builder);
// // TODO REMOVE
//             var m = builder.Mark();
//             builder.AdvanceLexer();
//             
//             var declaration = new ParameterDeclaration();
//
//             var decl = SpringCompositeNodeType.PROCEDURE_ARGUMENT.SetDeclaration(declaration);
//             builder.Done(m, decl, null);

            Seq(Many(Procedure, strong: true), Statement.Item1, O_DOT)(builder);
            SkipCommentsAndWhitespaces(builder);

            if (!builder.Eof())
            {
                builder.Error("Text After '.'");
                while (!builder.Eof())
                {
                    Next(builder);
                }
            }

            builder.Done(fileMark, SpringFileNodeType.Instance, null);
            var file = (IFile) builder.BuildTree();
            return file;
        }

        private static Parser.Parser ExpressionParser()
        {
            Parser.Parser expression = null;
            // ReSharper disable once AccessToModifiedClosure
            // ReSharper disable once PossibleNullReferenceException
            var expr = new Parser.Parser(b => expression(b)); // for recursive calls

            Parser.Parser factor = null;
            // ReSharper disable once AccessToModifiedClosure
            // ReSharper disable once PossibleNullReferenceException
            var fact = new Parser.Parser(b => factor(b)); // for recursive calls

            factor = AltStrong(
                "Expression expected",
                Seq(O_POPEN, expr, O_PCLOSE),
                Seq(O_AT, TOK_IDENT),
                Seq(O_BOPEN, Many(Seq(expr, OptStrong("Expression expected after ..", Seq(O_DOTS, expr))), O_COMMA),
                    O_BCLOSE),
                Seq(O_PLUS, fact),
                Seq(O_MINUS, fact),
                Seq(TOK_IDENT, OptStrong("Expression list expected", Seq(O_POPEN, Many(expr, O_COMMA), O_PCLOSE))),
                Seq(KW_NOT, fact),
                Alt(TOK_INT, TOK_STR, KW_NIL, TOK_REAL)
            );

            var term = Many(factor, Alt(O_MULT, O_DIV, KW_DIV, KW_MOD, KW_AND, KW_SHL, KW_SHR, KW_AS), true,
                true, true, "Operand expected");
            var simpleExpr = Many(term, Alt(O_PLUS, O_MINUS, KW_OR, KW_XOR), true,
                true, true, "Operand expected");
            expression = Many(simpleExpr, Alt(O_LT, O_GT, O_LE, O_GE, O_EQ, O_NEQ, KW_IN, KW_IS), true,
                true, true, "Operand expected");

            return expression;
        }

        private static (Parser.Parser, Parser.Parser) StatementParser(Parser.Parser expression)
        {
            Parser.Parser statement = null;
            // ReSharper disable once AccessToModifiedClosure
            // ReSharper disable once PossibleNullReferenceException
            var stmt = new Parser.Parser(b => statement(b)); // for recursive calls

            var identStmt = Seq(TOK_IDENT,
                AltStrong(Seq(O_POPEN, Many(expression, O_COMMA), O_PCLOSE),
                    Seq(Alt(O_ASSIGN, O_ASG_MNS, O_ASG_PLS, O_ASG_MUL, O_ASG_DIV), expression)));
            var gotoStmt = Seq(KW_GOTO, TOK_IDENT);

            var asmStmt = Seq(KW_ASM,
                b =>
                {
                    var start = b.Mark();
                    var initialOffset = b.GetTokenOffset();

                    while (!b.Eof())
                    {
                        if (b.GetTokenType() == KEYWORD && b.GetTokenText() == "end")
                        {
                            b.AlterToken(start, ASM_TEXT);
                            return new ParserResult(true, b.GetTokenOffset() - initialOffset);
                        }

                        Next(b);
                    }

                    b.Error("end expected");
                    b.Done(start, BLOCK, null);
                    return new ParserResult(false, b.GetTokenOffset() - initialOffset);
                }, KW_END, OptStrong("At least one string argument expected",
                    Seq(O_BOPEN, Many(TOK_STR, O_COMMA, true), O_BCLOSE)));

            var withStmt = Seq(KW_WITH, Many(TOK_IDENT, O_COMMA, true), KW_DO, stmt);
            var whileStmt = Seq(KW_WHILE, expression, KW_DO, stmt);
            var repeatStmt = Seq(KW_REPEAT, Many(stmt, O_SEMIC, true), KW_UNTIL, expression);

            var forStmt = Seq(
                KW_FOR, TOK_IDENT,
                AltStrong(Seq(O_ASSIGN, expression, Alt(KW_TO, KW_DOWNTO), expression), Seq(KW_IN, expression)),
                KW_DO, stmt
            );
            var ifStmt = Seq(KW_IF, expression, KW_THEN, stmt, OptStrong("Invalid else statement", Seq(KW_ELSE, stmt)));

            var constant = Alt(TOK_STR, TOK_INT, TOK_IDENT);
            var caseStmt = Seq(
                KW_CASE, expression, KW_OF,
                Many(Seq(
                    Many(Seq(constant, OptStrong("constant expected after ..", Seq(O_DOTS, constant))), O_COMMA, true),
                    O_COLON, stmt), O_SEMIC, true),
                Opt(O_SEMIC),
                OptStrong("Invalid else statement list", Seq(Alt(KW_ELSE, KW_OTHERWISE), Many(stmt, O_SEMIC))),
                Opt(O_SEMIC), KW_END
            );
            var compound = Seq(KW_BEGIN, Many(stmt, O_SEMIC), Opt(O_SEMIC), KW_END);

            statement = AltStrong(compound, ifStmt, repeatStmt, whileStmt, withStmt, asmStmt, gotoStmt,
                forStmt, caseStmt, identStmt);

            return (statement, compound);
        }

        private static Parser.Parser ProcedureParser(Parser.Parser compoundStatement, Parser.Parser expression)
        {
            var parameterType = Tok(IDENTIFIER, null, TYPE_VARIABLE);
            var defaultParameterValue = AltStrong(TOK_STR, KW_NIL, TOK_REAL, TOK_INT);

            var procArg = Tok(IDENTIFIER, null, PROCEDURE_ARGUMENT_DECLARATION);
            var variable = Tok(IDENTIFIER, null, VARIABLE_DECLARATION);

            var paramList = Seq(Many(procArg, O_COMMA, true), O_COLON, Opt(Seq(KW_ARRAY, KW_OF)), parameterType);
            var valueParameter = Alt(Seq(procArg, O_COLON, parameterType, O_EQ, defaultParameterValue), paramList);

            var parameterDeclaration =
                AltStrong(
                    Seq(KW_VAR, paramList),
                    Seq(KW_OUT, paramList),
                    Seq(KW_CONST, valueParameter),
                    valueParameter
                );
            var formalParameterList = Seq(O_POPEN, Many(parameterDeclaration, O_SEMIC), O_PCLOSE);
            var procedureHeader = Seq(KW_PROCEDURE, TOK_IDENT, formalParameterList);

            var modifiers = AltStrong(
                Seq(KW_ABSOLUTE, AltStrong(TOK_IDENT, TOK_INT)),
                Seq(
                    O_SEMIC, AltStrong(
                        KW_EXPORT,
                        KW_CVAR,
                        Seq(KW_EXTERNAL, Opt(TOK_STR), OptStrong("Name expected", Seq(KW_NAME, TOK_STR)))
                    )
                )
            );

            var variableDeclaration = Seq(variable, O_COLON, parameterType,
                OptStrong("Expression expected", Seq(O_EQ, expression)), Opt(modifiers), O_SEMIC);
            var variableDeclarationPart = Many(Seq(KW_VAR, Many(variableDeclaration)));

            var block = Seq(variableDeclarationPart, compoundStatement);

            var subroutineBlock = AltStrong(KW_FORWARD, block);
            var procedure = Mark(PROCEDURE_DECLARATION, Seq(procedureHeader, O_SEMIC, subroutineBlock, O_SEMIC));

            return procedure;
        }
    }

    [DaemonStage]
    class SpringDaemonStage : DaemonStageBase<SpringFile>
    {
        protected override IDaemonStageProcess CreateDaemonProcess(IDaemonProcess process,
            DaemonProcessKind processKind, SpringFile file,
            IContextBoundSettingsStore settingsStore)
        {
            return new SpringDaemonProcess(process, file);
        }

        internal class SpringDaemonProcess : IDaemonStageProcess
        {
            private readonly SpringFile _myFile;

            public SpringDaemonProcess(IDaemonProcess process, SpringFile file)
            {
                _myFile = file;
                DaemonProcess = process;
            }

            public void Execute(Action<DaemonStageResult> committer)
            {
                var highlightings = new List<HighlightingInfo>();
                foreach (var treeNode in _myFile.Descendants())
                {
                    if (treeNode is PsiBuilderErrorElement error)
                    {
                        var range = error.GetDocumentRange();

                        if (range.IsEmpty)
                        {
                            var newRange = range.ExtendRight(1);
                            range = _myFile.GetDocumentRange().Contains(newRange) ? newRange : range;
                        }

                        highlightings.Add(new HighlightingInfo(range,
                            new CSharpSyntaxError(error.ErrorDescription, range)));
                    }

                    if (treeNode.NodeType == TYPE_VARIABLE)
                    {
                        var range = treeNode.GetDocumentRange();
                        highlightings.Add(new HighlightingInfo(range, new CSharpIdentifierHighlighting(
                            HighlightingAttributeIds.LATE_BOUND_IDENTIFIER_ATTRIBUTE, range)));
                    }
                }

                var result = new DaemonStageResult(highlightings);
                committer(result);
            }

            public IDaemonProcess DaemonProcess { get; }
        }

        protected override IEnumerable<SpringFile> GetPsiFiles(IPsiSourceFile sourceFile)
        {
            yield return (SpringFile)
                (sourceFile ?? throw new ArgumentNullException(nameof(sourceFile)))
                .GetDominantPsiFile<SpringLanguage>();
        }
    }

    internal class TokenFactory : IPsiBuilderTokenFactory
    {
        public LeafElementBase CreateToken(TokenNodeType tokenNodeType, IBuffer buffer, int startOffset, int endOffset)
        {
            return tokenNodeType.Create(buffer, new TreeOffset(startOffset), new TreeOffset(endOffset));
        }
    }

    [ProjectFileType(typeof(SpringProjectFileType))]
    public class SelectEmbracingConstructProvider : ISelectEmbracingConstructProvider
    {
        public bool IsAvailable(IPsiSourceFile sourceFile)
        {
            return sourceFile.LanguageType.Is<SpringProjectFileType>();
        }

        public ISelectedRange GetSelectedRange(IPsiSourceFile sourceFile, DocumentRange documentRange)
        {
            var file = (SpringFile) sourceFile.GetDominantPsiFile<SpringLanguage>();
            var node = (file ?? throw new InvalidOperationException()).FindNodeAt(documentRange);
            return new SpringTreeNodeSelection(file, node);
        }

        public class SpringTreeNodeSelection : TreeNodeSelection<SpringFile>
        {
            public SpringTreeNodeSelection(SpringFile fileNode, ITreeNode node) : base(fileNode, node)
            {
            }

            public override ISelectedRange Parent => new SpringTreeNodeSelection(FileNode, TreeNode.Parent);
        }
    }
}