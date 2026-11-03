# #103 native verification — 2026-11-02

## Later owner-profile refresh — signing availability superseded

After this diagnostic snapshot, the owner refreshed the explicit Paws profile.
A copy of the original source-`b9793cc` archive is now development-signed and
verified; [exact sanitized receipt](../development-signing-2026-11-02.json) and
[current owner install/review handoff](../OWNER_REVIEW.md). The original unsigned
archive is unchanged. No new build, agent phone install/launch, physical pass or
Apple Sandbox scenario occurred. Local development signing is not distribution
acceptance. The missing-profile status below remains the dated pre-refresh
record; original diagnostic/Sandbox/OS coverage findings are unchanged.

PR #104 remains on `feat/103-native-age-access`. Review starting head:
`dc57b86c05afe97a790c7101f480e6b4be68a51b`; unchanged runtime source:
`b9793cc36ec5589ebbc538709c78a328b32ee13a`.

## Device signing: blocked by existing profile

[Sanitized local inspection](signing-readiness.json) found two valid signing
identities and one unexpired development profile. It covers `com.jweiss.paws`
through a wildcard App ID and has a matching valid local certificate. Its
expiration is 2027-10-27. The required age entitlement is absent from both the
plist view and the authoritative DER profile data.

The original app/export correctly requires
`com.apple.developer.declared-age-range=true`. Apple's
[TN3125](https://developer.apple.com/documentation/technotes/tn3125-inside-code-signing-provisioning-profiles)
requires iOS entitlement claims to be authorized by the profile allowlist;
the unrestricted-entitlement exception described there concerns macOS.
Xcode's `DECLARED_AGE_RANGE` capability metadata identifies the profile key
and does not require a separate distribution-approval request. This is a missing
profile authorization, not a request for a new service or purchase.

No development-signed device app was produced. No account/profile/capability
changes were made. The source-linked unsigned archive remains unchanged at tree
SHA256 `334cbacfc172ccf128e69110188ccfc695d2af8b253d7f8c71b611acc34a21f7`.
Its app target entitlement is true; minimum iOS16.3 and the previously inspected
weak `DeclaredAgeRange`, `PermissionKit` and `_PermissionKit_UIKit` system links
remain covered by the preserved archive/receipt. No Unity production rebuild
was necessary.

## Actual native runtime controls and OS coverage

[Native summary](verification-summary.json), [source hashes](source-manifest.json),
[test-only UIKit harness](NativeAccessCompatibilityProbe.swift),
[build command](build-command.json) and [actual load commands](linked-frameworks.json)
record an isolated app using the exact committed Swift bridge. The harness is
verification material only, outside the Unity project. No Apple age response is
mocked; InFlight/AwaitingResolution are deliberately seeded **app-owned workflow**
fixtures, not Apple age or parental-consent evidence.

| OS band | Actual execution / remaining gap |
| --- | --- |
| iOS16.3 | Exact runtime unavailable; prior deployment-target compilation only. |
| Pre26 controls: 17.0.1, 18.5 | Same native bridge, without the age entitlement: **8/8 scenarios, 80/80 assertions passed** across ordinary fallback, InFlight recovery, AwaitingResolution preservation and actual process relaunch. No Unity gameplay, device-signing or native-age acceptance is implied. |
| 26.0–26.1 | Runtime unavailable; prior SDK typechecks only. |
| 26.2–26.3 | Runtime unavailable; prior SDK typechecks only. |
| 26.4+ represented by 27.0 | Entitlement-bearing ad-hoc probe failed launch. Unentitled diagnostic control launched but returned **no native callback within 20 seconds** (0/1 scenario passed; report assertion failed). Cause unresolved in this isolated unauthenticated harness; not an Apple error response, Sandbox result or proven Paws defect. |

Entitlement-bearing ad-hoc Simulator probes failed to launch before native
scenarios on **all three** available runtimes (17.0.1/18.5/27.0). They cannot
substitute for a profile-authorized development-signed Paws candidate. Initial
probe attempts preceded supplemental Info.plist metadata; completed-metadata
17.0.1 retry still failed. The unentitled controls isolate some execution evidence
without weakening the production app entitlement or changing production sources.

Individual `control-*-receipt.json` files retain assertions/flags only. Complete
probe execution commands, packaging, producer-source copies and local logs remain
under ignored `.unity-results/issue103-native-verification/compatibility/`;
`verification-summary.json` names those local reproduction artifacts. The copied
build command contains the original absolute workspace paths; reproduce with the
same committed source/harness after adjusting the workspace prefix, rather than
using the historical Simulator identifiers. Original 32 Simulator states were
preserved; all three owned test devices are shut down. No runtime was downloaded
or installed, no production rebuild performed and no physical device accessed.

On the properly entitled signed candidate, verify startup resolves or provides
recovery. If Checking persists for 20 seconds, report the exact candidate, OS and
authorized native scenario for focused investigation. Do not infer normal eligible
gameplay or mandatory-restriction acceptance from the diagnostic controls.

## Actual Apple Sandbox: no completed scenarios

No authenticated age-assurance Sandbox session was exercised. Physical iPhone
account/settings state was not inspected or controlled. No login, test-account
creation, region/account change, install on the owner's phone or purchase occurred.

| Requested scenario | Current status / required evidence |
| --- | --- |
| Under13, 13–15, 16–17, adult native age categories | Not executed in Apple Sandbox; needs a compatible development-signed device app and an owner-operated authenticated iOS26.2+ Sandbox context. |
| Required versus optional/nonrequired regional features | No named-region Sandbox coverage. Existing OS/account test contexts must actually expose the native flags; locale/storefront alone is not evidence of applicable law. |
| Parental permission approved/pending/denied | Initial download permission is system-managed, not an app-visible Boolean. Actual significant-update requests are not exercised by the current empty change registry. |
| Optional age-sharing denial and correction/recovery | Account-backed native responses and the exact app's UI/lifecycle still need owner verification; old policy tests do not satisfy this row. |
| Revocation | Apple's documented Sandbox button triggers server notification. Paws has no server endpoint; notification text alone is not launch-revocation proof. OS launch prevention remains an unverified platform/human scenario. |
| Eligible normal gameplay without unnecessary prompts | Prior source-linked automated shell tests pass; actual signed iPhone gameplay/native-sheet behavior remains pending. |

Therefore **Sandbox passed: 0; Sandbox failed: 0; cases not executed remain
blocked/unverified**, rather than being converted to passes from mocked responses,
SDK typechecks or Simulator launch attempts.

[Apple Sandbox procedure](https://developer.apple.com/documentation/storekit/testing-age-assurance-in-sandbox)
requires the device's Developer/Sandbox Apple Account controls; the
[age-assurance Q&A](https://developer.apple.com/support/age-assurance/) distinguishes
OS support and app-launch revocation. No speculative notification backend or fake
production significant change is introduced for test access.

## Coverage and next physical steps

See the [updated owner handoff](../OWNER_REVIEW.md) for the ordered profile →
signing-copy verification → owner installation → native Sandbox and physical
checklist. The owner must record explicit pass/fail/unavailable with device/OS,
source/artifact, native scenario and region. Do not treat an expired/missing
runtime record, a Simulator-only/ad-hoc probe, a successful signature or an app
launch as a physical pass.

The across-restart significant-update approval durability limit remains unchanged
and must be settled before a future real significant change is enabled.

## Verification record

Repository validation and whitespace checks pass. All 14 copied JSON receipts
parse; native producer and probe hashes match; added local documentation links
resolve. Independent read-only QA found no blocking evidence/owner-handoff
findings. Prior Unity compilation, 806/806 EditMode, 63/63 Python and 83 native
workflow results are preserved, not rerun or presented as new Sandbox coverage.
Runtime sources/scripts are unchanged from `b9793cc`; no production defect was
established by the unsupported probe/control failures and no speculative fix
was added. #103/#90/#91/#92 native/signed/physical acceptance remains open.
