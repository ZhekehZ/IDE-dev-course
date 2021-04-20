using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;

namespace JetBrains.ReSharper.Plugins.Spring
{
    [LanguageDefinition(Name)]
    public class SpringLanguage : KnownLanguage
    {
        public new const string Name = "Spring";
    
        public static SpringLanguage Instance { get; private set; }

        private SpringLanguage() : base(Name, Name)
        {
        }

        protected SpringLanguage([NotNull] string name) : base(name)
        {
        }

        protected SpringLanguage([NotNull] string name, [NotNull] string presentableName) : base(name, presentableName)
        {
        }
    }
}