#!/usr/bin/env python3
"""Issue #63: inspect immutable Unity iOS exports/archives; never modify them."""

import argparse
import hashlib
import json
import os
import plistlib
import re
import subprocess
import sys
from pathlib import Path

BUNDLE_ID = "com.jweiss.paws"
AGE_ENTITLEMENT = "com.apple.developer.declared-age-range"
AGE_FRAMEWORKS = ("DeclaredAgeRange.framework", "PermissionKit.framework")
EXCLUDED_TYPES = re.compile(rb"BoardReviewBootstrap|(?:BoardInput|GameLoop|Completion|HintIdle|ArrowHold|Tutorial|Monetization)ReviewChecks|ReviewAdService|ReviewPurchaseService|GenerationBenchmark")


class InspectionError(ValueError):
    pass


def command(args):
    result = subprocess.run(args, capture_output=True, text=True, check=False)
    if result.returncode:
        # Tool stderr may contain signing identities or local account details.
        raise InspectionError(f"{Path(args[0]).name} inspection failed (exit {result.returncode}).")
    return result.stdout


def read_json(path):
    try:
        return json.loads(path.read_text(encoding="utf-8-sig"))
    except (OSError, ValueError) as error:
        raise InspectionError(f"Missing or invalid JSON: {path.name}") from error


def read_plist(path):
    try:
        with path.open("rb") as stream:
            result = plistlib.load(stream)
        if not isinstance(result, dict):
            raise ValueError("dictionary required")
        return result
    except (OSError, ValueError, plistlib.InvalidFileException) as error:
        raise InspectionError(f"Missing or invalid plist: {path.name}") from error


def file_sha256(path):
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def tree_sha256(path):
    """Hash sorted path/type/mode/content records; ignore modification times.

    Symlinks are hashed as links and must resolve inside the inspected tree.
    No tarball or other disposable archive is created.
    """
    root = path.resolve()
    digest = hashlib.sha256()
    for item in sorted(root.rglob("*"), key=lambda item: item.relative_to(root).as_posix()):
        relative = item.relative_to(root).as_posix()
        if item.is_symlink():
            if not item.resolve().is_relative_to(root):
                raise InspectionError(f"Archive symlink escapes its root: {relative}")
            kind, value = "symlink", os.readlink(item)
        elif item.is_file():
            kind, value = "file", file_sha256(item)
        elif item.is_dir():
            kind, value = "directory", ""
        else:
            raise InspectionError(f"Unsupported archive entry: {relative}")
        mode = item.lstat().st_mode & 0o777
        digest.update(json.dumps([relative, kind, mode, value], separators=(",", ":")).encode() + b"\n")
    return digest.hexdigest()


def unity_config(root):
    source = (root / "UnityProject/ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8-sig")

    def field(name, section=None):
        text = source
        if section:
            match = re.search(rf"^  {section}:\s*\n((?:    .*\n)+)", source, re.M)
            if not match:
                raise InspectionError(f"Missing Unity section: {section}")
            text = match.group(1)
        match = re.search(rf"^\s+{name}:\s*([^\n]+)", text, re.M)
        if not match:
            raise InspectionError(f"Missing Unity setting: {name}")
        return match.group(1).strip().strip('\"\'')

    family = {"0": [1], "1": [2], "2": [1, 2]}.get(field("targetDevice"))
    if family is None:
        raise InspectionError("Unsupported Unity iOS targetDevice setting.")
    unity_version = re.search(r"^m_EditorVersion:\s*(\S+)",
                              (root / "UnityProject/ProjectSettings/ProjectVersion.txt").read_text(), re.M)
    if not unity_version:
        raise InspectionError("Missing pinned Unity version.")
    return {"bundleIdentifier": field("iPhone", "applicationIdentifier"),
            "version": field("bundleVersion"), "buildNumber": field("iPhone", "buildNumber"),
            "minimumIOS": field("iOSTargetOSVersionString"), "deviceFamily": family,
            "displayName": field("productName"), "unityVersion": unity_version.group(1)}


def expect(actual, expected, label):
    if actual != expected:
        raise InspectionError(f"Configuration mismatch: {label}.")


def ios_version(value):
    if not isinstance(value, str) or not re.fullmatch(r"\d+(?:\.\d+){0,2}", value):
        raise InspectionError("Invalid minimum iOS version.")
    return tuple((list(map(int, value.split("."))) + [0, 0])[:3])


