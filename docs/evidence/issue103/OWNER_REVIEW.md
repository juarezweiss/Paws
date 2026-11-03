# #103 owner iPhone review — pending

## Development-signed copy ready — 2026-11-02

The owner refreshed/downloaded the explicit Paws development profile. Signing
preparation has now completed on a **copy** of the existing archive, with no
Unity/Xcode rebuild. [Sanitized signature/artifact receipt](development-signing-2026-11-02.json).

- Runtime/archive source: `b9793cc36ec5589ebbc538709c78a328b32ee13a`; reviewed
  documentation head before signing: `ba66da6c91204d9c8cdf384b30086c7e9aa733b6`.
- Owner-installable app (local, ignored):
  `Builds/Issue103-Development-b9793cc-20261102/Paws-Development.xcarchive/Products/Applications/Paws.app`.
  Archive/dSYMs and a receipt/OWNER-INSTALL.txt remain in the same output directory.
- Paws **0.1.0 (1)**, bundle **com.jweiss.paws**, iPhone/portrait, minimum iOS16.3.
  Signature is **local development**, with `get-task-allow=true`, expiring
  **2027-11-03 01:12:21 UTC**. This is not App Store distribution acceptance.
- Exact explicit app authorization, profile plist/authoritative DER age entitlement,
  existing valid local certificate/team/prefix, previously registered test-phone
  coverage/expiry and embedded profile bytes pass. Actual app XML/DER entitlements,
  extracted leaf certificate and deep/strict app/nested-framework signatures pass.
- Both ARM64 binaries match retained dSYMs; code sections, nonsignature load commands
  and original bundled resources are unchanged. Original unsigned archive remains
  at SHA256 `334cbacfc172ccf128e69110188ccfc695d2af8b253d7f8c71b611acc34a21f7`.
- The first certificate-extraction verification invocation used incorrect optional-
  argument syntax. It was recorded as incomplete, corrected, and verification
  passed without rebuilding or signing again; no product defect was established.

Independent read-only QA found no blocking findings and independently confirmed
artifact/executable hashes, strict signatures and matching ARM64/dSYM pairs.
Restricted-sandbox trust-service errors disappeared with the same read-only
checks outside that sandbox; no app/signing change was needed.

**Next owner steps:** install that exact signed app through the local Xcode
workflow; record device/OS, bundle/version/build and source/artifact; then perform
and report the human checklist below in available authenticated Sandbox contexts.
If the signed candidate remains Checking for 20 seconds, report its exact build/
OS/native scenario. No phone installation, launch or physical interaction was
performed by the agent. Sandbox remains **0 executed / 0 passed / 0 failed**;
physical review and final #90/#91/#92 acceptance remain pending. No new product
policy decision, account change, purchase, merge, submission or release occurred.

The pre-refresh blocked readiness record below is historical and superseded for
signing availability only. Its original unsigned/Simulator/Sandbox findings and
all missing coverage remain unchanged.

<details>
<summary>Historical November 2 handoff before owner profile refresh</summary>

## Native verification handoff — 2026-11-02

The owner authorized development-signing preparation with existing credentials,
but **no compatible local provisioning profile is available**. The existing
unexpired development profile covers Paws and has a matching valid certificate;
its plist and authoritative DER allowlist omit
`com.apple.developer.declared-age-range=true`. The app/export correctly requires
that entitlement. [Sanitized readiness](native-verification-2026-11-02/signing-readiness.json).
No development-signed device app was produced; the original unsigned archive
and prior tests remain intact. Agent installation on the owner's iPhone is not
authorized. No physical pass is recorded here.

Remaining steps, in order:

1. Owner enables Declared Age Range for explicit `com.jweiss.paws` and refreshes
   a development profile authorizing the entitlement, using the existing valid
   certificate and registered test phone. This is an owner account/profile action;
   no account changes or purchases were performed by this task. This does not
   imply a separate managed-entitlement approval application.
