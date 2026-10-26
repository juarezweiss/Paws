using System;
using System.IO;
using UnityEditor.iOS.Xcode;

namespace CatsAndArrows.UI.Editor
{
    // #67: app-only capability, weak OS framework link, no profile/account setup.
    public static class NativeAgeRangeBuild
    {
        public const string Entitlement = "com.apple.developer.declared-age-range";
        public const string EntitlementsPath = "PawsAgeRange.entitlements";
        public const string SwiftCondition = "PAWS_APPLE_AGE_RANGE";

        public static PlistDocument CreateEntitlements()
        {
            var entitlements = new PlistDocument();
            entitlements.root.SetBoolean(Entitlement, true);
            return entitlements;
        }

        public static void Add(string output)
        {
            var projectPath = PBXProject.GetPBXProjectPath(output);
            var project = new PBXProject();
            project.ReadFromFile(projectPath);
            var app = project.GetUnityMainTargetGuid();
            var framework = project.GetUnityFrameworkTargetGuid();
            var existing = project.GetBuildPropertyForAnyConfig(app, "CODE_SIGN_ENTITLEMENTS");
            if (!string.IsNullOrEmpty(existing))
                throw new InvalidOperationException("Review existing application entitlements before enabling age assurance.");
            var path = Path.Combine(output, EntitlementsPath);
            if (File.Exists(path)) throw new InvalidOperationException("Generated age entitlement file already exists.");
            CreateEntitlements().WriteToFile(path);
            project.AddFile(EntitlementsPath, EntitlementsPath, PBXSourceTree.Source);
            project.SetBuildProperty(app, "CODE_SIGN_ENTITLEMENTS", EntitlementsPath);
            project.AddFrameworkToProject(framework, "DeclaredAgeRange.framework", true);
            project.AddBuildProperty(framework, "SWIFT_ACTIVE_COMPILATION_CONDITIONS", SwiftCondition);
            project.WriteToFile(projectPath);
        }
    }
}
