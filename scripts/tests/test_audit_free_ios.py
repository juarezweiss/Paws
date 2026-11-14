import importlib.util
import json
import os
import plistlib
import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch

SPEC = importlib.util.spec_from_file_location("audit_free_ios", Path(__file__).resolve().parents[1] / "audit-free-ios.py")
audit = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(audit)
SHA = "a" * 40
UUID = "ABCD1234-1234-1234-1234-123456789ABC"
AGE_LOADS = "\n".join(f"Load command {i}\n cmd LC_LOAD_WEAK_DYLIB\n name /System/Library/Frameworks/{name}/{name[:-10]} (offset 24)" for i, name in enumerate(audit.AGE_FRAMEWORKS))


class ContentTests(unittest.TestCase):
    def test_prefilters_preserve_every_regex_alternative_and_utf16_encoding(self):
        # Enumerate variable separator branches too; a missing necessary prefix
        # must fail this positive-control suite instead of hiding an exclusion.
        alternatives = {
            "unity-iap": ["Unity.Purchasing.Stores", "Unity_Purchasing", "UnityPurchasing", "UnityStoreKitPlugin", "UnityEarlyTransactionObserver", "StoreKitManager"],
            "unity-commercial-services": ["Unity.Services.Core", "Unity_Services_Core", "Unity.Services_Core", "Unity_Services.Core", "UnityServicesProjectConfiguration", "UnityServicesInitialization", "UnityAdsIdentifier"],
            "apple-purchase-provider": ["PawsAppleEvidence", "PawsAppleOfferCode", "PawsEvidenceRecord", "PawsOfferCodeSheet", "ApplePurchaseService", "AppleStoreClient", "iap-evidence-v1"],
            "google-ads-consent": ["GoogleMobileAds", "UserMessagingPlatform", "AdMobAdService", "GoogleAdsBridge", "GADApplicationIdentifier", "GADMobileAds", "UMPConsent", "UMPPrivacy", "unity-plugin-library", "PawsAdMob"],
            "commercial-identifiers": ["ca-app-pub-3940256099942544", "com.jweiss.paws.removeads", "test.paws.removeads"],
            "commercial-configuration": ["SKAdNetworkItems", "SKAdNetworkIdentifier", "NSUserTrackingUsageDescription", "IAPProductCatalog", "UnityIAP", "paws-admob-review", "paws-apple-iap-review"],
            "monetization-only-age": ["PawsAgeAssurance", "AppleAgeAssurance", "AdAgeEligibility", "MonetizationAgeGate"],
            "monetization-domain": ["CatsAndArrows.Monetization", "CatsAndArrows_Monetization", "MonetizationCoordinator", "FileMonetizationStateStore", "MonetizationState", "RewardedHint", "IAdPrivacyService", "GoogleAdsContracts", "IConsentBridge"],
            "commercial-ui": ["MonetizationLocalization", "monetization.remove", "monetization.restore", "monetization.status", "monetization.privacy", "monetization.hint-ads", "monetization.redeem", "Remove Ads", "Restore Purchases", "Ad Privacy", "Redeem Offer Code"],
            "mock-review-resources": ["Mock purchases only", "Compras apenas simuladas", "Solo compras simuladas"],
            "diagnostics-tests": ["BoardReviewBootstrap", *[v + "ReviewChecks" for v in ("BoardInput", "GameLoop", "Completion", "HintIdle", "ArrowHold", "Tutorial", "Monetization")], "ReviewAdService", "ReviewPurchaseService", "GenerationBenchmark", "AdMobReviewBootstrap", "AppleIapReviewBootstrap", "nunit.framework", "UnityEngine.TestRunner", "UnityEditor.TestRunner", "NativeAccessReview", "PAWS_ACCESS_AUTORUN"],
        }
        self.assertEqual(set(audit.RULES), set(audit.RULE_PREFIXES))
        self.assertEqual(set(audit.RULES), set(alternatives))
        for rule, tokens in alternatives.items():
            for token in tokens:
                for value in (token, token.upper(), token.lower()):
                    for encoding in ("ascii", "utf-16-le", "utf-16-be"):
                        with self.subTest(rule=rule, token=value, encoding=encoding):
                            self.assertIn(rule, audit.content_hits(b"x" + value.encode(encoding) + b"\xff"))

    def test_each_exclusion_has_a_positive_control(self):
        examples = {
            "unity-iap": b"Unity.Purchasing.Stores",
            "unity-commercial-services": b"Unity.Services.Core.Telemetry",
            "apple-purchase-provider": b"PawsAppleOfferCode",
            "google-ads-consent": b"UMPConsentInformation",
            "commercial-identifiers": b"ca-app-pub-3940256099942544/4411468910",
            "commercial-configuration": b"SKAdNetworkItems",
            "monetization-only-age": b"PawsAgeAssurance",
            "monetization-domain": b"MonetizationCoordinator",
            "commercial-ui": b"MonetizationLocalization",
            "diagnostics-tests": b"GenerationBenchmark",
            "mock-review-resources": b"Mock purchases only",
        }
        self.assertEqual(set(examples), set(audit.RULES))
        for rule, text in examples.items():
            with self.subTest(rule=rule):
                self.assertIn(rule, audit.content_hits(text))

    def test_utf16_both_orders_and_unaligned_buffers(self):
        for encoding in ("utf-16-le", "utf-16-be"):
            for token in ("com.jweiss.paws.removeads", "GoogleMobileAds", "MonetizationCoordinator"):
                hits = audit.content_hits(b"x" + token.encode(encoding) + b"\xff")
                self.assertTrue(hits)
                self.assertEqual(next(iter(hits.values()))["encoding"], encoding)

    def test_ordinary_system_links_and_native_access_do_not_fail(self):
        text = b"/System/Library/Frameworks/StoreKit.framework/StoreKit DeclaredAgeRange PermissionKit _PermissionKit_UIKit PawsNativeAccess PawsAccessWorkflow CatsAndArrows.Access parental consent"
        self.assertEqual({}, audit.content_hits(text))

    def test_interspersed_non_utf16_zeroes_do_not_fabricate_hit(self):
        self.assertEqual({}, audit.content_hits(b"Google\0Mobile\0Ads"))

    def test_stream_boundary_does_not_hide_utf16_resource(self):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / "packed.resource"
            path.write_bytes(b"x" * (audit.CHUNK - 13) + "GoogleMobileAds".encode("utf-16-le"))
            self.assertIn("google-ads-consent", audit.scan_file(path))

    def test_all_three_native_loads_must_be_weak(self):
        loads = audit.parse_age_loads(AGE_LOADS)
        self.assertEqual(set(audit.AGE_FRAMEWORKS), {v["framework"] for v in loads})
        self.assertTrue(all(v["command"] == "LC_LOAD_WEAK_DYLIB" for v in loads))
        strong = audit.parse_age_loads(AGE_LOADS.replace("LC_LOAD_WEAK_DYLIB", "LC_LOAD_DYLIB", 1))
        self.assertEqual("LC_LOAD_DYLIB", strong[0]["command"])

    def test_uuid_parser_checks_architecture_and_uuid(self):
        self.assertEqual([(UUID.lower(), "arm64")], audit.uuid_pairs(f"UUID: {UUID} (arm64) file"))
        self.assertNotEqual(audit.uuid_pairs(f"UUID: {UUID} (arm64) file"), audit.uuid_pairs(f"UUID: {UUID} (x86_64) file"))


class IntegrationTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        root = Path(self.temp.name)
        self.export = root / "export"
        self.archive = root / "Paws.xcarchive"
        self.app = self.archive / "Products/Applications/Paws.app"
        generated = self.export / "Il2CppOutputProject/Source/il2cppOutput"
        generated.mkdir(parents=True)
        (generated / "CatsAndArrows.Access.cpp").write_text("Paws native access")
        (self.export / "paws-export.json").write_text(json.dumps({
            "sourceSha": SHA, "productionDefine": "PAWS_PRODUCTION", "monetizationEnabled": False,
            "nativeAgeAccess": True, "developmentBuild": False, "scriptDebugging": False,
            "profilerEnabled": False, "version": "0.1.0", "buildNumber": "1"}))
        (self.export / "PawsAccess.entitlements").write_bytes(plistlib.dumps({audit.AGE_ENTITLEMENT: True}))
        framework = self.app / "Frameworks/UnityFramework.framework"
        framework.mkdir(parents=True)
        (framework / "UnityFramework").write_bytes(b"fixture-binary")
        (self.app / "Paws").write_bytes(b"fixture-binary")
        metadata = self.app / "Data/Managed/Metadata"
        metadata.mkdir(parents=True)
        (metadata / "global-metadata.dat").write_bytes(b"fixture-metadata")
        (self.app / "Data/boot.config").write_text("wait-for-native-debugger=0\n")
        self.receipt = root / "production-receipt.json"
        (self.app / "Info.plist").write_bytes(plistlib.dumps({
            "CFBundleIdentifier": "com.jweiss.paws", "CFBundleDisplayName": "Paws", "CFBundleExecutable": "Paws",
            "CFBundleShortVersionString": "0.1.0", "CFBundleVersion": "1", "UIDeviceFamily": [1],
            "MinimumOSVersion": "16.3", "UISupportedInterfaceOrientations": ["UIInterfaceOrientationPortrait"]}))
        (framework / "PrivacyInfo.xcprivacy").write_bytes(plistlib.dumps({"NSPrivacyTracking": False}))
        (self.archive / "dSYMs/Paws.app.dSYM").mkdir(parents=True)
        (self.archive / "dSYMs/UnityFramework.framework.dSYM").mkdir(parents=True)

    def command(self, args):
        stdout = {
            "lipo": "arm64", "xcodebuild": "Xcode fixture", "xcrun": "sdk fixture", "sw_vers": "macOS fixture",
            "dwarfdump": f"UUID: {UUID} (arm64) fixture",
            "nm": "_PawsAccessRefresh _PawsAccessPoll _PawsAccessDispose",
            "strings": "Paws standalone access",
        }.get(args[0], "")
        if args[0] == "otool":
            stdout = AGE_LOADS if args[1] == "-l" else "/System/Library/Frameworks/StoreKit.framework/StoreKit"
        return {"command": args, "exitCode": 0, "stdout": stdout, "stderr": ""}

    def run_audit(self):
        self.write_receipt()
        with patch.object(audit, "run", side_effect=self.command):
            return audit.audit(self.export, self.archive, SHA, self.receipt)

    def write_receipt(self, source=SHA, archive_hash=None):
        try:
            archive_hash = archive_hash or audit.tree_hash(self.archive)
        except ValueError:
            archive_hash = "cannot-hash-malformed-archive"
        self.receipt.write_text(json.dumps({"sourceSha": source, "archive": {"archiveTreeSha256": archive_hash}}))

    def test_clean_fixture_and_system_storekit_linkage_pass(self):
        result = self.run_audit()
        self.assertEqual("PASS", result["result"], [c for c in result["checks"] if c["result"] == "FAIL"])
        self.assertTrue(result["nativeBinaries"][0]["ordinarySystemStoreKitReference"])
        self.assertEqual([], result["findings"])
        self.assertTrue(result["fileInventory"])

    def test_records_all_failed_layers_without_fail_fast(self):
        (self.export / "Il2CppOutputProject/Source/il2cppOutput/commerce.cpp").write_text("Unity.Purchasing MonetizationCoordinator")
        (self.app / "Data/Managed/Metadata/global-metadata.dat").write_bytes(b"GoogleMobileAds")
        (self.app / "consent.resources").write_bytes("ca-app-pub-3940256099942544/4411468910".encode("utf-16-be"))
        plugin = self.export / "Libraries/Plugins/Purchasing/Apple/PawsAppleOfferCode.swift"
        plugin.parent.mkdir(parents=True)
        plugin.write_text("native provider")
        result = self.run_audit()
        self.assertEqual("FAIL", result["result"])
        self.assertEqual({"il2cpp-source", "il2cpp-metadata", "shipped-app", "native-managed-link-resource-input"}, {f["layer"] for f in result["findings"]})
        self.assertEqual(5, len(result["findings"]))

    def test_stale_source_and_missing_entitlement_are_failures(self):
        (self.export / "PawsAccess.entitlements").unlink()
        result = None
        with patch.object(audit, "run", side_effect=self.command):
            result = audit.audit(self.export, self.archive, "b" * 40)
        failures = {c["name"] for c in result["checks"] if c["result"] == "FAIL"}
        self.assertIn("committed-export-source", failures)
        self.assertIn("native-access-entitlement-input", failures)

    def test_retained_inert_domain_and_commercial_ui_fail_all_contract_separately(self):
        (self.app / "Data/Managed/Metadata/global-metadata.dat").write_bytes(b"MonetizationCoordinator MonetizationLocalization Remove Ads")
        result = self.run_audit()
        self.assertEqual("FAIL", result["result"])
        self.assertEqual("PASS", result["providerSdkExclusionResult"])
        self.assertEqual("FAIL", result["allMonetizationComponentsExclusionResult"])
        self.assertGreater(result["exclusionRuleOccurrenceCounts"]["monetization-domain"], 0)
        self.assertGreater(result["exclusionRuleOccurrenceCounts"]["commercial-ui"], 0)

    def test_failed_inspection_tool_cannot_pass(self):
        def fail_nm(args):
            result = self.command(args)
            if args[0] == "nm":
                result["exitCode"] = 1
            return result
        with patch.object(audit, "run", side_effect=fail_nm):
            result = audit.audit(self.export, self.archive, SHA)
        self.assertEqual("FAIL", result["result"])
        self.assertTrue(any(c["name"].startswith("tool:nm") and c["result"] == "FAIL" for c in result["checks"]))

    def test_strong_native_overlay_and_missing_dsym_fail(self):
        def strong_overlay(args):
            result = self.command(args)
            if args[:2] == ["otool", "-l"]:
                result["stdout"] = AGE_LOADS.replace("cmd LC_LOAD_WEAK_DYLIB\n name /System/Library/Frameworks/_PermissionKit", "cmd LC_LOAD_DYLIB\n name /System/Library/Frameworks/_PermissionKit")
            return result
        (self.archive / "dSYMs/Paws.app.dSYM").rmdir()
        with patch.object(audit, "run", side_effect=strong_overlay):
            result = audit.audit(self.export, self.archive, SHA)
        failures = {c["name"] for c in result["checks"] if c["result"] == "FAIL"}
        self.assertIn("native-access-weak:_PermissionKit_UIKit.framework", failures)
        self.assertIn("dsym-match:Paws", failures)

    def test_hash_excludes_timestamps_but_detects_resource_mutation(self):
        before = audit.tree_hash(self.archive)
        resource = self.app / "Paws"
        os.utime(resource, (1000, 1000))
        self.assertEqual(before, audit.tree_hash(self.archive))
        resource.write_bytes(b"changed")
        self.assertNotEqual(before, audit.tree_hash(self.archive))

    def test_missing_application_binary_is_failure_without_aborting_report(self):
        (self.app / "Paws").unlink()
        result = self.run_audit()
        self.assertEqual("FAIL", result["providerSdkExclusionResult"])
        self.assertTrue(any(c["name"].startswith("binary-present:") and c["result"] == "FAIL" for c in result["checks"]))
        self.assertTrue(result["nativeBinaries"])

    def test_external_archive_symlink_cannot_claim_complete_exclusion(self):
        target = Path(self.temp.name) / "external.resource"
        target.write_bytes(b"uninspected-external-content")
        (self.app / "external.resource").symlink_to(target)
        result = self.run_audit()
        self.assertEqual("FAIL", result["result"])
        self.assertEqual("FAIL", result["providerSdkExclusionResult"])
        self.assertIsNone(result["archiveTreeSha256"])

    def test_unknown_framework_and_native_library_fail_provider_gate(self):
        for relative in ("Frameworks/Unknown.framework", "Frameworks/libUnknown.dylib"):
            with self.subTest(relative=relative):
                path = self.app / relative
                if path.suffix == ".framework":
                    path.mkdir()
                else:
                    path.write_bytes(b"unknown-native-library")
                result = self.run_audit()
                self.assertEqual("FAIL", result["result"])
                self.assertEqual("FAIL", result["providerSdkExclusionResult"])
                self.assertTrue(any(c["name"].startswith("embedded-") and c["result"] == "FAIL" for c in result["checks"]))
                path.rmdir() if path.is_dir() else path.unlink()

    def test_tracking_declaration_and_missing_manifest_fail_provider_gate(self):
        manifest = self.app / "Frameworks/UnityFramework.framework/PrivacyInfo.xcprivacy"
        manifest.write_bytes(plistlib.dumps({"NSPrivacyTracking": True}))
        result = self.run_audit()
        self.assertEqual("FAIL", result["providerSdkExclusionResult"])
        manifest.unlink()
        result = self.run_audit()
        self.assertEqual("FAIL", result["providerSdkExclusionResult"])

    def test_production_receipt_requires_matching_source_and_actual_archive_hash(self):
        for source, archive_hash in (("b" * 40, None), (SHA, "0" * 64)):
            with self.subTest(source=source, archive_hash=archive_hash):
                self.write_receipt(source, archive_hash)
                with patch.object(audit, "run", side_effect=self.command):
                    result = audit.audit(self.export, self.archive, SHA, self.receipt)
                self.assertEqual("FAIL", result["providerSdkExclusionResult"])
                self.assertFalse(result["productionReceipt"]["bindingVerified"])
                self.assertTrue(any(c["name"].startswith("production-receipt-") and c["result"] == "FAIL" for c in result["checks"]))

    def test_missing_or_malformed_receipt_aggregates_without_aborting(self):
        for value in (None, b"not-json", b"[]"):
            with self.subTest(value=value):
                if value is not None:
                    self.receipt.write_bytes(value)
                with patch.object(audit, "run", side_effect=self.command):
                    result = audit.audit(self.export, self.archive, SHA, self.receipt if value else None)
                self.assertEqual("FAIL", result["providerSdkExclusionResult"])
                self.assertTrue(result["nativeBinaries"])

    def test_missing_nonzero_or_duplicate_debug_boot_config_fails(self):
        boot = self.app / "Data/boot.config"
        for contents in (None, "wait-for-native-debugger=1\n", "wait-for-native-debugger=0\nconnect-profiler=1\n", "wait-for-native-debugger=0\nplayer-connection-debug=2\n", "wait-for-native-debugger=0\nwait-for-native-debugger=1\n"):
            with self.subTest(contents=contents):
                if contents is None:
                    boot.unlink()
                else:
                    boot.write_text(contents)
                result = self.run_audit()
                self.assertEqual("FAIL", result["providerSdkExclusionResult"])
                self.assertTrue(any(c["name"] in ("production-boot-config-present", "no-debug-boot-config") and c["result"] == "FAIL" for c in result["checks"]))


if __name__ == "__main__":
    unittest.main()
