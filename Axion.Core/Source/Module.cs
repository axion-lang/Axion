using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;

namespace Axion.Core.Source {
    public class Module {
        public string Name => Path.Name.Trim().ToLower();

        public DirectoryInfo Path { get; }

        public Module? Parent { get; private set; }

        public Dictionary<string, Module> Submodules { get; } = new Dictionary<string, Module>();

        public Dictionary<string, Unit> Units { get; } = new Dictionary<string, Unit>();

        public string FullName => Parent == null ? Name : Parent.Name + "." + Name;

        private Dictionary<string, IDefinitionExpr> Definitions { get; } =
            new Dictionary<string, IDefinitionExpr>();

        internal ProcessingMode ProcessingMode = ProcessingMode.Default;

        /// <summary>
        ///     Initializes new Axion source code module from specified directory.
        /// </summary>
        public static Module Root(string dirPath) {
            var dir = new DirectoryInfo(dirPath);
            if (!dir.Exists) {
                throw new DirectoryNotFoundException("Module directory doesn't exist.");
            }

            return new Module(dir);
        }

        /// <summary>
        ///     Initializes new Axion source code module from specified directory.
        ///     If module is already initialized, returns it.
        /// </summary>
        public Module AddSubmodule(string moduleName) {
            var dirInfo = Path.GetDirectories().FirstOrDefault(d => d.Name == moduleName);
            if (Submodules.ContainsKey(dirInfo.FullName)) {
                return Submodules[dirInfo.FullName];
            }
            var module = new Module(
                dirInfo
             ?? throw new DirectoryNotFoundException("Submodule directory doesn't exist.")
            ) {
                Parent = this
            };
            Submodules.Add(module.FullName, module);
            return module;
        }

        /// <summary>
        ///     Initializes new Axion source code module from specified directory.
        /// </summary>
        public Unit AddUnit(string fileName) {
            Unit unit = Unit.FromFile(new FileInfo(fileName));
            unit.Module = this;
            Units.Add(unit);
            return unit;
        }

        private Module(DirectoryInfo dir) {
            Path = dir;
        }

        public void AddDefinition(IDefinitionExpr def) {
            if (def.Name == null) {
                throw new ArgumentException("Definition name cannot be null", nameof(def));
            }
            var name = def.Name.ToString();
            if (Definitions.ContainsKey(name)) {
                LangException.Report(BlameType.NameIsAlreadyDefined, def.Name);
            }
            else {
                Definitions.Add(name, def);
            }
        }

        public Dictionary<string, IDefinitionExpr> GetDefinitions() {
            Dictionary<string, IDefinitionExpr> defs = Definitions;
            if (Parent != null) {
                defs = new Dictionary<string, IDefinitionExpr>(
                    defs.Concat(Parent.GetDefinitions())
                );
            }
            return defs;
        }

        public IDefinitionExpr? FindDefinitionByName(string name) {
            IDefinitionExpr? def = Definitions.FirstOrDefault(kvp => kvp.Key == name).Value;
            if (def == null) {
                def = Parent?.FindDefinitionByName(name);
            }
            return def;
        }
    }
}
