using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Nett;
using Nett.Coma;

namespace Axion.Core.Hierarchy {
    public class Project {
        public List<string> ImportedMacros {
            get {
                try {
                    var result = config.Get(
                        c => c.ImportedMacros,
                        null
                    );
                    if (result == null) {
                        result = new List<string>();
                        config.Set(c => c.ImportedMacros, result);
                    }

                    return result;
                }
                catch (KeyNotFoundException) {
                    var result = new List<string>();
                    config.Set(c => c.ImportedMacros, result);
                    return result;
                }
            }
        }

        public string? StdLibPath {
            get {
                try {
                    return config.Get(c => c.StdLibPath);
                }
                catch (KeyNotFoundException) {
                    return null;
                }
            }
        }

        public Module MainModule { get; }

        public FileInfo ConfigFile { get; }

        private readonly Config<ProjectConfig> config;

        private static readonly TomlSettings settings = TomlSettings.Create(
            s => s.ConfigurePropertyMapping(
                m => m.UseTargetPropertySelector(
                    standardSelectors => standardSelectors.IgnoreCase
                )
            )
        );

        public Project(string configPath) {
            ConfigFile = new FileInfo(configPath);
            if (!ConfigFile.Exists) {
                throw new FileNotFoundException(
                    "Specified project configuration file doesn't exist."
                );
            }

            config = Config.CreateAs()
                           .MappedToType(() => new ProjectConfig())
                           .StoredAs(
                               store => store.File(ConfigFile.FullName)
                                             .AccessedBySource(
                                                 "project",
                                                 out var _
                                             )
                           )
                           .UseTomlConfiguration(settings)
                           .Initialize();

            MainModule = Module.From(
                new DirectoryInfo(
                    Path.Join(ConfigFile.Directory!.FullName, "src")
                )
            );
            for (var i = 0; i < ImportedMacros.Count; i++) {
                var macro = ImportedMacros[i];
                if (!Path.IsPathRooted(macro)) {
                    macro = Path.Combine(ConfigFile.Directory.FullName, macro);
                }

                var fi = new FileInfo(macro);
                if (fi.Exists) {
                    MainModule.Bind(Unit.FromFile(fi));
                }
            }

            var stdLibPath = StdLibPath;
            if (stdLibPath != null) {
                if (!Path.IsPathRooted(stdLibPath)) {
                    stdLibPath = Path.Combine(
                        ConfigFile.Directory.FullName,
                        stdLibPath
                    );
                }

                var dir = new DirectoryInfo(stdLibPath);
                if (dir.Exists) {
                    var stdModule = Module.From(dir);
                    MainModule.Bind(stdModule);
                }
            }
        }
    }

    [SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
    public class ProjectConfig {
        public List<string>? ImportedMacros { get; }
        public string?       StdLibPath     { get; }
    }
}
