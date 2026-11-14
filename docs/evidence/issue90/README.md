# Issue #90 — final free production binary audit

Builds/validation: 2026-11-13. Final artifact audit completed: 2026-11-14.

**FAIL. Do not accept the final binary or close #90.** The configuration-only
correction excludes executable commercial SDK/provider payloads, but cannot prove
ALL monetization components absent: domain/state/UI code and commercial/mock text
remain, together with serialized provider/review type-name references. Null
composition is not exclusion. No signing, installation, account change, merge or
release was performed.

## Exact artifacts and receipts

Baseline: latest fetched develop
`3ef4e4c2efe33d578559b02f872dc14e20c04dff`, including merged #89/PR #102
(`a08e254`) and #103/PR #104. Fresh final configuration source:
`0ff97c17401c16970a25c4a62db84f3829ab3ca1`. Documentation commits after this source
are not silently substituted for the artifact source.

- [Baseline actual-artifact audit](baseline-audit.json) and
  [production receipt](baseline-release-receipt.json): actual IAP/services/Apple
  provider inclusion, not an inference from packages or source.
- [Final actual-artifact audit](final-audit.json): per-file SHA-256 inventory,
  actual binary symbols/strings/load commands, privacy/plist/resource inspections,
  exact tool hash, archive hash and app/framework dSYM UUID comparisons. The
  production receipt's source/hash must independently match the inspected archive.
- [Final production receipt](final-release-receipt.json): Unity/Xcode configuration,
  identity, native capability inputs, ordinary OS links and diagnostics.
- [Validation](validation.json): 822/822 EditMode; 85/85 Python, including 22
  audit tests; 147 native host workflow checks and five iPhoneOS SDK typechecks;
  pinned compile, repository checks, independent QA and exact log hashes.

Ignored local artifacts are retained with their dSYMs and complete exports:
`Builds/Issue90-baseline-3ef4e4c.xcarchive`,
`Builds/Issue90-final-0ff97c1.xcarchive`, and corresponding `-export` directories.
[Artifact summary](artifact-summary.json) records hashes, identities, commands and retained paths.
The intermediate `513b6af` archive/receipt remains local as superseded diagnostic
evidence; it is not the final candidate. The original slow inspection was
superseded by an equivalent optimized audit; no artifact was changed by inspection.

## Inspection disposition

| Layer | Result and evidence limit |
| --- | --- |
| Compiled managed/IL2CPP and native SDK/provider payload | PASS for executable exclusion: no Unity Purchasing/Services, Apple purchase/Offer Code, Advertising/Google provider generated assemblies or native plugins; no corresponding provider symbols in app/UnityFramework. Raw serialized names are separately failed below. |
| Native libraries/frameworks/plugins | Only UnityFramework is embedded. Ordinary Foundation/UIKit/Swift/engine system links remain. No StoreKit purchase linkage, GMA/AdMob/UMP framework or purchase bridge is linked. |
| Commercial IDs, consent payloads, monetization-only age clients | No configured commercial/test product/ad IDs, GMA/UMP consent resources or monetization-only age wrapper payload found. Preserved source and SDK pins are not claimed as shipped code. |
| Domain and commercial UI/resources | **FAIL.** `CatsAndArrows.Monetization`, `MonetizationCoordinator`, state/storage and UI/localization remain in generated code/metadata/binary. `Remove Ads`, restore/Offer Code text and localized mock-purchase wording remain in metadata. |
| Serialized registry/configuration footprint | **FAIL.** `Data/globalgamemanagers.assets` retains commercial/provider and review type-name references. These are raw serialized references, not proof that their executable assemblies survived. Commercial `RuntimeInitializeOnLoads.json` and services project JSON are removed by the final correction. |
| Debug/test/review | Production Release/no development/debug/profiler options; actual debugger boot setting is zero. Executable review/benchmark/mock/test assemblies/types are excluded by the existing diagnostic receipt and generated-code inspection. Raw registry names and an engine `InternalsVisibleTo`-style `UnityEngine.TestRunner` attribute string remain; the conservative token audit fails them. No executable TestRunner inclusion is inferred from that attribute alone. Mock UI text is independently a real remaining review-resource failure. |
| Required #103 standalone access | `CatsAndArrows.Access`, `PawsNativeAccess.swift`, `PawsAccessWorkflow.swift` retained. Application input has Boolean `com.apple.developer.declared-age-range = true`; framework/project targets do not acquire it. Actual `LC_LOAD_WEAK_DYLIB` for system DeclaredAgeRange, PermissionKit and `_PermissionKit_UIKit` preserves iOS16.3 support. Ordinary OS linkage is not a monetization component. |
| Production identity and free Hints | Paws / `com.jweiss.paws`, approved unchanged 0.1.0 (1), ARM64/device SDK/IL2CPP/Release, iPhone-only/iOS16.3/upright portrait and normal Paws scene. Full regression suite preserves locale/tutorial/progression/persistence and tap/drag/locks. Generated bootstrap supplies null monetization/privacy ports; #89 repeated offline eligible-Hint matrix passes across three locales/portraits with zero commercial-port calls. This is static/desktop evidence, not a new human-phone pass. |
| Privacy/data | Final manifest merge contains Unity engine reasons only: SystemBootTime 35F9.1, DiskSpace E174.1, UserDefaults CA92.1, FileTimestamp 0A2A.1/C617.1. No tracking domains/collected-data declarations or commercial manifest contribution. This records declarations and source/static behavior; no runtime network/data-capture or final App Privacy/legal conclusion is claimed. #103 still processes system age/permission signals under its documented transient/state policy. |

