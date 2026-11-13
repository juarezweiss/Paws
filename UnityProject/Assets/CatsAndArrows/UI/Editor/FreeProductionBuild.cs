using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace CatsAndArrows.UI.Editor
{
    // #90: only the free production entry point opts into these exclusions.
    // Review/future monetized builds retain the original source, imports and pins.
    public sealed class FreeProductionBuild : IFilterBuildAssemblies
    {
        private static bool active;
        public int callbackOrder => int.MaxValue;

        public static bool ExcludesAssembly(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path);
            return name.StartsWith("Unity.Purchasing", StringComparison.Ordinal) ||
                name.StartsWith("Unity.Services.", StringComparison.Ordinal) ||
                name.StartsWith("GoogleMobileAds", StringComparison.Ordinal) ||
                name.StartsWith("CatsAndArrows.Purchasing.", StringComparison.Ordinal) ||
                name.StartsWith("CatsAndArrows.Advertising", StringComparison.Ordinal);
        }

        public static bool ExcludesPlugin(string path) =>
            path.StartsWith("Packages/com.unity.purchasing/", StringComparison.Ordinal) ||
            path.StartsWith("Packages/com.unity.services.core/", StringComparison.Ordinal) ||
            path.StartsWith("Assets/CatsAndArrows/Purchasing/", StringComparison.Ordinal) ||
            path.StartsWith("Assets/ThirdParty/GoogleMobileAds/", StringComparison.Ordinal);

        public string[] OnFilterAssemblies(BuildOptions options, string[] assemblies) =>
            active ? assemblies.Where(path => !ExcludesAssembly(path)).ToArray() : assemblies;

        public static IDisposable Begin()
        {
            if (active) throw new InvalidOperationException("Free production exclusion scope is already active.");
            return new Scope();
        }

        private sealed class Scope : IDisposable
        {
            private readonly string generatedSettings = Path.Combine(Path.GetDirectoryName(Application.dataPath),
                "ProjectSettings/Packages/com.unity.services.core/Settings.json");
            private readonly bool settingsExisted;
            private readonly List<PluginImporter> plugins = new List<PluginImporter>();
            public Scope()
            {
                settingsExisted = File.Exists(generatedSettings);
                active = true;
                foreach (var plugin in PluginImporter.GetAllImporters())
                {
                    if (!ExcludesPlugin(plugin.assetPath)) continue;
                    plugin.SetIncludeInBuildDelegate(_ => false);
                    plugins.Add(plugin);
                }
            }
            public void Dispose()
            {
                foreach (var plugin in plugins) plugin.SetIncludeInBuildDelegate(null);
                active = false;
                RemoveNewEmptySettings(generatedSettings, settingsExisted);
            }
        }

        // The services editor package creates a zero-byte file during export.
        // Remove only that new empty scratch file; never hide or overwrite settings.
        public static void RemoveNewEmptySettings(string path, bool existedBeforeBuild)
        {
            if (!existedBeforeBuild && File.Exists(path) && new FileInfo(path).Length == 0) File.Delete(path);
        }

        [Serializable] private sealed class Initializers { public Initializer[] root; }
        [Serializable] private sealed class Initializer { public string assemblyName; }

        public static void RemoveCommercialInitializers(string path)
        {
            if (!File.Exists(path)) return;
            var initializers = JsonUtility.FromJson<Initializers>(File.ReadAllText(path));
            if (initializers?.root == null || initializers.root.Any(item => item == null || string.IsNullOrEmpty(item.assemblyName)))
                throw new InvalidOperationException("Invalid runtime initializer inventory.");
            if (!initializers.root.Any(item => ExcludesAssembly(item.assemblyName + ".dll"))) return;
            // This pinned Unity export contains only commercial initializers. Fail
            // on a future mixed list rather than delete an ordinary/native callback.
            if (initializers.root.Any(item => !ExcludesAssembly(item.assemblyName + ".dll")))
                throw new InvalidOperationException("Mixed runtime initializers require reviewed build filtering.");
            File.Delete(path);
        }

        public static void ConfigureExport(string output)
        {
            // The installed IAP package's editor hook adds StoreKit even when its
            // runtime is excluded. Remove that generated input, never the package.
            var path = PBXProject.GetPBXProjectPath(output);
            var project = new PBXProject(); project.ReadFromFile(path);
            foreach (var target in new[] { project.GetUnityMainTargetGuid(), project.GetUnityFrameworkTargetGuid() })
                project.RemoveFrameworkFromProject(target, "StoreKit.framework");
            project.WriteToFile(path);
            // Services injects this streaming asset independently of player assemblies.
            var config = Path.Combine(output, "Data/Raw/UnityServicesProjectConfiguration.json");
            if (File.Exists(config)) File.Delete(config);
            RemoveCommercialInitializers(Path.Combine(output, "Data/RuntimeInitializeOnLoads.json"));
        }
    }
}