2. Resume the already authorized signing preparation on a **copy** of the existing
   source-`b9793cc` archive. Verify profile/certificate/bundle/device/expiry matching,
   the app signature and age entitlement, and nested framework signing. A fresh
   Unity build is unnecessary while runtime inputs are unchanged.
3. After signing verification, owner installs that exact build through their local
   Xcode workflow. Record device/OS, bundle/version/build and archive/source. Device
   readiness or launch success is not physical acceptance.
4. On a native-capable iPhone (Sandbox requires iOS26.2+), the owner verifies an
   existing authenticated Sandbox Apple Account in Settings → Developer → Sandbox
   Apple Account → Manage → Age Assurance. No credentials belong in chat or the
   repository; account availability/region selection was not verified on the
   physical phone. No test-account creation or region/account changes were made.
5. Run the cases below, report each explicit pass/fail/unavailable and its native
   scenario/region. Test both an available required-regulatory context and an
   available nonrequired/optional-sharing context; record missing regional accounts
   rather than assuming worldwide coverage.

Apple references: [profile entitlement authorization](https://developer.apple.com/documentation/technotes/tn3125-inside-code-signing-provisioning-profiles)
and [age-assurance Sandbox procedure](https://developer.apple.com/documentation/storekit/testing-age-assurance-in-sandbox).

</details>

## Human checklist

Record reviewer, device/OS, app version/build, exact source/artifact, scenario and
explicit pass/fail. Report unavailable scenarios separately. Sandbox identities
are test inputs, not evidence of a real person's age or parental approval.

- Startup must resolve or offer recovery. If Checking persists for 20 seconds,
  record the exact build/source, OS and native scenario; the unentitled iOS27
  diagnostic control timed out and has not established a signed-candidate defect.
- Ordinary eligible/Unknown, nonmandatory startup: Menu → Play without an age
  questionnaire or unnecessary native sheet. Free offline Hints still work.
- Required native age scenario: shared under13 cannot enter/resume a puzzle,
  tutorial or Hint. Information, language, native Settings and recheck remain
  usable; permitted range correction restores access without deleting preferences.
- Optional sharing decline/unavailable: ordinary Unknown play is preserved when
  there is no prior protective state or missing mandatory obligation.
- Required unresolved range / prior protection: no gameplay bypass through Menu,
  tutorial, Clear, Next, Hint, background, restart or a late callback. An interrupted
  optional first request alone must not become a permanent restriction.
- On native-capable iOS, review readable localized recovery/status/information
  and comfortable controls at the actual safe areas; test tap/drag/arrow hold,
  completion lock, Clear/new game and language changes after recovery.
- Required parental app-consent revocation: Apple's Sandbox **Revoke App Consent**
  procedure documents sending `RESCIND_CONSENT` to a developer server. Its
  "Notification Triggered" message alone does not prove OS launch blocking.
  Paws has no server-side access/notification endpoint; no new backend is added
  for this test. Record server notification as inapplicable here and platform
  launch-revocation behavior as unverified unless the owner can exercise an
  appropriate supported parental-access scenario. Launch success never proves consent.
- Foreground/background and native sheet dismissal: no recursive sheets, stale
  approval, lost puzzle state or unnecessary return to gameplay from Settings.

The production significant-change registry is empty. Do not fabricate an update
in this candidate. Pending/denied/approved update and adult-acknowledgement paths
have deterministic/native compile evidence; their actual iOS26+ Sandbox behavior
remains unverified. Before a future real significant update, #91 must settle
account-safe approval durability across process restart and validate the native
flow (including account changes) on its exact candidate. Current-process
background/resume does not repeat a handled change; no global persisted approval
is used across Apple accounts.

Keep older-iOS and iOS26+ coverage explicit. Compilation with an iOS16.3 deployment
target does not establish runtime acceptance on that OS or on every regional account.
