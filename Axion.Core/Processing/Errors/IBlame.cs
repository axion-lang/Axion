namespace Axion.Core.Processing.Errors {
    public interface IBlame {
        (int line, int column) StartPosition { get; }
        (int line, int column) EndPosition   { get; }

        string AsMessage();
    }
}