def project_settings(export):
    project = export / "Unity-iPhone.xcodeproj/project.pbxproj"
    if not project.is_file():
        raise InspectionError("Missing Unity-iPhone.xcodeproj/project.pbxproj.")
    try:
        data = json.loads(project.read_text())
    except ValueError:
        data = json.loads(command(["plutil", "-convert", "json", "-o", "-", str(project)]))
    objects = data.get("objects", {})
    def target_release(name):
        targets = [item for item in objects.values()
                   if item.get("isa") == "PBXNativeTarget" and item.get("name") == name]
        if len(targets) != 1:
            raise InspectionError(f"Expected exactly one {name} target.")
        config_list = objects.get(targets[0].get("buildConfigurationList"), {})
        configs = [objects.get(key, {}) for key in config_list.get("buildConfigurations", [])]
        release = [item.get("buildSettings", {}) for item in configs if item.get("name") == "Release"]
        if len(release) != 1:
            raise InspectionError(f"Expected exactly one {name} Release configuration.")
        return release[0]

    app_release = target_release("Unity-iPhone")
    framework_release = target_release("UnityFramework")
    expect(app_release.get("PROVISIONING_PROFILE_SPECIFIER"), "$(PROVISIONING_PROFILE_SPECIFIER_APP)",
           "Unity app-specific provisioning binding")
    if framework_release.get("PROVISIONING_PROFILE_SPECIFIER") not in (None, "", "$(inherited)"):
        raise InspectionError("UnityFramework must not receive an app provisioning profile.")
    # Project and framework Release flags can enable development code too.
    for item in objects.values():
        if item.get("isa") == "XCBuildConfiguration" and item.get("name") == "Release":
            settings = item.get("buildSettings", {})
            if settings is not app_release and settings.get("PROVISIONING_PROFILE_SPECIFIER") not in (None, "", "$(inherited)"):
                raise InspectionError("App provisioning profile must be scoped to Unity-iPhone only.")
            flags = json.dumps(settings)
            if re.search(r"(?:DEBUG|DEVELOPMENT_BUILD|ENABLE_PROFILER|IL2CPP_MONO_DEBUGGER)\s*=\s*1", flags):
                raise InspectionError("Development/debug/profiler flag in Release configuration.")
            if settings.get("ENABLE_TESTABILITY") == "YES":
                raise InspectionError("Release configuration enables testability.")
    return app_release, data


def resolve_setting(value, settings):
    if isinstance(value, str):
        match = re.fullmatch(r"\$[({]([A-Z_]+)[)}]", value)
        if match:
            return settings.get(match.group(1), value)
    return value


def inspect_native_access(export, project, app_settings):
    """#103: inspect app-scoped capability and weak system links, without signing.

    Entitlement paths are generated project-relative inputs, never expanded shell
    or Xcode expressions. Resolve symlinks too before reading their contents.
    """
    def entitlement_file(value):
        if not isinstance(value, str) or not value.strip() or "$" in value or "\\" in value:
            raise InspectionError("Native age entitlements require a literal project-relative path.")
        path = Path(value)
        if path.is_absolute() or ".." in path.parts:
            raise InspectionError("Native age entitlement path escapes the export.")
        resolved = (export / path).resolve()
        if not resolved.is_relative_to(export.resolve()) or not resolved.is_file():
            raise InspectionError("Native age entitlement file is missing or escapes the export.")
        return resolved, read_plist(resolved)

    path, values = entitlement_file(app_settings.get("CODE_SIGN_ENTITLEMENTS"))
    if values.get(AGE_ENTITLEMENT) is not True:
        raise InspectionError("Application must declare the native age entitlement as Boolean true.")
    objects = project.get("objects", {})
    for item in objects.values():
        if item.get("isa") != "XCBuildConfiguration" or item.get("name") != "Release":
            continue
        settings = item.get("buildSettings", {})
        if settings is app_settings:
            continue
        entitlement = settings.get("CODE_SIGN_ENTITLEMENTS")
        if entitlement not in (None, "", "$(inherited)"):
            _, other_values = entitlement_file(entitlement)
            if AGE_ENTITLEMENT in other_values:
                raise InspectionError("Native age entitlement must be scoped to the application, not a framework/project.")

    links = []
    for target in objects.values():
        if target.get("isa") != "PBXNativeTarget":
            continue
        for phase_id in target.get("buildPhases", []):
            phase = objects.get(phase_id, {})
            if phase.get("isa") != "PBXFrameworksBuildPhase":
                continue
            for build_id in phase.get("files", []):
                build = objects.get(build_id, {})
                reference = objects.get(build.get("fileRef"), {})
                name = Path(str(reference.get("path", ""))).name
                if name not in AGE_FRAMEWORKS:
                    continue
                attributes = build.get("settings", {}).get("ATTRIBUTES", [])
                if build.get("isa") != "PBXBuildFile" or not isinstance(attributes, list) or "Weak" not in attributes:
                    raise InspectionError("Native age system frameworks must use weak linkage for older iOS.")
                if reference.get("sourceTree") != "SDKROOT":
                    raise InspectionError("Native age frameworks must reference the system SDK.")
                links.append({"target": target.get("name"), "framework": name, "weak": True})
    for name in AGE_FRAMEWORKS:
        if not any(link["target"] == "UnityFramework" and link["framework"] == name for link in links):
            raise InspectionError(f"Missing weak {name} linkage on UnityFramework.")
    return {"enabled": True, "entitlement": AGE_ENTITLEMENT,
            "entitlementFile": path.relative_to(export.resolve()).as_posix(),
            "exportEntitlementVerified": True, "systemFrameworkLinks": links,
            "signedCapabilityVerification": "pending-signing"}


