namespace Axion.Core.Processing.Translation {
    public interface INodeTranslator {
        public string OutputFileExtension { get; }

        public bool Translate(CodeWriter writer, ITranslatableNode node);
    }
}