Export-only matches are also classified explicitly: `Libraries/libiPhone-lib.a`
contains Unity engine `UnityPurchasingSettings` symbols/debug-path strings; the
archive is a linker input, not a shipped wholesale library, and the corresponding
provider marker is absent from final app/UnityFramework scans. Two bundled host
`Il2CppOutputProject/IL2CPP/build/deploy_{arm64,x86_64}/UnityLinker.dll` files contain
`NUnit.Framework` tool references. These host linker tools are not inside the app.
Neither category establishes that an executable purchase SDK or test runner ships.
The raw audit intentionally retains these input findings alongside the substantive
shipped-domain/resource failures rather than silently dropping matches.

The audit's provider-wide raw-reference result and strict ALL-components result
remain **FAIL** (39 file/rule matches), even though executable SDK/provider checks pass. No failed field
is relabeled PASS because providers are not initialized.

## Focused configuration correction

Only the explicit free production builder activates `IFilterBuildAssemblies` and
runtime plugin inclusion delegates; installed packages, pins and source remain.
Future review/monetized build entry points retain their original behavior. The
scope removes only a newly generated zero-byte services editor-settings file;
preexisting/nonempty settings are preserved. Export cleanup removes generated
StoreKit linkage/services JSON. It deletes runtime-initializer JSON only when all
entries belong to excluded assemblies, preserves an ordinary-only file byte for
byte, and rejects a mixed or malformed inventory before deletion.

Export regression fixtures preserve the two weak access frameworks, app-only
entitlement input and ordinary resources. Tests also cover failed inspection
tools, UTF16/stream boundaries, every token-rule alternative, unknown embedded
frameworks/libraries, missing/tracking manifests, boot flags, stale receipt source
and mismatched archive hashes. Independent read-only QA findings about weak
allowlist enforcement, provenance binding and export coverage were resolved;
no additional blocking code findings remain. Strict product acceptance still fails.

Unity API references: [assembly filtering](https://docs.unity3d.com/kr/current/ScriptReference/Build.IFilterBuildAssemblies.html)
and [plugin build inclusion](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityeditor/pluginimporter/includeinbuilddelegate).

## Remaining work and distinct release gates

1. #90 remains open. Scope a focused source-level conditional-compilation exclusion
   for runtime/UI commercial integration, preserving future source/pins. Address
   serialized registry references through supported build configuration; do not
   binary-patch them or treat a renamed token as removal. Repeat the final audit
   on the resulting exact commit. This configuration-only PR is a draft correction,
   not complete acceptance or merge authorization.
2. #91 retains territory/jurisdiction/permission evidence, retention/deletion,
   public Terms/Privacy/support and final artifact-based declarations. Static
   absence of ad SDKs is not legal clearance or proof of zero data processing.
3. #103 G1–G6 retain actual regional/native Sandbox, older-OS and mandatory-gap
   disposition, launch-revocation proof and selected-candidate coverage. Conditional
   D1–D4 remain before any real significant update/activation. This audit closes
   none of those gates; the empty significant-change registry remains unchanged.
4. #92 must inspect the signed selected candidate, verify its profile/entitlement,
   and record a new explicit human physical-iPhone report. Unsigned entitlement
   input/weak linkage is complete here; signed capability and runtime/human
   acceptance are pending. Existing owner reports are not extended to this source.
5. #21 distribution readiness remains blocked. Version/build allocation, signing,
   production merge/tag/upload/submission/release require their owner-controlled
   steps. None is authorized by this receipt.
