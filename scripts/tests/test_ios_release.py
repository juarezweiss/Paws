"""Issue #63 release evidence checks against synthetic exports, without Unity/Xcode."""

import copy
import importlib.util
import json
import os
import plistlib
import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch

SCRIPT = Path(__file__).resolve().parents[1] / "ios-release.py"
SPEC = importlib.util.spec_from_file_location("ios_release", SCRIPT)
ios = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(ios)
SHA = "a" * 40


class IOSReleaseTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(prefix="paws release ")
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        settings = self.root / "UnityProject/ProjectSettings"
        settings.mkdir(parents=True)
        (settings / "ProjectSettings.asset").write_text("""PlayerSettings:
  targetDevice: 0
  productName: Paws
  bundleVersion: 0.1.0
  applicationIdentifier:
    iPhone: com.jweiss.paws
  buildNumber:
    iPhone: 1
  iOSTargetOSVersionString: 16.3
""")
        (settings / "ProjectVersion.txt").write_text("m_EditorVersion: 6000.3.25f1\n")
        packages = self.root / "UnityProject/Packages"
        packages.mkdir()
        (packages / "packages-lock.json").write_text('{"dependencies":{"com.unity.test-framework":{"version":"1.6.0"}}}')
        self.export = self.root / "export"
        self.export.mkdir()
        self.metadata = {"sourceSha": SHA, "unityVersion": "6000.3.25f1", "bundleIdentifier": ios.BUNDLE_ID,
                         "version": "0.1.0", "buildNumber": "1", "minimumIOS": "16.3", "deviceFamily": [1],
                         "monetizationEnabled": False, "nativeAgeAccess": True, "productionDefine": "PAWS_PRODUCTION"}
        self.write_metadata()
        self.info = {"CFBundleIdentifier": "${PRODUCT_BUNDLE_IDENTIFIER}", "CFBundleDisplayName": "Paws",
                     "CFBundleShortVersionString": "0.1.0", "CFBundleVersion": "1",
                     "UISupportedInterfaceOrientations": ["UIInterfaceOrientationPortrait"],
                     "UIRequiredDeviceCapabilities": ["arm64", "metal"]}
        self.write_plist(self.export / "Info.plist", self.info)
        self.project = {"objects": {
            "target": {"isa": "PBXNativeTarget", "name": "Unity-iPhone", "buildConfigurationList": "list"},
            "list": {"buildConfigurations": ["release"]},
            "release": {"isa": "XCBuildConfiguration", "name": "Release", "buildSettings": {
                "PRODUCT_BUNDLE_IDENTIFIER": ios.BUNDLE_ID, "TARGETED_DEVICE_FAMILY": "1",
                "IPHONEOS_DEPLOYMENT_TARGET": "16.3",
                "CODE_SIGN_ENTITLEMENTS": "PawsAccess.entitlements",
                "PROVISIONING_PROFILE_SPECIFIER": "$(PROVISIONING_PROFILE_SPECIFIER_APP)"}},
            "framework": {"isa": "PBXNativeTarget", "name": "UnityFramework", "buildConfigurationList": "framework-list",
                          "buildPhases": ["framework-links"]},
            "framework-list": {"buildConfigurations": ["framework-release"]},
            "framework-release": {"isa": "XCBuildConfiguration", "name": "Release", "buildSettings": {
                "PROVISIONING_PROFILE_SPECIFIER": ""}},
            "framework-links": {"isa": "PBXFrameworksBuildPhase", "files": ["age-link", "permission-link"]},
            "age-link": {"isa": "PBXBuildFile", "fileRef": "age-ref", "settings": {"ATTRIBUTES": ["Weak"]}},
            "permission-link": {"isa": "PBXBuildFile", "fileRef": "permission-ref", "settings": {"ATTRIBUTES": ["Weak"]}},
            "age-ref": {"isa": "PBXFileReference", "path": "System/Library/Frameworks/DeclaredAgeRange.framework", "sourceTree": "SDKROOT"},
            "permission-ref": {"isa": "PBXFileReference", "path": "System/Library/Frameworks/PermissionKit.framework", "sourceTree": "SDKROOT"}}}
        self.access_entitlements = self.export / "PawsAccess.entitlements"
        self.write_plist(self.access_entitlements, {ios.AGE_ENTITLEMENT: True})
        self.write_project()
        self.generated = self.export / "Il2CppOutputProject/Source/il2cppOutput/CatsAndArrows.UI.cpp"
        self.generated.parent.mkdir(parents=True)
        self.generated.write_text("void PawsBootstrap_Start() {}")
        self.archive = self.root / "Paws.xcarchive"
        self.app = self.archive / "Products/Applications/Paws.app"
        self.app.mkdir(parents=True)
        app_info = dict(self.info, CFBundleIdentifier=ios.BUNDLE_ID, UIDeviceFamily=[1], MinimumOSVersion="16.3", CFBundleExecutable="Paws")
        self.write_plist(self.app / "Info.plist", app_info)
        self.write_plist(self.archive / "Info.plist", {"ApplicationProperties": {
            "CFBundleIdentifier": ios.BUNDLE_ID, "CFBundleShortVersionString": "0.1.0", "CFBundleVersion": "1"}})
        (self.app / "Paws").write_bytes(b"fixture binary")
        self.entitlements = {}
        self.binary_load_commands = {}
        self.binary_libraries = {}
        self.commands = []
        self.dirty_status = ""
        self.mock = patch.object(ios, "command", side_effect=self.run_command)
        self.mock.start()
        self.addCleanup(self.mock.stop)

    @staticmethod
    def write_plist(path, data):
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(plistlib.dumps(data))

    def write_metadata(self):
        (self.export / "paws-export.json").write_text(json.dumps(self.metadata))

    def write_project(self):
        path = self.export / "Unity-iPhone.xcodeproj/project.pbxproj"
        path.parent.mkdir(exist_ok=True)
        path.write_text(json.dumps(self.project))

    def run_command(self, args):
        self.commands.append(args)
        if args[0] == "git":
            return self.dirty_status if "status" in args else SHA
        if args[0] == "xcodebuild":
            return "Xcode 27.0\nBuild version fixture\n"
        if args[0] == "otool":
            if args[1] == "-l":
                return self.binary_load_commands.get(Path(args[2]).name, self.load_command("UIKit.framework", "LC_LOAD_DYLIB"))
            return self.binary_libraries.get(Path(args[2]).name,
                "Paws:\n\t/System/Library/Frameworks/UIKit.framework/UIKit (compatibility version 1.0.0)\n")
        if args[0] == "codesign" and "-d" in args:
            return plistlib.dumps(self.entitlements).decode()
        if args[0] == "codesign":
            return ""
        self.fail(f"Unexpected command: {args}")

    def receipt(self, archive=False):
        return ios.build_receipt(self.root, self.export, self.archive if archive else None)

    @staticmethod
    def load_command(framework, kind, index=0):
        stem = Path(framework).stem
        return (f"Load command {index}\n          cmd {kind}\n      cmdsize 104\n"
                f"         name /System/Library/Frameworks/{framework}/{stem} (offset 24)\n"
                "   time stamp 2 Thu Jan  1 00:00:02 1970\n      current version 1.0.0\n")

    def test_export_receipt_contains_real_config_packages_and_pending_human_gates(self):
        receipt = self.receipt()
        self.assertEqual(receipt["sourceSha"], SHA)
        self.assertEqual(receipt["stage"], "export-validated")
        self.assertEqual(receipt["configuration"]["deviceFamily"], [1])
        self.assertEqual(receipt["unityPackages"]["com.unity.test-framework"]["version"], "1.6.0")
        self.assertEqual(receipt["diagnosticExclusion"]["sourceFilesInspected"], 1)
        self.assertEqual(receipt["physicalIPhoneReview"], "pending-human")
        self.assertTrue(receipt["nativeAgeAccess"]["exportEntitlementVerified"])
        self.assertEqual(receipt["nativeAgeAccess"]["signedCapabilityVerification"], "pending-signing")
        self.assertEqual({link["framework"] for link in receipt["nativeAgeAccess"]["systemFrameworkLinks"]},
                         set(ios.AGE_FRAMEWORKS))
        self.assertFalse(any(args[0] == "codesign" for args in self.commands))

    def test_metadata_mismatches_fail(self):
        for key, value in {"sourceSha": "b" * 40, "unityVersion": "6000.3.1f1", "bundleIdentifier": "old.bundle",
                           "version": "0.2.0", "buildNumber": "2", "minimumIOS": "15.0", "deviceFamily": [1, 2],
                           "monetizationEnabled": True, "productionDefine": "REVIEW", "developmentBuild": True,
                           "scriptDebugging": True, "profilerEnabled": True, "nativeAgeAccess": False}.items():
            with self.subTest(key=key):
                original = copy.deepcopy(self.metadata)
                self.metadata[key] = value
                self.write_metadata()
                with self.assertRaises(ios.InspectionError):
                    self.receipt()
                self.metadata = original
        self.write_metadata()

    def test_native_access_metadata_requires_boolean_true(self):
        for value in (None, False, 1, "true"):
            with self.subTest(value=value):
                self.metadata["nativeAgeAccess"] = value
                self.write_metadata()
                with self.assertRaisesRegex(ios.InspectionError, "nativeAgeAccess"):
                    self.receipt()

    def test_native_access_entitlement_is_required_and_boolean(self):
        for values in ({}, {ios.AGE_ENTITLEMENT: False}, {ios.AGE_ENTITLEMENT: 1}, {ios.AGE_ENTITLEMENT: "true"}):
            with self.subTest(values=values):
                self.write_plist(self.access_entitlements, values)
                with self.assertRaisesRegex(ios.InspectionError, "entitlement as Boolean true"):
                    self.receipt()
        self.access_entitlements.unlink()
        with self.assertRaisesRegex(ios.InspectionError, "file is missing"):
            self.receipt()

    def test_native_access_entitlement_path_cannot_escape_or_use_expansion(self):
        outside = self.root / "outside.entitlements"
        self.write_plist(outside, {ios.AGE_ENTITLEMENT: True})
        (self.export / "escape.entitlements").symlink_to(outside)
        settings = self.project["objects"]["release"]["buildSettings"]
        for value in (None, "", "../outside.entitlements", str(outside), "$(SRCROOT)/PawsAccess.entitlements",
                      "escape.entitlements", "folder\\PawsAccess.entitlements"):
            with self.subTest(value=value):
                settings["CODE_SIGN_ENTITLEMENTS"] = value
                self.write_project()
                with self.assertRaisesRegex(ios.InspectionError, "entitlement"):
                    self.receipt()

    def test_native_access_entitlement_cannot_be_assigned_to_framework_or_project(self):
        for config in ("framework-release", "project-release"):
            with self.subTest(config=config):
                original = copy.deepcopy(self.project)
                self.project["objects"].setdefault(config, {"isa": "XCBuildConfiguration", "name": "Release", "buildSettings": {}})
                self.project["objects"][config]["buildSettings"]["CODE_SIGN_ENTITLEMENTS"] = "PawsAccess.entitlements"
                self.write_project()
                with self.assertRaisesRegex(ios.InspectionError, "scoped to the application"):
                    self.receipt()
                self.project = original

    def test_native_access_framework_links_are_weak_and_attached_to_unity_framework(self):
        for object_id in ("age-link", "permission-link"):
            for attributes in ([], ["Required"], "NotWeak"):
                with self.subTest(object_id=object_id, attributes=attributes):
                    original = copy.deepcopy(self.project)
                    self.project["objects"][object_id]["settings"]["ATTRIBUTES"] = attributes
                    self.write_project()
                    with self.assertRaisesRegex(ios.InspectionError, "weak linkage"):
                        self.receipt()
                    self.project = original
        self.project["objects"]["framework"]["buildPhases"] = []
        self.write_project()
        with self.assertRaisesRegex(ios.InspectionError, "Missing weak"):
            self.receipt()

    def test_native_access_rejects_additional_strong_link_on_app_target(self):
        self.project["objects"]["target"]["buildPhases"] = ["app-links"]
        self.project["objects"]["app-links"] = {"isa": "PBXFrameworksBuildPhase", "files": ["strong-age-link"]}
        self.project["objects"]["strong-age-link"] = {"isa": "PBXBuildFile", "fileRef": "age-ref"}
        self.write_project()
        with self.assertRaisesRegex(ios.InspectionError, "weak linkage"):
            self.receipt()

    def test_native_access_rejects_non_system_framework_reference(self):
        self.project["objects"]["age-ref"]["sourceTree"] = "SOURCE_ROOT"
        self.write_project()
        with self.assertRaisesRegex(ios.InspectionError, "system SDK"):
            self.receipt()

    def test_modified_xcode_identifier_is_rejected(self):
        self.project["objects"]["release"]["buildSettings"]["PRODUCT_BUNDLE_IDENTIFIER"] = "com.review"
        self.write_project()
        with self.assertRaisesRegex(ios.InspectionError, "Xcode bundle identifier"):
            self.receipt()

    def test_dirty_build_inputs_reject_export_provenance(self):
        for status in (" M UnityProject/Assets/Game.cs", "?? scripts/new-build.py"):
            with self.subTest(status=status):
                self.dirty_status = status
                with self.assertRaisesRegex(ios.InspectionError, "build inputs differ from HEAD"):
                    self.receipt()
        self.assertIn(["git", "-C", str(self.root), "status", "--porcelain", "--untracked-files=all", "--",
                       "UnityProject", "scripts"], self.commands)

    def test_app_requires_unmodified_unity_provisioning_binding(self):
        settings = self.project["objects"]["release"]["buildSettings"]
        for profile in (None, "", "global-profile", "$(inherited)"):
            with self.subTest(profile=profile):
                settings["PROVISIONING_PROFILE_SPECIFIER"] = profile
                self.write_project()
                with self.assertRaisesRegex(ios.InspectionError, "app-specific provisioning binding"):
                    self.receipt()

    def test_framework_and_project_cannot_receive_app_provisioning_profile(self):
        for config_name in ("framework-release", "project-release"):
            with self.subTest(config=config_name):
                original = copy.deepcopy(self.project)
                self.project["objects"][config_name] = {"isa": "XCBuildConfiguration", "name": "Release",
                    "buildSettings": {"PROVISIONING_PROFILE_SPECIFIER": "$(PROVISIONING_PROFILE_SPECIFIER_APP)"}}
                self.write_project()
                with self.assertRaisesRegex(ios.InspectionError, "provisioning profile"):
                    self.receipt()
                self.project = original

    def test_framework_can_inherit_an_empty_project_profile(self):
        self.project["objects"]["framework-release"]["buildSettings"]["PROVISIONING_PROFILE_SPECIFIER"] = "$(inherited)"
        self.project["objects"]["project-release"] = {"isa": "XCBuildConfiguration", "name": "Release", "buildSettings": {}}
        self.write_project()
        self.assertEqual(self.receipt()["stage"], "export-validated")

    def test_release_development_flags_are_rejected(self):
        self.project["objects"]["release"]["buildSettings"]["GCC_PREPROCESSOR_DEFINITIONS"] = ["DEBUG=1"]
        self.write_project()
        with self.assertRaisesRegex(ios.InspectionError, "Development"):
            self.receipt()

    def test_missing_plist_project_metadata_or_il2cpp_fails(self):
        for path in (self.export / "Info.plist", self.export / "paws-export.json", self.generated,
                     self.export / "Unity-iPhone.xcodeproj/project.pbxproj", self.access_entitlements):
            with self.subTest(path=path.name):
                data = path.read_bytes()
                path.unlink()
                with self.assertRaises(ios.InspectionError):
                    self.receipt()
                path.write_bytes(data)

    def test_review_benchmark_and_mock_code_is_rejected(self):
        for name in ("BoardReviewBootstrap", "BoardInputReviewChecks", "GameLoopReviewChecks", "ReviewAdService",
                     "ReviewPurchaseService", "GenerationBenchmark"):
            with self.subTest(name=name):
                self.generated.write_text(f"void {name}_Start();")
                with self.assertRaisesRegex(ios.InspectionError, "production IL2CPP"):
                    self.receipt()

    def test_type_crossing_read_block_is_rejected(self):
        self.generated.write_bytes(b"x" * (1024 * 1024 - 5) + b"BoardReviewBootstrap")
        with self.assertRaises(ios.InspectionError):
            self.receipt()

    def test_export_plist_identity_version_family_and_os_are_verified(self):
        for key, value in {"CFBundleIdentifier": "old.bundle", "CFBundleDisplayName": "Other",
                           "CFBundleShortVersionString": "0.2.0", "CFBundleVersion": "2",
                           "UIDeviceFamily": [1, 2], "MinimumOSVersion": "15.0",
                           "UISupportedInterfaceOrientations": ["UIInterfaceOrientationLandscapeLeft"]}.items():
            with self.subTest(key=key):
                self.write_plist(self.export / "Info.plist", dict(self.info, **{key: value}))
                with self.assertRaises(ios.InspectionError):
                    self.receipt()
        self.write_plist(self.export / "Info.plist", self.info)

    def test_inspection_does_not_mutate_export_or_archive(self):
        before = (ios.tree_sha256(self.export), ios.tree_sha256(self.archive))
        self.receipt(True)
        self.assertEqual(before, (ios.tree_sha256(self.export), ios.tree_sha256(self.archive)))

    def test_export_records_system_framework_references(self):
        self.project["objects"]["UIKit"] = {"isa": "PBXFileReference", "path": "System/Library/Frameworks/UIKit.framework"}
        self.write_project()
        self.assertEqual(self.receipt()["exportLinkedSDKReferences"],
                         sorted(["System/Library/Frameworks/UIKit.framework"] +
                                ["System/Library/Frameworks/" + name for name in ios.AGE_FRAMEWORKS]))

    def test_unsigned_archive_needs_no_signing_credentials(self):
        result = self.receipt(True)
        self.assertEqual(result["stage"], "unsigned-archive-validated")
        archive = result["archive"]
        self.assertFalse(archive["signatureVerified"])
        self.assertEqual(archive["nativeAgeCapabilityVerification"], "pending-signing")
        self.assertEqual(len(archive["archiveTreeSha256"]), 64)
        self.assertEqual(archive["capabilities"], ["arm64", "metal"])
        self.assertIn("UIKit.framework", archive["linkedLibraries"][0]["linkedLibraries"][0])
        self.assertFalse(any(args[0] == "codesign" for args in self.commands))

    def test_archived_native_age_frameworks_have_actual_weak_load_commands(self):
        framework = self.app / "Frameworks/UnityFramework.framework"
        framework.mkdir(parents=True)
        (framework / "UnityFramework").write_bytes(b"framework")
        self.binary_libraries["UnityFramework"] = "UnityFramework:\n" + "".join(
            f"\t/System/Library/Frameworks/{name}/{Path(name).stem} (compatibility version 1.0.0)\n"
            for name in ios.AGE_FRAMEWORKS)
        self.binary_load_commands["UnityFramework"] = "UnityFramework:\n" + "".join(
            self.load_command(name, "LC_LOAD_WEAK_DYLIB", index) for index, name in enumerate(ios.AGE_FRAMEWORKS))
        result = self.receipt(True)
        links = next(item for item in result["archive"]["linkedLibraries"] if item["binary"].endswith("/UnityFramework"))
        self.assertEqual({item["framework"] for item in links["nativeAgeSystemLoadCommands"]}, set(ios.AGE_FRAMEWORKS))
        self.assertTrue(all(item["command"] == "LC_LOAD_WEAK_DYLIB" for item in links["nativeAgeSystemLoadCommands"]))
        self.assertIn(["otool", "-l", str(framework / "UnityFramework")], self.commands)

    def test_archived_native_age_strong_autolinks_are_rejected_despite_weak_project(self):
        # A valid PBX Weak link must not hide an extra strong Swift-autolink result.
        for name in ios.AGE_FRAMEWORKS:
            with self.subTest(framework=name):
                self.binary_load_commands["Paws"] = (
                    self.load_command(name, "LC_LOAD_WEAK_DYLIB") + self.load_command(name, "LC_LOAD_DYLIB", 1))
                with self.assertRaisesRegex(ios.InspectionError, "require LC_LOAD_WEAK_DYLIB"):
                    self.receipt(True)

    def test_native_age_load_command_inspection_cannot_silently_omit_a_listed_link(self):
        self.binary_libraries["Paws"] = "Paws:\n\t/System/Library/Frameworks/DeclaredAgeRange.framework/DeclaredAgeRange\n"
        with self.assertRaisesRegex(ios.InspectionError, "Cannot verify native age framework Mach-O"):
            self.receipt(True)

    def test_native_age_load_command_does_not_inherit_kind_from_previous_command(self):
        self.binary_load_commands["Paws"] = self.load_command("UIKit.framework", "LC_LOAD_WEAK_DYLIB") + (
            "Load command 1\n         name /System/Library/Frameworks/PermissionKit.framework/PermissionKit (offset 24)\n")
        with self.assertRaisesRegex(ios.InspectionError, "require LC_LOAD_WEAK_DYLIB"):
            self.receipt(True)

    def test_signed_archive_verifies_signature_and_reads_entitlements(self):
        signature = self.app / "_CodeSignature/CodeResources"
        signature.parent.mkdir()
        signature.write_bytes(b"fixture")
        self.entitlements = {"get-task-allow": False, "aps-environment": "production", ios.AGE_ENTITLEMENT: True}
        result = self.receipt(True)
        self.assertEqual(result["stage"], "signed-archive-validated")
        self.assertTrue(result["archive"]["signatureVerified"])
        self.assertEqual(result["archive"]["entitlements"]["aps-environment"], "production")
        self.assertEqual(result["archive"]["nativeAgeCapabilityVerification"], "signed-entitlement-verified")
        self.assertTrue(any(args[:4] == ["codesign", "--verify", "--deep", "--strict"] for args in self.commands))
        self.entitlements["get-task-allow"] = True
        with self.assertRaisesRegex(ios.InspectionError, "development debugging"):
            self.receipt(True)

    def test_signed_native_age_entitlement_is_required_and_boolean(self):
        signature = self.app / "_CodeSignature/CodeResources"
        signature.parent.mkdir()
        signature.write_bytes(b"fixture")
        for values in ({}, {ios.AGE_ENTITLEMENT: False}, {ios.AGE_ENTITLEMENT: "true"}, {ios.AGE_ENTITLEMENT: 1}):
            with self.subTest(values=values):
                self.entitlements = values
                with self.assertRaisesRegex(ios.InspectionError, "missing its native age entitlement"):
                    self.receipt(True)

    def test_archive_requires_app_archive_metadata_and_executable(self):
        for path in (self.app / "Info.plist", self.archive / "Info.plist", self.app / "Paws"):
            with self.subTest(path=path):
                data = path.read_bytes()
                path.unlink()
                with self.assertRaises(ios.InspectionError):
                    self.receipt(True)
                path.write_bytes(data)

    def test_archive_mismatch_rejected(self):
        info = ios.read_plist(self.app / "Info.plist")
        info["UIDeviceFamily"] = [1, 2]
        self.write_plist(self.app / "Info.plist", info)
        with self.assertRaisesRegex(ios.InspectionError, "device family"):
            self.receipt(True)

    def test_inventory_records_framework_library_privacy_and_entitlements(self):
        framework = self.app / "Frameworks/UnityFramework.framework"
        framework.mkdir(parents=True)
        (framework / "UnityFramework").write_bytes(b"framework")
        (self.app / "libExample.dylib").write_bytes(b"library")
        self.write_plist(self.app / "PrivacyInfo.xcprivacy", {"NSPrivacyTracking": False})
        self.write_plist(self.export / "Paws.entitlements", {"get-task-allow": False})
        receipt = self.receipt(True)
        inventory = receipt["archive"]["inventory"]
        self.assertEqual(len(inventory["frameworks"]), 1)
        self.assertEqual(len(inventory["nativeLibraries"]), 1)
        self.assertFalse(inventory["privacyManifests"][0]["declarations"]["NSPrivacyTracking"])
        self.assertFalse(receipt["exportInventory"]["entitlementFiles"][0]["values"]["get-task-allow"])

    def test_checksum_ignores_times_but_tracks_paths_content_permissions_and_links(self):
        original = ios.tree_sha256(self.archive)
        os.utime(self.app / "Paws", (1, 1))
        self.assertEqual(original, ios.tree_sha256(self.archive))
        (self.app / "Paws").write_bytes(b"changed")
        changed = ios.tree_sha256(self.archive)
        self.assertNotEqual(original, changed)
        (self.app / "Paws").chmod(0o700)
        self.assertNotEqual(changed, ios.tree_sha256(self.archive))
        (self.app / "safe-link").symlink_to("Paws")
        self.assertEqual(len(ios.tree_sha256(self.archive)), 64)
        (self.app / "escape").symlink_to(self.root / "export")
        with self.assertRaisesRegex(ios.InspectionError, "escapes"):
            ios.tree_sha256(self.archive)

    def test_cli_preserves_existing_output_and_rejects_receipt_inside_export(self):
        output = self.root / "receipt.json"
        output.write_text("keep")
        for path in (output, self.export / "receipt.json"):
            with self.subTest(path=path), patch.object(ios.sys, "argv", [str(SCRIPT), "--export", str(self.export), "--output", str(path)]):
                self.assertEqual(ios.main(), 1)
        self.assertEqual(output.read_text(), "keep")
        self.assertFalse((self.export / "receipt.json").exists())


if __name__ == "__main__":
    unittest.main()
