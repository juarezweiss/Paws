# Paws 1.0 — free release checklist

Reconciled 2026-10-28 for [#88](https://github.com/juarezweiss/Paws/issues/88) /
[PR #93](https://github.com/juarezweiss/Paws/pull/93), against fetched develop
`3cb814f7be0dc117dc4f585bf9e8c8fd65f9d1cd`. Authority:
[RELEASE.md](RELEASE.md), owner free-launch decision (2026-10-26) and
[approved eligibility policy](AGE_ELIGIBILITY.md) (2026-10-27).
This reconciliation changes documentation/tracking only; it supplies no new
runtime, archive, signing, physical-device or store acceptance.

## Contract and execution order

**FREE, ad-free, IAP disabled; unlimited free eligible Hints, including offline.**
No purchase/Restore/Offer Code controls or placeholders, ads, UMP or unnecessary
age prompts. Exclude all monetization SDKs/providers from the release binary;
retain implemented code, pins and compatible storage for separately approved 1.1+.
Normal Menu → Play and existing puzzle/Hint eligibility/completion locks remain.

**#88 / PR #93 contract → #89 UI cleanup → #90 binary verification →
#92 signed RC / human physical QA → #21 distribution readiness.**
#91 runs in parallel: early access/territory decisions feed standalone native
[#103](https://github.com/juarezweiss/Paws/issues/103); its implementation/native
evidence and exact allowlist precede final #90 acceptance. Final #91 declarations
use #90/#92 evidence, and **#91 must be accepted before #21 final readiness**.
It is not a serial metadata step between #90 and #92. #90 exclusion auditing may
proceed while #103 is pending; #92 owns final signed-artifact/human acceptance.

| Issue | Actual status / remaining deliverable | Dependency and acceptance evidence |
| --- | --- | --- |
| [#88 / #93](https://github.com/juarezweiss/Paws/pull/93) contract | Merged into develop as `fe81861` before #89 implementation. | Existing contract accepted/integrated; #89 did not modify or merge PR #93. |
| [#89 UI](https://github.com/juarezweiss/Paws/issues/89) | Implemented on focused branch; integration review pending. Null/disabled composition omits commercial Settings rows. | Source `82f0e8c`: pinned compile, 24/24 focused and 755/755 EditMode pass; 18 locale/portrait/composition cases, repeated immediate offline-port Hints across 90 completed boards, 54 desktop captures. [Evidence and limits](evidence/issue89/validation.json). |
| [#90 binary](https://github.com/juarezweiss/Paws/issues/90) | Pending. Null monetization bootstrap and historical disabled receipts do not prove the final binary. | Integrated #89; #91 early decisions and #103 implementation/automated-native evidence plus exact allowlist before final acceptance; committed-source export/archive inventory. |
| [#103 native access](https://github.com/juarezweiss/Paws/issues/103) | Planned, unimplemented. Standalone Declared Age Range/capability/entitlement and required parental-access handling. | Early #91 legal/permission/retention/older-OS decisions; focused automated/native tests and #90 allowlist. Physical report remains #92. No monetization dependency. |
| [#92 RC / human QA](https://github.com/juarezweiss/Paws/issues/92) | Pending. Prior review-app passes are scoped development evidence. | #90 and settled access scope; owner-approved version/build and signing; reinspection after signing and explicit human checklist result. |
| [#21 distribution](https://github.com/juarezweiss/Paws/issues/21) | Pending. Prepare final GO/NO-GO and owner action list; no submission authorized. | #88/#93, #89, #90, #92 and accepted #91, plus recorded #22 trial disposition. |
| [#91 metadata / compliance](https://github.com/juarezweiss/Paws/issues/91) | Parallel, pending. Terms/Privacy/support URLs, declarations and early legal/access decisions for #103. | Draft now; early decisions feed #103 before final #90/#92; reconcile metadata/screenshots to final artifacts before #21. |

## Completed development and accepted evidence

All four issues are closed and their PRs merged into develop. They are not
remaining implementation tasks. Counts below describe the retained runs at their
recorded sources, not tests rerun by this reconciliation or final free-RC passes.

| Completed work | Exact provenance | Accepted evidence and limits |
| --- | --- | --- |
| [#95 / PR #97](https://github.com/juarezweiss/Paws/pull/97): tutorial clarity, centered instructions, 2s guidance using existing fade | Merge `74ccf46078f39601dc0b35e45c9e992290fc324f`; runtime `9697e3a70f0578f2822eccf1f632e264e860f165`; installed `321e82a61c77165eb262a6f0435a1ce6e7688aa6`. | [Validation](evidence/issue95/attention/validation.json): 683/683 EditMode, 51/51 scripts, 198 localized step/size cases and 315 captures. [Owner pass](evidence/issue95/attention/owner-review-2026-10-28.json): supplied EN/pt-BR/ES presentation/guidance/cleanup checklist, iPhone 17 / iOS 27.0.1. |
| [#96 / PR #98](https://github.com/juarezweiss/Paws/pull/98): fresh ordinary starts Easy → Easy → Medium → existing random, returning-history migration | Merge `259b4f4d97d47ea152fe55c2b75000714fde4df9`; validated/installed `10db178d3535f32f0af8192c11cb44b31c70d852`. | [Validation](evidence/issue96/validation.json): 751/751 EditMode including 122 progression cases, 51/51 scripts. [Owner pass](evidence/issue96/device-preparation.json): fresh-reinstall supplied progression/Next/restart checklist; individual actions, raw sequence and which tutorial route were not separately reported. No broad lifecycle/device pass inferred. |
| [#99 / PR #100](https://github.com/juarezweiss/Paws/pull/100): owner CSV wording in three locales, preserving #95 behavior | Merge `a6cfa5576cbddc0f520b297df428af84aca55ac9`; runtime `e9d9e3f164479b24f41a340d9018b3ac1b456ef1`; installed `9f7a4dad87dc0ece7a5384227b79bba2494ac9b9`. | [Validation](evidence/issue99/validation.json): 751/751 EditMode, 51/51 scripts, 198 cases / 315 captures. [Owner pass](evidence/issue99/owner-review-2026-10-28.json): supplied wording/spacing/guidance/hold/completion/Skip/replay/cleanup checklist on one iPhone; no separately reported per-language/device passes. |
| [#94 / PR #101](https://github.com/juarezweiss/Paws/pull/101): approved sage arrow cells, original dark arrows, 1.10× stronger-ink paws | Merge / current develop `3cb814f7be0dc117dc4f585bf9e8c8fd65f9d1cd`; runtime `7c346d101adfd3d38fb83a4b75527e2b70c7ef78`; installed `39f806901834e7eba0af674941eeaee244c03cb2`. | [Validation](evidence/issue94/implementation/validation.json): 97/97 focused, 754/754 EditMode, 51/51 scripts; twelve native frames. [Owner “all pass”](evidence/issue94/implementation/owner-validation-2026-10-28.json): Easy/Hard readability, tap/drag and arrow hold on iPhone 17 / iOS 27.0.1. Unreported lifecycle, other-device and distribution coverage remain unclaimed. |

#63 established production identity/build tooling; it is not an unfinished
identity implementation. Current configuration remains Paws / com.jweiss.paws /
0.1.0 (1), iPhone-only, iOS 16.3, portrait. Final distribution version/number and
signing still require owner allocation/confirmation. Review-app signatures and
bundle com.juarezweiss.catsarrows.inputreview do not prove production-bundle RC
acceptance. No existing physical pass is extended to a different candidate.
Other inspected recent commits update governance terminology or ignore local
investigation material; they do not complete these release gates.

## #90 / #91 age and access scope reconciliation

The approved policy remains minimum **16 EU / 13 elsewhere**, under-18 parental
permission, and no gameplay/free-Hint exception for reliably known ineligible
users. Unknown age is not Adult; Unknown age or unresolved jurisdiction alone is
not established below-minimum status. No custom questionnaire or birthday
collection. This is a policy contract, **not implemented or legally cleared
free-candidate evidence**.

**2026-10-30 update:** [#90's finding](https://github.com/juarezweiss/Paws/issues/90#issuecomment-6530526490)
establishes a standalone native prerequisite for global free 1.0. #103 owns its
focused implementation/test scope, including the required Declared Age Range
entitlement. [AGE_ELIGIBILITY](AGE_ELIGIBILITY.md#standalone-free-release-prerequisite--2026-10-30-103)
records dated official sources and remaining decisions. This planning update
does not authorize implementation or claim worldwide legal/physical acceptance.

| Component / decision | Release treatment |
| --- | --- |
| IAP/Offer Code, Unity Purchasing, AdMob/GMA, UMP and monetization-only services/bridges/config/age wrappers | Exclude from free binary unconditionally under #90. An access requirement never authorizes shipping monetization. |
| Standalone Apple/native access or parental component, framework/entitlement | Concrete global-release prerequisite tracked by #103. Include only its reviewed exact allowlist and Declared Age Range entitlement; implementation/validation remain pending. #91 supplies early scope decisions. |
| Terms/Privacy/support and limited information/recovery surface | #91 owns reviewed/localized content, owner URLs and access requirements. #89 removes commercial placeholders while preserving existing tutorial/language/settings behavior; it does not implement new age/access policy. |
| Required access implementation and verification | #103 owns precise native signals, approved minimums, pending/denied/revoked permission, correction/recheck, significant updates, privacy, OS guards and focused tests. Owner settles #91 policy gaps and separately assigns implementation before final #90 acceptance; #92 verifies the signed candidate and human physical result. |

Global distribution is the target. Exact open decisions for #91/#103: defensible
jurisdiction and permission evidence where native signals are insufficient;
lawful minimum protective-state retention versus Texas post-verification deletion;
and any demonstrated mandatory-check gap on older supported iOS. Do not silently
raise the iOS 16.3 minimum, exclude territories or block all Unknown users.
Known below-minimum access and required denied/pending/revoked permissions remain
restricted; optional age-sharing refusal alone is not ineligibility. A rating or
Terms checkbox does not itself prove age or enforce access. Preserve the existing
at-least-global-16+ rating proposal, truthful questionnaire and actual regional/
legacy mapping verification; no new rating or legal clearance is asserted here.

#90 can prepare its monetization exclusion audit in parallel, but cannot claim
final binary acceptance before #103 implementation/native evidence and its
allowlist are accepted. #91's eventual final
metadata uses actual #90/#92 artifacts; this staged dependency avoids a cycle.
#67 owns future advertising/IAP/UMP policy, not an excuse to defer an applicable
free-1.0 access requirement. No completed gameplay issue is reopened.

## Remaining free release gates

- [x] #88/#93: reconciled contract merged into develop (`fe81861`).
- [ ] #89: implementation and automated/desktop verification complete; focused PR
  integration approval pending. No commercial rows, labels, placeholders, ad/UMP
  or commercial age routes in free composition; repeated eligible Hints are
  immediate/free with no provider calls. Physical acceptance remains #92.
- [ ] #90: actual non-development production artifact excludes monetization SDKs/
  providers, diagnostics/mocks/review/autorun/benchmark code and commercial config;
  inspect managed/IL2CPP symbols, libraries/resources/linkage, Info.plist,
  entitlements, privacy manifests/data behavior and approved standalone access
  allowlist from #103. Explain OS-only references separately; source/pin presence
  alone proves neither shipping nor exclusion. Preserve source/config/tool SHA,
  artifact checksum, receipt and matching dSYMs.
- [ ] #103: standalone native implementation/automated-native evidence and exact
  #90 allowlist; required entitlement, age/permission/significant-update states,
  correction/revocation, older-OS fallback and privacy lifecycle verified. No
  unnecessary prompt or monetization dependency. Human iPhone report stays #92.
- [ ] #91: accepted early legal/access/permission/retention/older-OS decisions;
  reviewed localized Terms/Privacy/support and reachable public HTTPS URLs;
  factual metadata, final screenshots, privacy/tracking/required-reason/export/
  age-rating declarations from actual candidate and selected-territory assessment.
- [ ] #92: appropriate owner-supplied signature/profile class/expiry and signed-artifact
  reinspection; exact production-candidate human QA for fresh/returning tutorial,
  E/E/M then random, language/portrait, tap/drag/hold/protected cells, Clear/free
  offline Hints/completion, lifecycle/preferences and any required access behavior.
  Record explicit report, reviewer/device/OS/build/SHA and coverage limits.
- [ ] #21: reconcile accepted #91, #90/#92 artifacts and #22 findings into a factual
  GO/NO-GO, rollback/hotfix record and separate owner production action list.

## Tester feedback and owner inputs

[#22](https://github.com/juarezweiss/Paws/issues/22) has T01 and T02, with unknown
exact installed revisions. Arrow/X distinction, tutorial and introductory
progression now have the focused merged follow-ups above; that does not rewrite
original feedback or prove every complaint fixed. Drag drift lacks the pointer
path/reproduction needed to establish a defect; it is not a newly invented
release blocker. Hint caps, ad penalties, board replay/header removal and larger
boards remain unapproved suggestions. No additional 5–8-person quota is inferred;
owner decides trial sufficiency, residual findings and device coverage in #21.

Other owner inputs: final marketing version and next globally increasing
production distribution number (1.0 scope does not edit 0.1.0), public URLs/
contacts, territories and reviewed declarations, current enrollment/account/
signing readiness, route and eventual production GO/NO-GO. No stale local
profile expiry is represented as current status. Every charge/account/credential/
publication/main merge/tag/upload/invitation/submission/release remains separately
controlled; none is authorized or performed by this reconciliation.

## Preserved future work and superseded gates

#67/#82 stay open for future monetization, outside the free critical path. PR #87
must remain untouched; its original code/evidence head
`4acfb61d9625b271f3203ec8d63a608102fbabfa`, runtime/build source
`c0622a361bcc7c7ae2b04d0642f0ce189b094f68` and
[monetized checklist](https://github.com/juarezweiss/Paws/blob/4acfb61d9625b271f3203ec8d63a608102fbabfa/docs/RELEASE_RC_1_0.md)
remain historical inputs. Do not merge that enabled configuration or use its
unsigned rollback archive as new-candidate acceptance. Future purchase safeguards
and failed/inconclusive results are not waived or converted to passes.

This dated reconciliation supersedes the older serial #90 → #91 → #92 ordering,
stale implementation priorities and blanket age-component exclusions in the
[previous #93 checklist](https://github.com/juarezweiss/Paws/blob/64e2e8efdb5f2278efe69a62a4c1af2f9d19c8b4/docs/RELEASE_RC_1_0.md).
October 19 planning snapshots and pre-pivot #82 gates remain explicitly historical.
No source, historical receipt, native archive, gameplay document or evidence
image is replaced by this documentation update.
