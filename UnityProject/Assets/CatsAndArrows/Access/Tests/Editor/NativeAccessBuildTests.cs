using System;
using System.IO;
using CatsAndArrows.Access.Editor;
using NUnit.Framework;
using UnityEditor.iOS.Xcode;

namespace CatsAndArrows.Access.Tests
{
    public sealed class NativeAccessBuildTests
    {
        private string directory;
        private const string App = "222222222222222222222222";
        private const string Framework = "333333333333333333333333";
        [SetUp] public void SetUp()
        {
            directory = Path.Combine(Path.GetTempPath(), "paws-access-build-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(directory, "Unity-iPhone.xcodeproj"));
            File.WriteAllText(PBXProject.GetPBXProjectPath(directory), Fixture);
        }
        [TearDown] public void TearDown() { Directory.Delete(directory, true); }
        [Test]
        public void EntitlementBelongsOnlyToAppAndSystemFrameworksAreWeak()
        {
            NativeAccessBuild.Configure(directory);
            NativeAccessBuild.Configure(directory);
            var project = new PBXProject(); project.ReadFromFile(PBXProject.GetPBXProjectPath(directory));
            Assert.That(project.GetBuildPropertyForAnyConfig(App, "CODE_SIGN_ENTITLEMENTS"), Is.EqualTo(NativeAccessBuild.EntitlementsPath));
            Assert.That(project.GetBuildPropertyForAnyConfig(Framework, "CODE_SIGN_ENTITLEMENTS"), Is.Null.Or.Empty);
            var entitlement = new PlistDocument(); entitlement.ReadFromFile(Path.Combine(directory, NativeAccessBuild.EntitlementsPath));
            Assert.That(entitlement.root[NativeAccessBuild.Entitlement].AsBoolean(), Is.True);
            Assert.That(project.GetBuildPropertyForAnyConfig(App, "IPHONEOS_DEPLOYMENT_TARGET"), Is.EqualTo("16.3"));
            var text = project.WriteToString();
            Assert.That(text, Does.Contain("DeclaredAgeRange.framework"));
            Assert.That(text, Does.Contain("PermissionKit.framework"));
            Assert.That(text, Does.Contain("Weak"));
            Assert.That(text, Does.Not.Contain("com.apple.developer.ubiquity-kvstore-identifier"));
        }
        [Test]
        public void ExistingAppEntitlementsRemainAndUnsafePathsAreRejected()
        {
            var project = new PBXProject(); project.ReadFromFile(PBXProject.GetPBXProjectPath(directory));
            project.SetBuildProperty(App, "CODE_SIGN_ENTITLEMENTS", "Existing.entitlements");
            project.WriteToFile(PBXProject.GetPBXProjectPath(directory));
            var original = new PlistDocument(); original.root.SetBoolean("existing.owner.capability", true);
            original.WriteToFile(Path.Combine(directory, "Existing.entitlements"));
            NativeAccessBuild.Configure(directory);
            var result = new PlistDocument(); result.ReadFromFile(Path.Combine(directory, "Existing.entitlements"));
            Assert.That(result.root["existing.owner.capability"].AsBoolean(), Is.True);
            Assert.That(result.root[NativeAccessBuild.Entitlement].AsBoolean(), Is.True);
            project.ReadFromFile(PBXProject.GetPBXProjectPath(directory));
            project.SetBuildProperty(App, "CODE_SIGN_ENTITLEMENTS", "../Escape.entitlements");
            project.WriteToFile(PBXProject.GetPBXProjectPath(directory));
            Assert.Throws<InvalidOperationException>(() => NativeAccessBuild.Configure(directory));
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
