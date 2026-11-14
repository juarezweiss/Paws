using System;
using System.IO;
using UnityEditor.iOS.Xcode;
using CatsAndArrows.UI.Editor;
using NUnit.Framework;
using UnityEditor;

namespace CatsAndArrows.Tests
{
    // #90: production exclusions must be scoped, and standalone access preserved.
    public sealed class FreeProductionBuildTests
    {
        [TestCase("Unity.Purchasing.dll")]
        [TestCase("Unity.Purchasing.Stores.dll")]
        [TestCase("Unity.Services.Core.dll")]
        [TestCase("GoogleMobileAds.Ump.iOS.dll")]
        [TestCase("CatsAndArrows.Purchasing.Apple.dll")]
        [TestCase("CatsAndArrows.Purchasing.Apple.Review.dll")]
        [TestCase("CatsAndArrows.Advertising.dll")]
        public void CommercialAssembliesAreExcludedOnlyWithinFreeScope(string commercial)
        {
            var filter = new FreeProductionBuild();
            var input = new[] { commercial, "CatsAndArrows.Access.dll", "CatsAndArrows.UI.dll" };
            Assert.That(filter.OnFilterAssemblies(BuildOptions.None, input), Is.SameAs(input));
            using (FreeProductionBuild.Begin())
            {
                Assert.That(filter.OnFilterAssemblies(BuildOptions.None, input),
                    Is.EqualTo(new[] { "CatsAndArrows.Access.dll", "CatsAndArrows.UI.dll" }));
                Assert.Throws<InvalidOperationException>(() => FreeProductionBuild.Begin());
            }
            Assert.That(filter.OnFilterAssemblies(BuildOptions.None, input), Is.SameAs(input));
        }

        [TestCase("Packages/com.unity.purchasing/Plugins/UnityPurchasing/iOS/UnityPurchasing.m", true)]
        [TestCase("Packages/com.unity.services.core/Runtime/Device/UserIdProviders/NSUserDefaults.mm", true)]
        [TestCase("Assets/CatsAndArrows/Purchasing/Apple/Plugins/iOS/PawsAppleOfferCode.swift", true)]
        [TestCase("Assets/ThirdParty/GoogleMobileAds/GoogleMobileAds.iOS.dll", true)]
        [TestCase("Assets/CatsAndArrows/Access/Plugins/iOS/PawsNativeAccess.swift", false)]
        [TestCase("Assets/CatsAndArrows/Access/Plugins/iOS/PawsAccessWorkflow.swift", false)]
        public void NativePluginRulesPreserveStandaloneAccess(string path, bool excluded) =>
            Assert.That(FreeProductionBuild.ExcludesPlugin(path), Is.EqualTo(excluded));

