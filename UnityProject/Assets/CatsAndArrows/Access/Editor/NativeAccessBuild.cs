using System;
using System.IO;
using UnityEditor.iOS.Xcode;

namespace CatsAndArrows.Access.Editor
{
    // #103: export-only configuration. Does not sign, provision, or alter accounts.
    public static class NativeAccessBuild
    {
        public const string Entitlement = "com.apple.developer.declared-age-range";
        public const string EntitlementsPath = "PawsAccess.entitlements";
        public static void Configure(string exportPath)
        {
            var projectPath = PBXProject.GetPBXProjectPath(exportPath);
            var project = new PBXProject(); project.ReadFromFile(projectPath);
            var app = project.GetUnityMainTargetGuid();
            var framework = project.GetUnityFrameworkTargetGuid();
            if (string.IsNullOrEmpty(app) || string.IsNullOrEmpty(framework))
                throw new InvalidOperationException("Expected Unity iOS application and framework targets.");
            var entitlementPath = project.GetBuildPropertyForAnyConfig(app, "CODE_SIGN_ENTITLEMENTS");
            if (string.IsNullOrWhiteSpace(entitlementPath)) entitlementPath = EntitlementsPath;
            if (Path.IsPathRooted(entitlementPath) || entitlementPath.Contains("..") || entitlementPath.Contains("$("))
                throw new InvalidOperationException("Access entitlements must use a project-relative path.");
            var fullPath = Path.Combine(exportPath, entitlementPath);
            var entitlements = new PlistDocument();
            if (File.Exists(fullPath)) entitlements.ReadFromFile(fullPath);
            entitlements.root.SetBoolean(Entitlement, true);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            entitlements.WriteToFile(fullPath);
            if (string.IsNullOrEmpty(project.FindFileGuidByProjectPath(entitlementPath)))
                project.AddFile(entitlementPath, entitlementPath, PBXSourceTree.Source);
            project.SetBuildProperty(app, "CODE_SIGN_ENTITLEMENTS", entitlementPath);
            foreach (var name in new[] { "DeclaredAgeRange.framework", "PermissionKit.framework" })
                project.AddFrameworkToProject(framework, name, true);
            project.SetBuildProperty(framework, "SWIFT_VERSION", "5.0");
            project.SetBuildProperty(app, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");
            // This Unity Xcode API predates PBXCapabilityType.DeclaredAgeRange.
            // The app entitlement declares the capability; the owner enables it in
            // the App ID/profile before any separately authorized signed build.
            project.WriteToFile(projectPath);
        }
    }
}
