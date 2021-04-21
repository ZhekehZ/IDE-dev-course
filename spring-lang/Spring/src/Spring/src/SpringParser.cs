using System;
using System.Collections.Generic;
using JetBrains.Application.Settings;
using JetBrains.DocumentModel;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon.CSharp.Errors;
using JetBrains.ReSharper.Daemon.Css.Stages;
using JetBrains.ReSharper.Daemon.VisualElements;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Feature.Services.SelectEmbracingConstruct;
using JetBrains.ReSharper.I18n.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Impl.Resolve.Filters;
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
using static JetBrains.ReSharper.Plugins.Spring.SpringTokenType;

namespace JetBrains.ReSharper.Plugins.Spring
{
    internal class SpringParser : IParser
    {
        private readonly ILexer _myLexer;
        private readonly Func<PsiBuilder, bool> statement = StatementParser();
        
        public SpringParser(ILexer lexer)
        {
            _myLexer = lexer;
        }

        public IFile ParseFile()
        {
            using var def = Lifetime.Define();
            var builder = new PsiBuilder(_myLexer, SpringFileNodeType.Instance, new TokenFactory(), def.Lifetime);
            var fileMark = builder.Mark();

            while (!builder.Eof() && (builder.GetTokenType().IsWhitespace || builder.GetTokenType().IsComment)) 
                builder.AdvanceLexer();
            if (!builder.Eof())
            {
                statement(builder);
            }

            if (!builder.Eof())
            {
                builder.Error("Invalid statement");
                while (!builder.Eof())
                {
                    Next(builder);                    
                }
            }

            builder.Done(fileMark, SpringFileNodeType.Instance, null);
            var file = (IFile)builder.BuildTree();
            return file;
        }

        private static Func<PsiBuilder, bool> ExpressionParser()
        {
            Func<PsiBuilder, bool> expression = null;
            // ReSharper disable once AccessToModifiedClosure
            // ReSharper disable once PossibleNullReferenceException
            bool Expr(PsiBuilder b) => expression(b); // for recursive calls

            Func<PsiBuilder, bool> factor = null;
            // ReSharper disable once AccessToModifiedClosure
            // ReSharper disable once PossibleNullReferenceException
            bool Fact(PsiBuilder b) => factor(b); // for recursive calls
            
            var constant = Alt(TInt, TStr, TIdent, KwNil, TReal);
            
            factor = AltForce(
                Seq(OPOpen, Expr, OPClose),
                Seq(TIdent, OptForce("Expression list expected", Seq(OPOpen, Many(Expr, OComma), OPClose))),
                constant,
                Seq(OAt, TIdent),
                Seq(OBOpen, Many(Seq(Expr, OptForce("Expression expected after ..", Seq(ODots, Expr))), OComma), OBClose),
                Seq(KwNot, Fact),
                Seq(OPlus, Fact),
                Seq(OMinus, Fact)
            );

            var term = ManyForce("Operand expected", factor, Alt(OMult, ODiv, KwDiv, KwMod, KwAnd, KwShl, KwShr, KwAs), true);
            var simpleExpr = ManyForce("Operand expected", term, Alt(OPlus, OMinus, KwOr, KwXor), true);
            expression = ManyForce("Operand expected", simpleExpr, Alt(OLt, OGt, OLe, OGe, OEq, ONeq, KwIn, KwIs));
            
            return expression;
        }

