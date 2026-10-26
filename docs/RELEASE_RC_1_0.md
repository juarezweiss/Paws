# Paws 1.0 — free release checklist

Authority: owner decision 2026-10-26, [#88](https://github.com/juarezweiss/Paws/issues/88),
[RELEASE.md](RELEASE.md). Planning baseline: fetched `develop`
`3a44192697b8f1c416cc49afabeb839c5a0c3dc7`. This file is new on the free docs
branch; PR #87's same-named monetized checklist remains at its immutable evidence
head, not copied into this release path.

## Scope and execution order

FREE, ad-free, IAP disabled; unlimited direct free eligible Hints, including
offline. No purchase/Restore/Offer Code UI, ads, UMP or unnecessary age prompts.
Retain source for 1.1+; exclude monetization SDKs/providers from the shipped binary.
Existing Hint eligibility, solvability and completed-board protection remain.

| Order | Issue / deliverable | Dependencies / evidence |
| --- | --- | --- |
| 1 | [#88 scope and gates](https://github.com/juarezweiss/Paws/issues/88) | Docs-only PR to develop; repository/whitespace validation; owner integration review. |
| 2 | [#89 disabled UI cleanup](https://github.com/juarezweiss/Paws/issues/89) | #88; focused shell/interaction regression and en/pt-BR/es compact/standard/tall review. |
| 3 | [#90 free binary exclusion](https://github.com/juarezweiss/Paws/issues/90) | #89 integrated; clean committed-source production export/archive and actual-artifact inspection. |
| 4 | [#91 store metadata, URLs, compliance](https://github.com/juarezweiss/Paws/issues/91) | #88 for drafts, #90 inventory for final declarations; official current sources and owner-reviewed public content. |
| 5 | [#92 signed RC and human iPhone QA](https://github.com/juarezweiss/Paws/issues/92) | #90 plus reviewed #91 inputs; owner signing/build allocation and explicit scoped physical pass. |
| 6 | [#21 submission/distribution readiness](https://github.com/juarezweiss/Paws/issues/21) | #88–#92 complete; reconcile exact signed artifact, metadata, #22 trial evidence and owner action list. |

#91 drafting may run alongside #89/#90; finalization follows binary evidence.
One responsibility per issue/PR. This planning task performs no implementation,
build/sign/install, publishing or App Store action.

## Free candidate gates — all artifact acceptance pending

- [ ] #89: no commerce/ad/consent/unnecessary-age routes or disabled placeholders;
  repeated eligible Hints have no ad, charge, quota or commercial eligibility gate.
- [ ] #90: no purchasing/Offer Code provider, Unity Purchasing runtime, AdMob/GMA,
  UMP, monetization-only services/native bridges or age client/framework/entitlement
  in actual binary. Inspect symbols/IL2CPP, linkage, embedded resources/frameworks,
  Info.plist, capabilities, manifests and data behavior. Explain ordinary OS-only
  framework references separately; source/package presence alone proves neither
  shipping nor exclusion. No source deletion or SDK upgrade.
- [ ] #90: retain exact source/config/tool versions, archive/app checksum, dSYMs and
  receipt; exclude diagnostics, test IDs, mocks, autorun/review/benchmark content.
- [ ] #91: localized factual metadata and candidate screenshots; reachable public
  HTTPS privacy/support pages and owner contacts; final App Privacy, required-reason
  API/manifest, tracking/ATT, age-rating/export and selected-territory assessment.
  Do not infer no collection or a legal exemption from the free pricing decision.
- [ ] #92: inspect signature/profile class/expiry and recheck exclusions on signed
  artifact; development-device signing remains separate from App Store distribution.
- [ ] #92: owner reports explicit pass/fail, reviewer/device/OS/build/SHA and scope
  for tutorial/play/locales/portrait, tap/drag/arrow hold/protected cells, Clear,
  repeated free/offline Hints, completion lock and lifecycle/preferences. Physical
  tests are human-operated; technical install/launch does not satisfy acceptance.
- [ ] #21: artifact, metadata, URLs, signing, #22 findings and unresolved risks agree;
  owner chooses trial/coverage disposition, route and production GO/NO-GO.

## Owner decisions and separate actions

Pending: final marketing version (the 1.0 scope label does not edit configured
0.1.0), next globally increasing distribution build number, public URLs/contacts,
territories/audience and reviewed declarations, current enrollment/account/signing
prerequisites, physical review/coverage and #22 trial disposition, distribution
route and eventual release approval. Public page publication or hosting changes
require their own authorization; no URL or readiness result is invented.

Owner controls every purchase/renewal, credential/account/console action, production
merge/tag, upload, invitation, submission and release. Preparing this checklist or
closing readiness issues does not perform or authorize those actions. Preserve
source/archive/dSYMs/receipts for a focused hotfix; no remote kill switch is claimed.

## Future monetization is separate

[#67](https://github.com/juarezweiss/Paws/issues/67) tracks 1.1+ IAP/ads/UMP/age
research and enablement. [#82](https://github.com/juarezweiss/Paws/issues/82) stays
open for the pending-purchase restart investigation and future purchase safety.
No IAP product submission, Offer Code distribution, AdMob IDs/traffic, UMP console
readiness, monetization age capability or purchase/ad physical acceptance blocks
purchase-free 1.0. #91 still checks requirements actually applicable to the free
app and selected territories; document any concrete prerequisite separately.

Preserve [PR #87](https://github.com/juarezweiss/Paws/pull/87), evidence head
`4acfb61d9625b271f3203ec8d63a608102fbabfa`, runtime/build source
`c0622a361bcc7c7ae2b04d0642f0ce189b094f68`, and its
[monetized checklist](https://github.com/juarezweiss/Paws/blob/4acfb61d9625b271f3203ec8d63a608102fbabfa/docs/RELEASE_RC_1_0.md).
Its tests, unsigned archives, unresolved signing/privacy/purchase/age gates and
inconclusive results remain scoped to that source. Do not merge enabled config
into free 1.0 or relabel historical evidence as the new candidate's acceptance.
Future enablement starts from latest fetched develop, preserving newer game/UI/
assets/ProjectSettings, with separate owner scope, safety validation and activation.
