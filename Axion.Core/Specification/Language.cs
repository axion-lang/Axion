using System;

namespace Axion.Core.Specification {
    public enum Language {
        Axion,
        CSharp,
        Python,
        Pascal,
        DebuggingOutput
    }

    public static class LanguageToFileExtension {
        public static string ToFileExtension(this Language language) {
            return language switch {
                Language.Axion           => ".ax",
                Language.CSharp          => ".cs",
                Language.Python          => ".py",
                Language.Pascal          => ".pas",
                Language.DebuggingOutput => ".dbg.json",
                _                        => throw new NotImplementedException()
            };
        }
    }
}