def age_framework_reference(value):
    return next((name for name in AGE_FRAMEWORKS
                 if re.search(rf"(?:^|/){re.escape(name)}/", value)), None)


def inspect_age_load_commands(output, linked_libraries):
    """Verify actual Mach-O load commands, including Swift-autolink results.

    PBX Weak attributes describe intended linker inputs; only LC_LOAD_WEAK_DYLIB
    proves a produced binary will tolerate these newer frameworks being absent.
    """
    kind = None
    references = []
    for line in output.splitlines():
        if re.fullmatch(r"\s*Load command\s+\d+\s*", line):
            kind = None
        command_match = re.fullmatch(r"\s*cmd\s+(LC_[A-Z0-9_]+)\s*", line)
        if command_match:
            kind = command_match.group(1)
        library = re.match(r"\s*name\s+(.+?)\s+\(offset\s+\d+\)\s*$", line)
        if not library or kind == "LC_ID_DYLIB":
            continue
        framework = age_framework_reference(library.group(1))
        if not framework:
            continue
        if kind != "LC_LOAD_WEAK_DYLIB":
            raise InspectionError("Native age frameworks require LC_LOAD_WEAK_DYLIB in the archived binary.")
        references.append({"framework": framework, "command": kind})
    listed = {name for line in linked_libraries if (name := age_framework_reference(line))}
    verified = {reference["framework"] for reference in references}
    if not listed.issubset(verified):
        raise InspectionError("Cannot verify native age framework Mach-O load commands.")
    return references


def inspect_identity(info, config, settings=None, archived=False):
    settings = settings or {}
    value = lambda key: resolve_setting(info.get(key), settings)
    expect(value("CFBundleIdentifier"), config["bundleIdentifier"], "bundle identifier")
    expect(str(value("CFBundleShortVersionString")), config["version"], "app version")
    expect(str(value("CFBundleVersion")), config["buildNumber"], "build number")
    expect(value("CFBundleDisplayName"), config["displayName"], "public display name")
    minimum = info.get("MinimumOSVersion", settings.get("IPHONEOS_DEPLOYMENT_TARGET"))
    expect(ios_version(minimum), ios_version(config["minimumIOS"]), "minimum iOS")
    family = info.get("UIDeviceFamily")
    if family is None and not archived:
        family = [int(part) for part in str(settings.get("TARGETED_DEVICE_FAMILY", "")).split(",") if part]
    expect(family, config["deviceFamily"], "supported device family")
    expect(info.get("UISupportedInterfaceOrientations"), ["UIInterfaceOrientationPortrait"], "portrait orientation")
    if info.get("get-task-allow"):
        raise InspectionError("Development entitlement in app Info.plist.")


def inventory(path):
    frameworks, libraries, manifests, entitlements = [], [], [], []
    for item in sorted(path.rglob("*")):
        relative = item.relative_to(path).as_posix()
        if item.suffix == ".framework" and item.is_dir():
            frameworks.append(relative)
        if item.suffix in (".a", ".dylib", ".tbd") and item.is_file():
            libraries.append({"path": relative, "sha256": file_sha256(item)})
        if item.name == "PrivacyInfo.xcprivacy" and item.is_file():
            manifests.append({"path": relative, "sha256": file_sha256(item), "declarations": read_plist(item)})
        if item.suffix == ".entitlements" and item.is_file():
            entitlements.append({"path": relative, "values": read_plist(item)})
    return {"frameworks": frameworks, "nativeLibraries": libraries,
            "privacyManifests": manifests, "entitlementFiles": entitlements}


