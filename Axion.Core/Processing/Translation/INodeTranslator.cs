namespace Axion.Core.Processing.Translation {
    public interface INodeConverter {
        public string OutputFileExtension { get; }

        public bool Convert(CodeWriter writer, IConvertibleNode node);
    }
}
