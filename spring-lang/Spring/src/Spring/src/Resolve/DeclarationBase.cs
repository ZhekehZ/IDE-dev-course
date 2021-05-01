using System.Collections.Generic;
using System.Xml;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;
using JetBrains.Util.DataStructures;

namespace JetBrains.ReSharper.Plugins.Spring.Resolve
{
    public abstract class DeclarationBase : CompositeElement, IDeclaration
    {
        public override PsiLanguageType Language => SpringLanguage.Instance;
        public XmlNode GetXMLDoc(bool inherit) => null;

        public void SetName(string name)
        {
        }

        public TreeTextRange GetNameRange() => this.GetTreeTextRange();
        public bool IsSynthetic() => false;
        public abstract IDeclaredElement DeclaredElement { get; }
        public string DeclaredName => GetText();
        public override bool IsValid() => true;
    }

    public abstract class SpringDeclared : CompositeElement, IDeclaredElement
    {
        private readonly IDeclaration _declaration;

        protected SpringDeclared(IDeclaration declaration)
        {
            _declaration = declaration;
        }

        public override IPsiServices GetPsiServices() => _declaration.GetPsiServices();
        public IList<IDeclaration> GetDeclarations() => new List<IDeclaration> {_declaration};

        public IList<IDeclaration> GetDeclarationsIn(IPsiSourceFile sourceFile)
        {
            var file = _declaration.GetSourceFile();
            if (file != null && file.Equals(sourceFile)) return new List<IDeclaration> {_declaration};
            return EmptyList<IDeclaration>.Instance;
        }

        public DeclaredElementType GetElementType() => CLRDeclaredElementType.FIELD;
        public XmlNode GetXMLDoc(bool inherit) => null;
        public XmlNode GetXMLDescriptionSummary(bool inherit) => null;
        public override bool IsValid() => _declaration.IsValid();
        public override NodeType NodeType => _declaration.NodeType;
        public override PsiLanguageType Language => SpringLanguage.Instance;
        public bool IsSynthetic() => false;

        public new HybridCollection<IPsiSourceFile> GetSourceFiles()
        {
            var file = _declaration.GetSourceFile();
            return file == null ? HybridCollection<IPsiSourceFile>.Empty : new HybridCollection<IPsiSourceFile>(file);
        }

        public new bool HasDeclarationsIn(IPsiSourceFile sourceFile) => sourceFile.Equals(_declaration.GetSourceFile());
        public string ShortName => _declaration.DeclaredName;
        public bool CaseSensitiveName => false;
        public PsiLanguageType PresentationLanguage => SpringLanguage.Instance;
    }
}