        [Test]
        public void ExportCorrectionPreservesNativeAccessAndOrdinaryResources()
        {
            var directory = Path.Combine(Path.GetTempPath(), "paws-free-export-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(directory, "Unity-iPhone.xcodeproj"));
            Directory.CreateDirectory(Path.Combine(directory, "Data/Raw"));
            try
            {
                var path = PBXProject.GetPBXProjectPath(directory);
                File.WriteAllText(path, Fixture);
                var project = new PBXProject(); project.ReadFromFile(path);
                var app = project.GetUnityMainTargetGuid();
                var framework = project.GetUnityFrameworkTargetGuid();
                project.AddFrameworkToProject(app, "StoreKit.framework", false);
                project.AddFrameworkToProject(framework, "StoreKit.framework", false);
                project.AddFrameworkToProject(framework, "DeclaredAgeRange.framework", true);
                project.AddFrameworkToProject(framework, "PermissionKit.framework", true);
                project.SetBuildProperty(app, "CODE_SIGN_ENTITLEMENTS", "PawsAccess.entitlements");
                project.WriteToFile(path);
                var entitlements = new PlistDocument();
                entitlements.root.SetBoolean("com.apple.developer.declared-age-range", true);
                var entitlementPath = Path.Combine(directory, "PawsAccess.entitlements");
                entitlements.WriteToFile(entitlementPath);
                var originalEntitlement = File.ReadAllText(entitlementPath);
                File.WriteAllText(Path.Combine(directory, "Data/Raw/UnityServicesProjectConfiguration.json"), "{}");
                var ordinary = Path.Combine(directory, "Data/Raw/ordinary.json");
                File.WriteAllText(ordinary, "ordinary");
                FreeProductionBuild.ConfigureExport(directory);
                FreeProductionBuild.ConfigureExport(directory);
                project.ReadFromFile(path);
                Assert.That(project.ContainsFramework(app, "StoreKit.framework"), Is.False);
                Assert.That(project.ContainsFramework(framework, "StoreKit.framework"), Is.False);
                Assert.That(project.ContainsFramework(framework, "DeclaredAgeRange.framework"), Is.True);
                Assert.That(project.ContainsFramework(framework, "PermissionKit.framework"), Is.True);
                Assert.That(project.WriteToString().Split(new[] { "Weak" }, StringSplitOptions.None).Length - 1, Is.EqualTo(2));
                Assert.That(project.GetBuildPropertyForAnyConfig(app, "CODE_SIGN_ENTITLEMENTS"), Is.EqualTo("PawsAccess.entitlements"));
                Assert.That(File.ReadAllText(entitlementPath), Is.EqualTo(originalEntitlement));
                Assert.That(File.ReadAllText(ordinary), Is.EqualTo("ordinary"));
                Assert.That(File.Exists(Path.Combine(directory, "Data/Raw/UnityServicesProjectConfiguration.json")), Is.False);
            }
            finally { Directory.Delete(directory, true); }
        }
        [Test]
        public void GeneratedSettingsCleanupPreservesPreexistingAndNonemptyFiles()
        {
            var path = Path.Combine(Path.GetTempPath(), "paws-settings-" + Guid.NewGuid().ToString("N"));
            try
            {
                File.WriteAllText(path, "");
                FreeProductionBuild.RemoveNewEmptySettings(path, true);
                Assert.That(File.Exists(path), Is.True);
                File.WriteAllText(path, "owner-settings");
                FreeProductionBuild.RemoveNewEmptySettings(path, false);
                Assert.That(File.ReadAllText(path), Is.EqualTo("owner-settings"));
                File.WriteAllText(path, "");
                FreeProductionBuild.RemoveNewEmptySettings(path, false);
                Assert.That(File.Exists(path), Is.False);
                FreeProductionBuild.RemoveNewEmptySettings(path, false);
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }

        [Test]
        public void InitializerCleanupNeverDeletesOrdinaryOrMixedCallbacks()
        {
            var path = Path.Combine(Path.GetTempPath(), "paws-initializers-" + Guid.NewGuid().ToString("N"));
            try
            {
                var ordinary = "{\"root\":[{\"assemblyName\":\"CatsAndArrows.Access\",\"methodName\":\"Required\"}]}";
                File.WriteAllText(path, ordinary);
                FreeProductionBuild.RemoveCommercialInitializers(path);
                Assert.That(File.ReadAllText(path), Is.EqualTo(ordinary));
                var mixed = "{\"root\":[{\"assemblyName\":\"CatsAndArrows.Access\"},{\"assemblyName\":\"Unity.Purchasing\"}]}";
                File.WriteAllText(path, mixed);
                Assert.Throws<InvalidOperationException>(() => FreeProductionBuild.RemoveCommercialInitializers(path));
                Assert.That(File.ReadAllText(path), Is.EqualTo(mixed));
                File.WriteAllText(path, "{\"root\":[{\"assemblyName\":\"Unity.Purchasing\"},{\"assemblyName\":\"Unity.Services.Core\"}]}");
                FreeProductionBuild.RemoveCommercialInitializers(path);
                Assert.That(File.Exists(path), Is.False);
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }

        private const string Fixture = @"// !$*UTF8*$!
{
 archiveVersion = 1;
 classes = {};
 objectVersion = 56;
 objects = {
 111111111111111111111111 = {isa = PBXProject; attributes = {}; buildConfigurationList = AAAAAAAAAAAAAAAAAAAAAAAA; compatibilityVersion = ""Xcode 3.2""; mainGroup = 444444444444444444444444; productRefGroup = 555555555555555555555555; targets = (222222222222222222222222, 333333333333333333333333);};
 222222222222222222222222 = {isa = PBXNativeTarget; name = ""Unity-iPhone""; productName = ""Unity-iPhone""; buildConfigurationList = BBBBBBBBBBBBBBBBBBBBBBBB; buildPhases = (666666666666666666666666); productType = ""com.apple.product-type.application"";};
 333333333333333333333333 = {isa = PBXNativeTarget; name = UnityFramework; productName = UnityFramework; buildConfigurationList = CCCCCCCCCCCCCCCCCCCCCCCC; buildPhases = (777777777777777777777777); productType = ""com.apple.product-type.framework"";};
 444444444444444444444444 = {isa = PBXGroup; children = (555555555555555555555555); sourceTree = ""<group>"";};
 555555555555555555555555 = {isa = PBXGroup; children = (); name = Products; sourceTree = ""<group>"";};
 666666666666666666666666 = {isa = PBXFrameworksBuildPhase; buildActionMask = 2147483647; files = (); runOnlyForDeploymentPostprocessing = 0;};
 777777777777777777777777 = {isa = PBXFrameworksBuildPhase; buildActionMask = 2147483647; files = (); runOnlyForDeploymentPostprocessing = 0;};
 AAAAAAAAAAAAAAAAAAAAAAAA = {isa = XCConfigurationList; buildConfigurations = (DDDDDDDDDDDDDDDDDDDDDDDD); defaultConfigurationIsVisible = 0; defaultConfigurationName = Release;};
 BBBBBBBBBBBBBBBBBBBBBBBB = {isa = XCConfigurationList; buildConfigurations = (EEEEEEEEEEEEEEEEEEEEEEEE); defaultConfigurationIsVisible = 0; defaultConfigurationName = Release;};
 CCCCCCCCCCCCCCCCCCCCCCCC = {isa = XCConfigurationList; buildConfigurations = (FFFFFFFFFFFFFFFFFFFFFFFF); defaultConfigurationIsVisible = 0; defaultConfigurationName = Release;};
 DDDDDDDDDDDDDDDDDDDDDDDD = {isa = XCBuildConfiguration; name = Release; buildSettings = {};};
 EEEEEEEEEEEEEEEEEEEEEEEE = {isa = XCBuildConfiguration; name = Release; buildSettings = {IPHONEOS_DEPLOYMENT_TARGET = 16.3;};};
 FFFFFFFFFFFFFFFFFFFFFFFF = {isa = XCBuildConfiguration; name = Release; buildSettings = {IPHONEOS_DEPLOYMENT_TARGET = 16.3;};};
 };
 rootObject = 111111111111111111111111;
}";
    }
}
