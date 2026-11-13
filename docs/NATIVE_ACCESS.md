# Native age and parental access — #103

## Final acceptance review — 2026-11-13

[Final assessment](evidence/issue103/acceptance-2026-11-13/README.md): **NOT READY**
for develop merge. One current blocker:26.4+ update-only regulatory flags trigger
an unnecessary age request with no pending change. The focused host reproduction
fails3/5 cases; it is not native/Sandbox coverage. No production code changed.
Retained806/806 EditMode and other prior evidence remain valid at their recorded
source; CI is green. The owner iPhone17/iOS27.0.1 pass is preserved within its
reported scope. Remaining native/legal/final-artifact evidence is classified as
release gates; dormant significant-update gaps are explicit activation gates.


## Owner profile refresh and development-signing handoff — 2026-11-02

[The refreshed-profile receipt](evidence/issue103/development-signing-2026-11-02.json)
records successful development signing/verification of a copy of the unchanged
source-`b9793cc` archive. The explicit profile authorizes Declared Age Range in
plist and authoritative DER, covers the prior test phone, and matches an existing
valid local identity. Actual signed entitlement/certificate, strict nested
signatures, ARM64/dSYMs, unchanged code/resources and original archive checksum
pass. Profile expiry: 2027-11-03 01:12:21 UTC; `get-task-allow=true` marks local
development signing, not distribution approval. No rebuild or agent phone install.

[Current owner handoff](evidence/issue103/OWNER_REVIEW.md) identifies the exact app.
Owner installation and explicit human/Sandbox results remain pending; no native
region/OS gap or final #90/#91/#92 gate is closed by a valid signature. The older
November 2 missing-profile statement below describes the pre-refresh snapshot,
now superseded for signing availability only; its diagnostic findings are retained.

Follow-up 2026-11-02: [native verification and signing readiness](evidence/issue103/native-verification-2026-11-02/README.md)
reuse the unchanged runtime/archive. The existing local profile does not authorize
the age entitlement, so a development-signed device candidate is blocked. Native
Simulator diagnostic controls are reported separately from entitlement-enabled
Paws, authenticated Apple Sandbox and human iPhone acceptance. No phone install,
account/profile change or merge is authorized by that verification task.

Validation recorded 2026-11-01: [source-linked results](evidence/issue103/validation.json),
[unsigned archive receipt](evidence/issue103/unsigned-release-receipt.json) and
[pending owner checklist](evidence/issue103/OWNER_REVIEW.md). Implementation is
review-ready; actual iOS runtime/Sandbox, signed capability and physical gates
remain open. This is not final #91 legal or #90 binary-exclusion acceptance.

## Scope settled before coding — 2026-10-31

Owner assignment authorizes implementation and one PR to develop, with no merge,
signing, installation or release. Physical iPhone acceptance remains pending.
Approved: Unknown is not Adult and allows play unless reliable evidence establishes
ineligibility or a legally mandatory permission is missing; keep minimum iOS 16.3;
process age data transiently and retain only necessary lawful protective workflow
state. Minimum ages remain 16 EU / 13 elsewhere; under-18 parental permission.

## Current applicability and source checks