def inspect_il2cpp(export):
    directory = export / "Il2CppOutputProject/Source/il2cppOutput"
    sources = sorted(item for item in directory.rglob("*") if item.suffix in (".cpp", ".h") and item.is_file())
    if not sources:
        raise InspectionError("Missing generated IL2CPP sources; cannot verify diagnostic exclusion.")
    for source in sources:
        if EXCLUDED_TYPES.search(source.name.encode()):
            raise InspectionError("Review, benchmark or mock type found in production IL2CPP output.")
        with source.open("rb") as stream:
            tail = b""
            for block in iter(lambda: stream.read(1024 * 1024), b""):
                data = tail + block
                if EXCLUDED_TYPES.search(data):
                    raise InspectionError("Review, benchmark or mock type found in production IL2CPP output.")
                tail = data[-256:]
    return {"sourceFilesInspected": len(sources), "excludedTypesAbsent": True}


def inspect_archive(archive, config):
    if not archive.is_dir() or archive.suffix != ".xcarchive":
        raise InspectionError("Archive must be an existing .xcarchive directory.")
    archive_info = read_plist(archive / "Info.plist")
    applications = list((archive / "Products/Applications").glob("*.app"))
    if len(applications) != 1:
        raise InspectionError("Archive must contain exactly one application.")
    app = applications[0]
    app_info = read_plist(app / "Info.plist")
    inspect_identity(app_info, config, archived=True)
    properties = archive_info.get("ApplicationProperties", {})
    expect(properties.get("CFBundleIdentifier"), config["bundleIdentifier"], "archive bundle identifier")
    expect(str(properties.get("CFBundleShortVersionString")), config["version"], "archive app version")
    expect(str(properties.get("CFBundleVersion")), config["buildNumber"], "archive build number")
    executable = app / str(app_info.get("CFBundleExecutable", ""))
    if not executable.is_file():
        raise InspectionError("Archive application executable is missing.")
    signed = (app / "_CodeSignature/CodeResources").is_file()
    entitlements = {}
    if signed:
        command(["codesign", "--verify", "--deep", "--strict", str(app)])
        raw = command(["codesign", "-d", "--entitlements", ":-", str(app)])
        try:
            entitlements = plistlib.loads(raw.encode())
        except (ValueError, plistlib.InvalidFileException) as error:
            raise InspectionError("Cannot inspect signed application entitlements.") from error
        if entitlements.get("get-task-allow"):
            raise InspectionError("Archive permits development debugging (get-task-allow).")
        if entitlements.get(AGE_ENTITLEMENT) is not True:
            raise InspectionError("Signed application is missing its native age entitlement.")
    binaries = [executable]
    for framework in app.rglob("*.framework"):
        binary = framework / framework.stem
        if binary.is_file():
            binaries.append(binary)
    binaries.extend(item for item in app.rglob("*.dylib") if item.is_file())
    dependencies = []
    for binary in sorted(set(binaries)):
        lines = command(["otool", "-L", str(binary)]).splitlines()
        linked_libraries = [line.strip() for line in lines[1:] if line.strip()]
        age_loads = inspect_age_load_commands(command(["otool", "-l", str(binary)]), linked_libraries)
        dependencies.append({"binary": binary.relative_to(archive).as_posix(),
                             "linkedLibraries": linked_libraries, "nativeAgeSystemLoadCommands": age_loads})
    return {"stage": "signed-archive-validated" if signed else "unsigned-archive-validated",
            "archiveTreeSha256": tree_sha256(archive), "checksumAlgorithm": "paws-tree-sha256-v1",
            "signatureVerified": signed, "entitlements": entitlements,
            "nativeAgeCapabilityVerification": "signed-entitlement-verified" if signed else "pending-signing",
            "capabilities": app_info.get("UIRequiredDeviceCapabilities", []),
            "backgroundModes": app_info.get("UIBackgroundModes", []),
            "infoPlist": app_info, "inventory": inventory(archive), "linkedLibraries": dependencies,
            "distributionApproval": "pending-owner", "physicalIPhoneReview": "pending-human"}


