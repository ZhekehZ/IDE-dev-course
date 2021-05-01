namespace JetBrains.ReSharper.Plugins.Spring.Lexer
{
    public enum Status
    {
        Ok,
        Fail
    }

    public abstract class ALexerResult<T>
    {
        public abstract int StartPosition { get; }
        public abstract int EndPosition { get; }
        public abstract string OriginalString { get; }
        public abstract string ErrorMessage { get; }
        public abstract T Result { get; }
        public abstract Status Status { get; }
        public int NextPosition => EndPosition;
        public int Length => EndPosition - StartPosition;
        public abstract int ParsedPos { get; }

        public string Value => Status == Status.Ok ? OriginalString.Substring(StartPosition, Length) : "";
    }
}