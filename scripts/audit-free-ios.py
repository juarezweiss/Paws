#!/usr/bin/env python3
"""Issue #90: read-only commercial-exclusion audit of a Unity iOS export/archive.

No signing, device operations, networking or modification of inspected artifacts.
Exit 0 means the recorded static binary checks passed, not release/legal approval.
"""
import argparse
import hashlib
import json
import os
import plistlib
import re
import subprocess
import sys
from pathlib import Path

RULES = {
    "unity-iap": rb"Unity[._]Purchasing(?:[._A-Za-z0-9]*)|UnityPurchasing|UnityStoreKitPlugin|UnityEarlyTransactionObserver|StoreKitManager",
    "unity-commercial-services": rb"Unity[._]Services[._]Core(?:[._A-Za-z0-9]*)|UnityServicesProjectConfiguration|UnityServicesInitialization|UnityAdsIdentifier",
    "apple-purchase-provider": rb"PawsApple(?:Evidence|OfferCode)|PawsEvidenceRecord|PawsOfferCodeSheet|ApplePurchaseService|AppleStoreClient|iap-evidence-v1",
    "google-ads-consent": rb"GoogleMobileAds|UserMessagingPlatform|AdMobAdService|GoogleAdsBridge|GADApplicationIdentifier|GADMobileAds|UMPConsent|UMPPrivacy|unity-plugin-library|PawsAdMob",
    "commercial-identifiers": rb"ca-app-pub-[0-9]{10,}(?:[~/][0-9]+)?|com[.]jweiss[.]paws[.]removeads|test[.]paws[.]removeads",
    "commercial-configuration": rb"SKAdNetworkItems|SKAdNetworkIdentifier|NSUserTrackingUsageDescription|IAPProductCatalog|UnityIAP|paws-admob-review|paws-apple-iap-review",
    "monetization-only-age": rb"PawsAgeAssurance|AppleAgeAssurance|AdAgeEligibility|MonetizationAgeGate",
    "monetization-domain": rb"CatsAndArrows[._]Monetization|MonetizationCoordinator|FileMonetizationStateStore|MonetizationState|RewardedHint|IAdPrivacyService|GoogleAdsContracts|IConsentBridge",
    "commercial-ui": rb"MonetizationLocalization|monetization[.](?:remove|restore|status|privacy|hint-ads|redeem)|Remove Ads|Restore Purchases|Ad Privacy|Redeem Offer Code",
    "diagnostics-tests": rb"BoardReviewBootstrap|(?:BoardInput|GameLoop|Completion|HintIdle|ArrowHold|Tutorial|Monetization)ReviewChecks|ReviewAdService|ReviewPurchaseService|GenerationBenchmark|AdMobReviewBootstrap|AppleIapReviewBootstrap|nunit[.]framework|UnityEngine[.]TestRunner|UnityEditor[.]TestRunner|NativeAccessReview|PAWS_ACCESS_AUTORUN",
}
COMPILED = {name: re.compile(pattern, re.I) for name, pattern in RULES.items()}
DOMAIN_RULES = {"monetization-domain", "commercial-ui"}
AGE_FRAMEWORKS = ("DeclaredAgeRange.framework", "PermissionKit.framework", "_PermissionKit_UIKit.framework")
AGE_ENTITLEMENT = "com.apple.developer.declared-age-range"
CHUNK = 1024 * 1024


def sha256(path):
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(CHUNK), b""):
            digest.update(block)
    return digest.hexdigest()


def tree_hash(root):
    digest = hashlib.sha256()
    for item in sorted(root.rglob("*"), key=lambda p: p.relative_to(root).as_posix()):
        if item.is_symlink():
            kind, value = "symlink", os.readlink(item)
            if not item.resolve().is_relative_to(root.resolve()):
                raise ValueError("symlink escapes inspected tree")
        elif item.is_file():
            kind, value = "file", sha256(item)
        elif item.is_dir():
            kind, value = "directory", ""
        else:
            raise ValueError("unsupported tree entry")
        digest.update(json.dumps([item.relative_to(root).as_posix(), kind,
                                  item.lstat().st_mode & 0o777, value], separators=(",", ":")).encode() + b"\n")
    return digest.hexdigest()


def content_hits(data):
    """Find exact ASCII and UTF-16 tokens; keep one representative per rule."""
    hits = {}
    compact = data.replace(b"\0", b"")
    for name, pattern in COMPILED.items():
        match = pattern.search(data)
        if match:
            hits[name] = {"token": match.group().decode("ascii", "replace")[:180], "encoding": "ascii"}
            continue
        for match in pattern.finditer(compact):
            token = match.group().decode("ascii", "replace")
            for encoding in ("utf-16-le", "utf-16-be"):
                if token.encode(encoding) in data:
                    hits[name] = {"token": token[:180], "encoding": encoding}
                    break
            if name in hits:
                break
    return hits