        private static Func<PsiBuilder, bool> StatementParser()
        {
            Func<PsiBuilder, bool> statement = null;
            // ReSharper disable once AccessToModifiedClosure
            // ReSharper disable once PossibleNullReferenceException
            bool Stmt(PsiBuilder b) => statement(b); // for recursive calls
            var expression = ExpressionParser();
            
            var identStmt = Seq(TIdent, 
                AltForce(Seq(OPOpen, Many(expression, OComma), OPClose),
                         Seq(Alt(OAssign, OAsgMns, OAsgPls, OAsgMul, OAsgDiv), expression)));
            var gotoStmt = Seq(KwGoto, TIdent);

            var asmStmt = Seq(KwAsm,
                b =>
                {
                    var m = b.Mark();
                    while (!b.Eof())
                    {
                        if (b.GetTokenType() == KEYWORD && b.GetTokenText() == "end")
                        {
                            b.AlterToken(m, ASM_TEXT);
                            return true;
                        }
                        Next(b);
                    }
                    b.Error("end expected");
                    return false;
                }, KwEnd, OptForce("At least one string argument expected", 
                                        Seq(OBOpen, Many(TStr, OComma, true), OBClose)));

            var withStmt = Seq(KwWith, Many(TIdent, OComma, true), KwDo, Stmt);
            var whileStmt = Seq(KwWhile, expression, KwDo, Stmt);
            var repeatStmt = Seq(KwRepeat, Many(Stmt, OSemic, true), KwUntil, expression);

            var forStmt = Seq(
                KwFor, TIdent, 
                AltForce(Seq(OAssign, expression, Alt(KwTo, KwDownto), expression),Seq(KwIn, expression)),
                KwDo, Stmt
            );
            var ifStmt = Seq(KwIf, expression, KwThen, Stmt, OptForce("Invalid else statement", Seq(KwElse, Stmt)));

            var constant = Alt(TStr, TInt, TIdent);
            var caseStmt = Seq(
                KwCase, expression, KwOf,
                Many(Seq(Many(Seq(constant, OptForce("constant expected after ..", Seq(ODots, constant))), OComma, true), 
                    OColon, Stmt), OSemic, true),
                Opt(OSemic), OptForce("Invalid else statement list", Seq(Alt(KwElse, KwOtherwise), Many(Stmt, OSemic))),
                Opt(OSemic), KwEnd
            );
            var compound = Seq(KwBegin, Many(Stmt, OSemic), Opt(OSemic), KwEnd);

            statement = AltForce(compound, ifStmt, repeatStmt, whileStmt, withStmt, asmStmt, gotoStmt, forStmt, caseStmt, identStmt);

            return statement;
        }
    }

    [DaemonStage]
    class SpringDaemonStage : DaemonStageBase<SpringFile>
    {
        protected override IDaemonStageProcess CreateDaemonProcess(IDaemonProcess process, DaemonProcessKind processKind, SpringFile file,
            IContextBoundSettingsStore settingsStore)
        {
            return new SpringDaemonProcess(process, file);
        }

        internal class SpringDaemonProcess : IDaemonStageProcess
        {
            private readonly SpringFile myFile;
            public SpringDaemonProcess(IDaemonProcess process, SpringFile file)
            {
                myFile = file;
                DaemonProcess = process;
            }

            public void Execute(Action<DaemonStageResult> committer)
            {
                var highlightings = new List<HighlightingInfo>();
                foreach (var treeNode in myFile.Descendants())
                {
                    if (treeNode is PsiBuilderErrorElement error)
                    {
                        var range = error.GetDocumentRange();
                        highlightings.Add(new HighlightingInfo(range, new CSharpSyntaxError(error.ErrorDescription, range)));
                    }
                }
                
                var result = new DaemonStageResult(highlightings);
                committer(result);
            }

            public IDaemonProcess DaemonProcess { get; }
        }

        protected override IEnumerable<SpringFile> GetPsiFiles(IPsiSourceFile sourceFile)
        {
            yield return (SpringFile)sourceFile.GetDominantPsiFile<SpringLanguage>();
        }
    } 

    internal class TokenFactory : IPsiBuilderTokenFactory
    {
        public LeafElementBase CreateToken(TokenNodeType tokenNodeType, IBuffer buffer, int startOffset, int endOffset)
        {
            return tokenNodeType.Create(buffer, new TreeOffset(startOffset), new TreeOffset(endOffset));
        }
    }

    [ProjectFileType(typeof (SpringProjectFileType))]
    public class SelectEmbracingConstructProvider : ISelectEmbracingConstructProvider
    {
        public bool IsAvailable(IPsiSourceFile sourceFile)
        {
            return sourceFile.LanguageType.Is<SpringProjectFileType>();
        }

        public ISelectedRange GetSelectedRange(IPsiSourceFile sourceFile, DocumentRange documentRange)
        {
            var file = (SpringFile) sourceFile.GetDominantPsiFile<SpringLanguage>();
            var node = file.FindNodeAt(documentRange);
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
