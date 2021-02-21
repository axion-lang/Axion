using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Specification;

namespace Axion.Core.Hierarchy {
    public class Module {
        public DirectoryInfo Directory { get; }

        private DirectoryInfo? outputDirectory;

        /// <summary>
        ///     Path to directory where generated result is located.
        ///     Defaults to [source-directory]/../out.
        /// </summary>
        public DirectoryInfo OutputDirectory {
            get {
                if (outputDirectory != null) {
                    return outputDirectory;
                }

                outputDirectory = new DirectoryInfo(
                    Path.Combine(
                        Root.Directory.Parent.FullName,
                        "out",
                        Path.GetRelativePath(
                            Root.Directory.FullName,
                            Directory.FullName!
                        )
                    )
                );

                Utilities.ResolvePath(outputDirectory.FullName);
                return outputDirectory;
            }
        }

        public Module? Parent { get; private set; }

        public Module? Root {
            get {
                var p = this;
                while (p?.Parent != null) {
                    p = p.Parent;
                }

                return p;
            }
        }

        public Dictionary<string, Module> Submodules { get; } = new();

        public Dictionary<string, Unit> Units { get; } = new();

        public string Name => DirToModuleName(Directory);

        public string FullName =>
            Parent == null || string.IsNullOrWhiteSpace(Parent.Name)
                ? Name
                : Parent.Name + "." + Name;

        public bool IsEmpty =>
            Units.Count == 0 && Submodules.Values.All(s => s.IsEmpty);

        public bool HasErrors => Units.Values.Any(u => u.HasErrors);

        public List<LanguageReport> Blames =>
            Units.Values.Select(u => u.Blames).SelectMany(x => x).ToList();

        private readonly Dictionary<string, IDefinitionExpr> definitions = new();

        public Dictionary<string, IDefinitionExpr> Definitions {
            get => definitions;
            set {
                foreach (var (name, def) in value) {
                    if (!definitions.ContainsKey(name)) {
                        definitions.Add(name, def);
                    }
                }
            }
        }

        public HashSet<string> CustomKeywords { get; } = new();

        private Module(DirectoryInfo dir) {
            Directory = dir;
        }

        /// <summary>
        ///     Initializes a new Axion submodule from specified directory.
        ///     Any units or submodules of this module
        ///     are NOT automatically bound.
        /// </summary>
        public static Module RawFrom(DirectoryInfo dir) {
            if (!dir.Exists) {
                throw new DirectoryNotFoundException(
                    "Module directory doesn't exist: " + dir.FullName
                );
            }

            return new Module(dir);
        }

        /// <summary>
        ///     Recursively initializes a new Axion submodule
        ///     and all it's child submodules from specified directory.
        ///     Returns cached module if it is already initialized.
        /// </summary>
        public static Module From(DirectoryInfo dir) {
            if (!dir.Exists) {
                throw new DirectoryNotFoundException(
                    "Module directory doesn't exist: " + dir.FullName
                );
            }

            var module = new Module(dir);
            foreach (var subDir in dir.EnumerateDirectories()) {
                var subModule = From(subDir);
                if (!subModule.IsEmpty) {
                    module.Bind(subModule);
                }
            }

            foreach (var file in dir.EnumerateFiles()) {
                if (file.Extension.Equals(Spec.FileExtension)
                 && Unit.NameFromFile(file) != null) {
                    module.Bind(Unit.FromFile(file));
                }
            }

            return module;
        }

        public Module Bind(Module module) {
            if (Submodules.TryGetValue(module.Name, out var cached)) {
                return cached;
            }

            module.Parent = this;
            Submodules.Add(module.Name, module);
            return module;
        }

        public Unit Bind(Unit unit) {
            if (Units.TryGetValue(unit.Name, out var cached)) {
                return cached;
            }

            unit.Module = this;
            Units.Add(unit.Name, unit);
            return unit;
        }

        public Module BindByName(string moduleName) {
            var path   = moduleName.Split(".");
            var module = this;
            foreach (var step in path) {
                if (module.Submodules.TryGetValue(step, out var subModule)) {
                    module = subModule;
                }
                else {
                    module.Bind(
                        From(
                            new DirectoryInfo(
                                Path.Combine(module.Directory.FullName, step)
                            )
                        )
                    );
                }
            }

            return module;
        }

        public void AddDefinition(IDefinitionExpr def) {
            if (def.Name == null) {
                throw new ArgumentException(
                    "Definition name cannot be null",
                    nameof(def)
                );
            }

            var name = def.Name.ToString();
            if (Definitions.ContainsKey(name)) {
                LanguageReport.To(BlameType.NameIsAlreadyDefined, def.Name);
            }
            else {
                Definitions.Add(name, def);
            }
        }

        public IDefinitionExpr? FindDefinitionByName(string name) {
            var def =
                Definitions.FirstOrDefault(kvp => kvp.Key == name).Value
             ?? Parent?.FindDefinitionByName(name);
            return def;
        }

        public void RegisterCustomKeyword(string keyword) {
            if (!Spec.Keywords.ContainsKey(keyword)
             && !Spec.Operators.ContainsKey(keyword)
             && !Spec.Punctuation.ContainsKey(keyword)) {
                CustomKeywords.Add(keyword);
            }
        }

        public static string DirToModuleName(DirectoryInfo dir) {
            var dn = dir.Name.Trim().ToLower();
            return dn == "src" ? "" : dn;
        }
    }
}