def scan_file(path):
    hits = content_hits(path.name.encode())
    with path.open("rb") as stream:
        tail = b""
        for block in iter(lambda: stream.read(CHUNK), b""):
            hits.update({k: v for k, v in content_hits(tail + block).items() if k not in hits})
            tail = block[-1024:]
    return hits


def run(args):
    try:
        result = subprocess.run(args, capture_output=True, text=True, errors="replace", check=False)
        return {"command": args, "exitCode": result.returncode,
                "stdout": result.stdout, "stderr": result.stderr}
    except OSError as error:
        return {"command": args, "exitCode": -1, "stdout": "", "stderr": str(error)}


def parse_age_loads(text):
    kind = None
    result = []
    for line in text.splitlines():
        if re.fullmatch(r"\s*Load command\s+\d+\s*", line):
            kind = None
        match = re.fullmatch(r"\s*cmd\s+(LC_[A-Z0-9_]+)\s*", line)
        if match:
            kind = match[1]
        match = re.match(r"\s*name\s+(.+?)\s+\(offset\s+\d+\)", line)
        if match and kind != "LC_ID_DYLIB":
            for name in AGE_FRAMEWORKS:
                if re.search(r"(?:^|/)" + re.escape(name) + "/", match[1]):
                    result.append({"framework": name, "command": kind})
    return result


def uuid_pairs(text):
    return sorted((uuid.lower(), arch) for uuid, arch in re.findall(r"UUID:\s*([A-Fa-f0-9-]+)\s*\(([^)]+)\)", text))