def build_receipt(root, export, archive=None):
    dirty = command(["git", "-C", str(root), "status", "--porcelain", "--untracked-files=all", "--",
                     "UnityProject", "scripts"]).strip()
    if dirty:
        raise InspectionError("UnityProject/scripts build inputs differ from HEAD; commit or restore them before receipt inspection.")
    config = unity_config(root)
    expect(config["bundleIdentifier"], BUNDLE_ID, "approved production identity")
    if not re.fullmatch(r"0\.\d+\.\d+", config["version"]):
        raise InspectionError("Production preparation requires an approved 0.x.y app version.")
    if not re.fullmatch(r"[1-9]\d*", config["buildNumber"]):
        raise InspectionError("iOS build number must be a positive integer.")
    metadata = read_json(export / "paws-export.json")
    sha = command(["git", "-C", str(root), "rev-parse", "HEAD"]).strip()
    if not re.fullmatch(r"[0-9a-f]{40}", sha):
        raise InspectionError("Cannot establish source commit SHA.")
    expect(metadata.get("sourceSha"), sha, "export source SHA versus current checkout")
    for key in ("bundleIdentifier", "version", "buildNumber", "minimumIOS", "deviceFamily", "unityVersion"):
        expect(metadata.get(key), config[key], f"export {key}")
    expect(metadata.get("monetizationEnabled"), False, "disabled monetization")
    if metadata.get("nativeAgeAccess") is not True:
        raise InspectionError("Production export must enable nativeAgeAccess.")
    expect(metadata.get("productionDefine"), "PAWS_PRODUCTION", "production compilation define")
    for flag in ("developmentBuild", "scriptDebugging", "profilerEnabled"):
        expect(metadata.get(flag, False), False, flag)
    settings, project = project_settings(export)
    native_access = inspect_native_access(export, project, settings)
    expect(settings.get("PRODUCT_BUNDLE_IDENTIFIER"), BUNDLE_ID, "Unity-generated Xcode bundle identifier")
    info = read_plist(export / "Info.plist")
    inspect_identity(info, config, settings)
    exclusion = inspect_il2cpp(export)
    packages = read_json(root / "UnityProject/Packages/packages-lock.json").get("dependencies", {})
    receipt = {"schema": "paws-ios-release-receipt-v1", "sourceSha": sha, "configuration": config,
               "enabledConfiguration": metadata, "stage": "export-validated",
               "unityVersion": config["unityVersion"], "xcodeVersion": command(["xcodebuild", "-version"]).strip(),
               "exportInfoPlist": info, "exportInventory": inventory(export),
               "exportLinkedSDKReferences": sorted({str(item.get("path", ""))
                    for item in project.get("objects", {}).values()
                    if item.get("isa") == "PBXFileReference" and
                    str(item.get("path", "")).endswith((".framework", ".tbd", ".dylib", ".a"))}),
               "exportProjectCapabilities": [item.get("attributes", {}).get("TargetAttributes", {})
                                             for item in project.get("objects", {}).values() if item.get("isa") == "PBXProject"],
               "unityPackages": packages, "diagnosticExclusion": exclusion,
               "nativeAgeAccess": native_access,
               "physicalIPhoneReview": "pending-human", "distributionApproval": "pending-owner"}
    if archive:
        receipt["archive"] = inspect_archive(archive, config)
        receipt["stage"] = receipt["archive"]["stage"]
    return receipt


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--export", required=True, type=Path)
    parser.add_argument("--archive", type=Path)
    parser.add_argument("--output", required=True, type=Path)
    args = parser.parse_args()
    try:
        export = args.export.resolve(strict=True)
        archive = args.archive.resolve(strict=True) if args.archive else None
        output = args.output.resolve()
        if output.exists():
            raise InspectionError("Receipt output already exists; choose a fresh path.")
        if output.is_relative_to(export) or (archive and output.is_relative_to(archive)):
            raise InspectionError("Write the receipt outside the inspected export/archive.")
        receipt = build_receipt(Path(__file__).resolve().parents[1], export, archive)
        with output.open("x", encoding="utf-8") as stream:
            json.dump(receipt, stream, indent=2, sort_keys=True)
            stream.write("\n")
    except (InspectionError, OSError, ValueError, KeyError, TypeError) as error:
        print(f"iOS release inspection failed: {error}", file=sys.stderr)
        return 1
    print(f"iOS release receipt written ({receipt['stage']}): {output}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
