using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.iOS.Xcode;

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
            private readonly List<PluginImporter> plugins = new List<PluginImporter>();
            public Scope()
            {
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
            }
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
        }
    }
}