def audit(export, archive, source_sha, receipt=None):
    findings, checks, inventory, commands = [], [], [], []

    def check(name, passed, detail):
        checks.append({"name": name, "result": "PASS" if passed else "FAIL", "detail": detail})

    def execute(args, required=True):
        result = run(args)
        recorded = dict(result)
        for key in ("stdout", "stderr"):
            if len(recorded[key]) > 32768:
                full = recorded[key]
                recorded[key + "Sha256"] = hashlib.sha256(full.encode()).hexdigest()
                recorded[key + "Characters"] = len(full)
                recorded[key] = full[:16384] + "\n[truncated; full output hash recorded]\n" + full[-16384:]
        commands.append(recorded)
        if required and result["exitCode"]:
            check("tool:" + " ".join(args[:2]), False, "Required inspection command failed; see commands.")
        return result["stdout"]

    for label, root in (("export", export), ("archive", archive)):
        if not root.is_dir():
            check(label + "-present", False, str(root))
            continue
        check(label + "-present", True, str(root))
        for path in sorted(root.rglob("*")):
            if not path.is_file() or path.is_symlink():
                continue
            relative = path.relative_to(root).as_posix()
            try:
                inventory.append({"root": label, "path": relative, "bytes": path.stat().st_size, "sha256": sha256(path)})
            except OSError as error:
                check("file-readable:" + label + ":" + relative, False, str(error))
                continue
            if label == "archive" and not relative.startswith("Products/Applications/"):
                continue  # dSYMs inventoried/UUID checked, not shipped code/resources.
            if label == "export" and relative.startswith("Il2CppOutputProject/Source/il2cppOutput/"):
                layer = "il2cpp-source"
            elif path.name == "global-metadata.dat":
                layer = "il2cpp-metadata"
            elif label == "archive":
                layer = "shipped-app"
            elif relative.startswith(("Libraries/", "Frameworks/")) or path.suffix in (".swift", ".m", ".mm", ".pbxproj", ".entitlements", ".plist", ".xcprivacy", ".json", ".dll"):
                layer = "native-managed-link-resource-input"
            else:
                continue
            hits = content_hits(relative.encode())
            try:
                hits.update(scan_file(path))
            except OSError as error:
                check("file-scannable:" + label + ":" + relative, False, str(error))
            for rule, hit in hits.items():
                findings.append({"root": label, "path": relative, "layer": layer, "rule": rule, **hit})

    for rule in RULES:
        count = sum(f["rule"] == rule for f in findings)
        check("exclusion:" + rule, count == 0, {"filesWithForbiddenHits": count})
    generated = list((export / "Il2CppOutputProject/Source/il2cppOutput").glob("*.cpp"))
    metadata = list(archive.glob("Products/Applications/*.app/**/global-metadata.dat"))
    check("il2cpp-source-present", bool(generated), {"cppFiles": len(generated)})
    check("shipped-il2cpp-metadata-present", len(metadata) == 1, {"files": [str(p.relative_to(archive)) for p in metadata]})
    config = {}
    try:
        config = json.loads((export / "paws-export.json").read_text())
        check("committed-export-source", config.get("sourceSha") == source_sha, config.get("sourceSha"))
        check("production-config", config.get("productionDefine") == "PAWS_PRODUCTION" and config.get("monetizationEnabled") is False and config.get("nativeAgeAccess") is True and all(config.get(k) is False for k in ("developmentBuild", "scriptDebugging", "profilerEnabled")), config)
    except (OSError, ValueError) as error:
        check("production-config", False, str(error))
    entitlement_files = []
    for path in export.rglob("*.entitlements"):
        try:
            values = plistlib.loads(path.read_bytes())
            entitlement_files.append({"path": path.relative_to(export).as_posix(), "values": values})
            check("no-debug-entitlement:" + path.name, not values.get("get-task-allow"), values)
        except (ValueError, OSError) as error:
            check("entitlement-readable:" + path.name, False, str(error))
    check("native-access-entitlement-input", any(p["values"].get(AGE_ENTITLEMENT) is True for p in entitlement_files), entitlement_files)

    applications = list(archive.glob("Products/Applications/*.app"))
    check("single-app", len(applications) == 1, len(applications))
    native_inventory = []
    manifests = []
    for app in applications:
        try:
            info = plistlib.loads((app / "Info.plist").read_bytes())
            check("production-identity", info.get("CFBundleIdentifier") == "com.jweiss.paws" and info.get("CFBundleDisplayName") == "Paws" and info.get("UIDeviceFamily") == [1] and info.get("MinimumOSVersion") == "16.3" and info.get("UISupportedInterfaceOrientations") == ["UIInterfaceOrientationPortrait"], info)
            check("version-build-consistency", info.get("CFBundleShortVersionString") == config.get("version") and str(info.get("CFBundleVersion")) == str(config.get("buildNumber")), {"version": info.get("CFBundleShortVersionString"), "build": info.get("CFBundleVersion")})
            binaries = [app / info["CFBundleExecutable"]]
        except (OSError, ValueError, KeyError) as error:
            check("production-identity", False, str(error))
            binaries = []
        check("unsigned-app", not (app / "_CodeSignature").exists() and not (app / "embedded.mobileprovision").exists(), "No application signature/profile; signed capability validation remains pending.")
        binaries += [p / p.stem for p in app.rglob("*.framework") if (p / p.stem).is_file()]
        binaries += list(app.rglob("*.dylib"))
        frameworks = [str(p.relative_to(app)) for p in app.rglob("*.framework")]
        check("embedded-framework-allowlist", frameworks == ["Frameworks/UnityFramework.framework"], frameworks)
        all_age_loads = []
        native_symbol_text = ""
        for binary in binaries:
            if not binary.is_file():
                check("binary-present:" + str(binary.relative_to(archive)), False, "Required application/framework executable is missing.")
                continue
            record = {"path": str(binary.relative_to(archive)), "sha256": sha256(binary)}
            record["architecture"] = execute(["lipo", "-archs", str(binary)]).strip()
            check("arm64:" + binary.name, record["architecture"] == "arm64", record["architecture"])
            record["linkedLibraries"] = execute(["otool", "-L", str(binary)])
            loads = execute(["otool", "-l", str(binary)])
            age_loads = parse_age_loads(loads)
            all_age_loads += age_loads
            record["nativeAccessLoadCommands"] = age_loads
            # System StoreKit linkage is not by itself evidence of a provider.
            record["ordinarySystemStoreKitReference"] = "/System/Library/Frameworks/StoreKit.framework/" in record["linkedLibraries"]
            for name, args in (("symbols", ["nm", "-a", str(binary)]), ("undefinedSymbols", ["nm", "-u", str(binary)]), ("strings", ["strings", "-a", str(binary)])):
                output = execute(args)
                if name == "symbols":
                    native_symbol_text += output
                hits = content_hits(output.encode())
                record[name + "Hits"] = hits
                check("binary-" + name + ":" + binary.name, not hits, hits)
            uuids = uuid_pairs(execute(["dwarfdump", "--uuid", str(binary)]))
            dsym = archive / "dSYMs" / (binary.parent.name + ".dSYM" if binary.parent.suffix == ".framework" else app.name + ".dSYM")
            dsym_uuids = uuid_pairs(execute(["dwarfdump", "--uuid", str(dsym)])) if dsym.exists() else []
            record.update({"uuids": uuids, "dsymPath": str(dsym.relative_to(archive)), "dsymUuids": dsym_uuids})
            check("dsym-match:" + binary.name, bool(uuids) and uuids == dsym_uuids, {"binary": uuids, "dsym": dsym_uuids})
            native_inventory.append(record)
        for name in AGE_FRAMEWORKS:
            relevant = [x for x in all_age_loads if x["framework"] == name]
            check("native-access-weak:" + name, bool(relevant) and all(x["command"] == "LC_LOAD_WEAK_DYLIB" for x in relevant), relevant)
        for name in ("PawsAccessRefresh", "PawsAccessPoll", "PawsAccessDispose"):
            check("native-access-entrypoint:" + name, name in native_symbol_text, "Required standalone bridge entry point in actual binary symbols.")
        for path in app.rglob("PrivacyInfo.xcprivacy"):
            try:
                values = plistlib.loads(path.read_bytes())
                manifests.append({"path": str(path.relative_to(archive)), "sha256": sha256(path), "values": values})
                check("privacy-no-tracking:" + str(path.relative_to(app)), values.get("NSPrivacyTracking", False) is False and not values.get("NSPrivacyTrackingDomains") and not values.get("NSPrivacyCollectedDataTypes"), values)
            except (OSError, ValueError) as error:
                check("privacy-readable", False, str(error))
        check("privacy-manifest-present", bool(manifests), "Static declarations only; runtime/native legal coverage belongs to #91/#103/#92.")
    tool_versions = {"python": sys.version, "xcode": execute(["xcodebuild", "-version"]), "sdk": execute(["xcrun", "--sdk", "iphoneos", "--show-sdk-version"]), "clang": execute(["xcrun", "clang", "--version"]), "os": execute(["sw_vers"])}
    binary_hits = [rule for binary in native_inventory for key, values in binary.items()
                   if key.endswith("Hits") for rule in values]
    rule_counts = {rule: sum(f["rule"] == rule for f in findings) + binary_hits.count(rule) for rule in RULES}
    archive_hash = None
    if archive.is_dir():
        try:
            archive_hash = tree_hash(archive)
            check("archive-tree-hash", True, archive_hash)
        except (ValueError, OSError) as error:
            check("archive-tree-hash", False, str(error))
    incomplete_inspection = any(c["result"] == "FAIL" for c in checks
                                if c["name"].startswith(("tool:", "export-present", "archive-present", "il2cpp-source-present", "shipped-il2cpp-metadata-present", "single-app", "file-readable:", "file-scannable:", "archive-tree-hash", "binary-present:", "committed-export-source", "production-config")))
    provider_result = "PASS" if not incomplete_inspection and all(count == 0 for rule, count in rule_counts.items() if rule not in DOMAIN_RULES) else "FAIL"
    all_components_result = "PASS" if provider_result == "PASS" and all(rule_counts[rule] == 0 for rule in DOMAIN_RULES) else "FAIL"
    result = {"schema": "paws-free-ios-exclusion-audit-v1", "sourceSha": source_sha,
              "result": "PASS" if checks and all(c["result"] == "PASS" for c in checks) else "FAIL",
              "scope": "Static unsigned production export/archive inspection; no signing, install, runtime data capture or legal/physical acceptance.",
              "providerSdkExclusionResult": provider_result,
              "allMonetizationComponentsExclusionResult": all_components_result,
              "exclusionRuleOccurrenceCounts": rule_counts,
              "retainedDomainClassification": "Any retained coordinator, state, commercial UI or localization code is a strict ALL-components failure, including inert/null-composed code. These findings are separate from provider/SDK exclusions.",
              "remainingReleaseGates": ["#91 final territory/permission/retention/public content/declarations", "#103 actual regional/native runtime and older-OS coverage", "#92 selected signed candidate and explicit human physical review", "#21 distribution approval"],
              "checks": checks, "findings": findings, "fileInventory": inventory,
              "nativeBinaries": native_inventory, "privacyManifests": manifests,
              "entitlementInputs": entitlement_files, "toolVersions": tool_versions,
              "archiveTreeSha256": archive_hash,
              "checksumAlgorithm": "paws-tree-sha256-v1", "commands": commands}
    if receipt:
        result["productionReceipt"] = {"path": str(receipt), "sha256": sha256(receipt)}
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--export", required=True, type=Path)
    parser.add_argument("--archive", required=True, type=Path)
    parser.add_argument("--source-sha", required=True)
    parser.add_argument("--receipt", type=Path)
    parser.add_argument("--output", required=True, type=Path)
    args = parser.parse_args()
    if not re.fullmatch(r"[a-f0-9]{40}", args.source_sha):
        parser.error("--source-sha must be the exact committed 40-character source SHA")
    output = args.output.resolve()
    if output.exists() or any(output.is_relative_to(p.resolve()) for p in (args.export, args.archive)):
        parser.error("--output must be fresh and outside inspected artifacts")
    result = audit(args.export.resolve(), args.archive.resolve(), args.source_sha, args.receipt)
    with output.open("x") as stream:
        json.dump(result, stream, indent=2, sort_keys=True)
        stream.write("\n")
    print(f"{result['result']}: {len(result['findings'])} forbidden file/rule matches; receipt {output}")
    return 0 if result["result"] == "PASS" else 1


if __name__ == "__main__":
    raise SystemExit(main())
