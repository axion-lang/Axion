using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Specification;

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
                Module? p = this;
                while (p?.Parent != null) {
                    p = p.Parent;
                }

                return p;
            }
        }

        public Dictionary<string, Module> Submodules { get; } =
            new Dictionary<string, Module>();

        public Dictionary<string, Unit> Units { get; } = new Dictionary<string, Unit>();

        public string Name => DirToModuleName(Directory);

        public string FullName =>
            Parent == null || string.IsNullOrWhiteSpace(Parent.Name)
                ? Name
                : Parent.Name + "." + Name;

        public bool IsEmpty => Units.Count == 0 && Submodules.Values.All(s => s.IsEmpty);

        public bool HasErrors => Units.Values.Any(u => u.HasErrors);

        public List<LangException> Blames =>
            Units.Values.Select(u => u.Blames).SelectMany(x => x).ToList();

        private readonly Dictionary<string, IDefinitionExpr> definitions =
            new Dictionary<string, IDefinitionExpr>();

        public Dictionary<string, IDefinitionExpr> Definitions {
            get => definitions;
            set {
                foreach ((string? name, IDefinitionExpr? def) in value) {
                    if (!definitions.ContainsKey(name)) {
                        definitions.Add(name, def);
                    }
                }
            }
        }

        public HashSet<string> CustomKeywords { get; } = new HashSet<string>();

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
            foreach (DirectoryInfo subDir in dir.EnumerateDirectories()) {
                Module subModule = From(subDir);
                if (!subModule.IsEmpty) {
                    module.Bind(subModule);
                }
            }

            foreach (FileInfo file in dir.EnumerateFiles()) {
                if (file.Extension.Equals(Language.Axion.ToFileExtension())
                 && Unit.NameFromFile(file) != null) {
                    module.Bind(Unit.FromFile(file));
                }
            }

            return module;
        }

        public Module Bind(Module module) {
            if (Submodules.TryGetValue(module.Name, out Module cached)) {
                return cached;
            }

            module.Parent = this;
            Submodules.Add(module.Name, module);
            return module;
        }

        public Unit Bind(Unit unit) {
            if (Units.TryGetValue(unit.Name, out Unit cached)) {
                return cached;
            }

            unit.Module = this;
            Units.Add(unit.Name, unit);
            return unit;
        }

        public Module BindByName(string moduleName) {
            string[] path   = moduleName.Split(".");
            Module   module = this;
            foreach (string step in path) {
                if (module.Submodules.TryGetValue(step, out Module subModule)) {
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
                LangException.Report(BlameType.NameIsAlreadyDefined, def.Name);
            }
            else {
                Definitions.Add(name, def);
            }
        }

        public IDefinitionExpr? FindDefinitionByName(string name) {
            IDefinitionExpr? def =
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