- Texas: [SB2420 §§121.051–.055](https://capitol.texas.gov/tlodocs/89R/billtext/html/SB02420F.htm)
  covers free apps, store-provided age/consent information and deletion of provided
  personal data after verification. [Fifth Circuit June 4 stay](https://www.ca5.uscourts.gov/opinions/pub/25/25-51073-CV0.pdf)
  and [Supreme Court docket, July 6 denial](https://www.supremecourt.gov/search.aspx?filename=/docket/docketfiles/html/public/25a1389.html)
  support current enforcement. [Apple June 3 notice](https://developer.apple.com/news/?id=sg176nne)
  applies its implementation to new Texas accounts from June 4, 2026. No later
  contrary official order was found in the checked sources on October 31.
- Utah: [current developer requirements](https://le.utah.gov/xcode/Title13/Chapter76/C13-76-S202_2026031820260318.pdf)
  postpone developer obligations to May 6, 2027; [construction/limits](https://le.utah.gov/xcode/Title13/Chapter76/C13-76-S404_2026031820260318.pdf)
  preserve previously installed access subject to revocation/significant changes.
- Louisiana: [Act 185 of 2026](https://www.legis.la.gov/Legis/ViewDocument.aspx?d=1475238)
  prevents Act 481 of 2025 taking effect and replaces it with requirements effective
  July 1, 2027. Apple's [February regional notice](https://developer.apple.com/news/?id=f5zj08ey)
  predates both deferrals; its Utah/Louisiana 2026 dates are not current legal dates.
- Brazil: [current ECA Digital](https://www2.camara.leg.br/legin/fed/lei/2025/lei-15211-17-setembro-2025-797997-normaatualizada-pl.html)
  and [implementing decree, Articles 25–26](https://www.gov.br/mj/pt-br/assuntos/arquivos-imprensa/sedigi/decretos-eca-digital.pdf)
  require proportionate age-appropriate protection and technical age-signal
  reception. Paws has no legally adult-only content, communication, commerce or
  profiling; do not add a universal 18+ gate. Support native signals and preserve
  protective defaults for all users. #91 owns final release/legal assessment.
- Australia/Singapore: Apple's notice concerns 18+ downloads, not a universal
  puzzle-game adult gate. EU contractual 16 is independent of those mechanisms.

These are dated implementation inputs, not exhaustive worldwide legal clearance.
Revalidate for final #91 territory/declaration acceptance and when laws change.

## Minimal native contract

[Apple framework/entitlement](https://developer.apple.com/documentation/declaredagerange/):
`com.apple.developer.declared-age-range = true` on the application target. Add
availability-guarded, weak system linkage; no commercial adapter, SDK or PR #87.
Installed Xcode 27.2 SDK interfaces confirm the APIs below. A modern SDK does not
change the iOS 16.3 deployment minimum.

| OS | Required path |
| --- | --- |
| 16.3–18.x | No range API or speculative native mandate. Unknown permits ordinary play absent existing protective workflow requiring recovery. No blanket upgrade. |
| 26.0–26.1 | Range API exists; no regulatory preflight. No unsolicited optional request. A user recovery/recheck can request a range. |
| 26.2–26.3 | Use `isEligibleForAgeFeatures` for per-account native applicability and range checks; use PermissionKit for actual significant changes. |
| 26.4+ | Inspect the complete `requiredRegulatoryFeatures` set: required range, parental significant-change consent, adult acknowledgement. Do not hardcode state dates as runtime restrictions. |

Native eligibility is not proof of adulthood, consent or EU residence. Apple
provides no residence field and no initial-download-consent Boolean. Keep initial
consent explicitly system-managed/not exposed, never Approved inferred from an
installation, sharing choice, declaration source or parental-control flag.
[Apple's Q&A](https://developer.apple.com/support/age-assurance/) describes Apple's
download/launch enforcement and revocation blocking. The app handles actual
observable required permissions; Terms remain a condition of under-18 use where
no separate verification is exposed. Do not invent a permission deficiency or
fake significant update to obtain a new parent prompt. #91 records this evidence
limitation; it is not a claim that the app independently verified a parent.

Outside mandatory native handling, ordinary startup performs no range prompt.
Only a necessary recovery action may request optional sharing. Unknown/declined
optional sharing is not ineligibility. A range entirely under 13 restricts globally;
EU under16 restriction additionally requires reliable EU jurisdiction evidence.
No residence is inferred from storefront, locale, timezone or regulatory eligibility;
unresolved jurisdiction alone does not establish ineligibility. Partial/straddling
ranges do not prove adulthood or below-minimum status.

## Significant updates and recovery

Current free composition has no newly declared significant change: no fake
change is configured merely to exercise a sheet. A future actual significant
change must provide a stable identifier, affected version and localized meaningful
description, with #91's documented trigger decision. Test the real path through
injected configurations without shipping diagnostic scenarios.

For 26.2 PermissionKit, `ask` sends a request; it is not approval. Consume matching
question-ID responses, preserve pending workflow across restarts and fail safely
on denial/unavailable/error. Use newer API results only with correct availability
guards. Adults receive a required acknowledgement instead of parental requests.
Do not repeat an already handled change or accept a stale callback for a newer
check/change. Suspend board input and disregard stale approvals on lifecycle
transitions. Correction uses native Settings and recheck; never guess birthdays.

Apple blocks launches when parental app consent is revoked. Re-evaluate exposed
native state on resume; do not fabricate a revocation callback. Paws has no
server-side access, so no speculative notification backend is added. Reassess
`RESCIND_CONSENT` if server-side access is introduced.

## Data lifecycle

No DOB, IDs, raw responses, age/category/source, account identifiers or region are
written to storage, logs or telemetry. Consume coarse native evidence transiently
and release it after policy evaluation. Apple owns its age-response cache and
native correction path; the app does not duplicate it.

Persist only a neutral application-owned workflow phase, with no restriction reason:
`InFlight` is written before a necessary range request; `AwaitingResolution` is
established before exposing a completed app policy decision that still requires
recovery. An interrupted first optional InFlight request alone is not evidence
of ineligibility and must not create a permanent Unknown gate. A prior
AwaitingResolution survives failed/declined refreshes and restart, and clears only
on a sufficiently reliable fresh complete eligibility decision. No age/source,
permission choice, region or raw store field is stored in either phase.

This limited operational derivative is necessary to preserve known restrictions
without treating an interrupted optional request as knowledge of a child. Texas
§121.055(a) permits protection/compliance processing; (b) requires deletion of data
provided by the store. The workflow phase is generated by the application and
contains none of those provided values. This is a narrow literal implementation
rationale, not a claim of anonymity, exemption from all privacy laws or final
legal clearance. Native raw evidence is released after verification.

Significant-change baseline and a pending question UUID identify application
workflow only. They do not store a parent's answer or child's age. Handled approvals
remain transient for the current native account context, survive ordinary
backgrounding, and are invalidated on account/service-lifetime change. An older
service's asynchronous work must not publish into a replacement service.
Protect from stale-response/change mixups and do not erase unrelated preferences.
This narrow reading distinguishes app-created workflow from data provided by the
store under Texas §121.055; it is an implementation rationale, not legal clearance.
Any proposed extra retention needs a separate necessity/legal assessment.

## Implementation and verification plan

Recovery UI reference lock: existing `GameShellView.ShowSettings` owns scrollable
safe-area layout and 48+ point controls; `ShowModal` owns information hierarchy;
Apple's native sheets own age/parental decisions. Reuse the cream canvas, existing
ink/text, font and controls without new imagery or color-only statuses. Bundled
Refero copy guidance supplies clear outcome → recovery action. Live Refero lookup
was unavailable (no active subscription); no purchase was made. Validate localized
short/tall portrait rendering and actionable recovery without an assent bypass.

1. Independent deterministic policy/coordinator plus a narrow native bridge;
   no Core/Gameplay or monetization dependency.
2. Gate shell entry/resume/tutorial/Hint/board input; preserve information/recovery,
   localization, normal eligible play and free offline Hints.
3. Reproducible capability/weak linkage and tests; exact-source unsigned export/
   native build evidence for #90/#91, with signed/profile review deferred to #92.
4. Focused policy/interaction/build/native tests, pinned Unity compile/EditMode,
   repository validation and independent read-only QA. Report unavailable native
   OS/regional/sandbox coverage explicitly. The November 2 direct CoreSimulator
   inventory and qualified diagnostic controls are recorded in the follow-up;
   they do not satisfy authenticated age-assurance Sandbox or physical acceptance.

Rollback must retain the standalone protections when legally required; returning
to the old free bootstrap is not a compliant release rollback. Owner retains
all production, signing, installation and physical-acceptance boundaries.

## Significant-update durability limit

The shipping change registry is empty. For a future configured significant update,
current-process approval is not repeated merely after background/resume. Across a
process restart, no account-safe durable approval record is available without
additional account-scoped storage. Global local approval flags would let another
Apple account inherit approval and are deliberately excluded. The safe path may
request the real pending update again after restart. Before the first such update,
#91 must resolve durable account-scoped/idempotent handling and validate it with
Apple sandbox; this PR does not claim the across-restart no-repeat criterion is
complete or enable a speculative cloud capability.
