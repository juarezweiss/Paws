# Current Sprint — iOS tester readiness and first trial

## Current owner amendment — 2026-10-26 (#88)

**Paws 1.0 = FREE, ad-free, IAP disabled, unlimited free eligible Hints.**
No purchase/Restore/Offer Code UI, ads, UMP or unnecessary age prompts. Exclude
monetization providers/SDKs from shipped binary; retain existing source/pins and
storage for separately approved 1.1+ monetization (#67/#82). Preserve PR #87 and
its evidence; never merge its enabled configuration into the free release.

Current execution order: [#88](https://github.com/juarezweiss/Paws/issues/88)
scope → [#89](https://github.com/juarezweiss/Paws/issues/89) UI →
[#90](https://github.com/juarezweiss/Paws/issues/90) binary exclusion →
[#91](https://github.com/juarezweiss/Paws/issues/91) metadata/URLs/compliance →
[#92](https://github.com/juarezweiss/Paws/issues/92) signed RC/human iPhone QA →
reused [#21](https://github.com/juarezweiss/Paws/issues/21) distribution readiness.
#91 drafts may run alongside #89/#90; final declarations require actual binary
inventory. [Release checklist](../docs/RELEASE_RC_1_0.md) defines current gates.

Baseline inspected: fetched develop `3a44192697b8f1c416cc49afabeb839c5a0c3dc7`.
#63 identity/build foundation and real provider foundations #64–#66 are already
integrated; current production composition is disabled. Shipping exclusion and
clean UI still require #89/#90 evidence. Older claims that no SDK/provider exists
refer to their dated baseline, not current repository contents.

#67 research/enablement and #82 IAP findings continue separately, without blocking
purchase-free 1.0. Existing puzzle/accessibility guarantees, #22 trial findings,
owner physical review and production control remain. The owner must explicitly
record trial/coverage disposition; this scope decision does not claim a trial pass.
No feature implementation/build/store action is authorized by this planning task.
The $500 cash ceiling and separate approval for every charge are unchanged.
Older priorities, baselines and release gates below are dated history where they
conflict with this explicit owner amendment; historical results remain scoped.

**Primary work:** [#21](https://github.com/juarezweiss/Paws/issues/21) and
[#22](https://github.com/juarezweiss/Paws/issues/22)

**Source baseline:** `develop` at `5b9e06b` (2026-10-19)

**Status:** playable Paws feature work is integrated; distribution and broader
trial evidence remain pending.

## Current product baseline

- Paws is a portrait iOS-first logic game with Easy 5×5, Medium 6×6 and Hard 7×7
  procedural boards. `Play`/`Next board` choose among the three modes with a
  persistent two-repeat limit; first-ever Play uses the fixed Easy tutorial.
- The complete loop includes tap/drag marking, Clear/Menu, immediate unlimited
  Hint, arrow-path hold, idle Hint attention, exact-paw completion, celebration
  and return to the locked completed board in English, pt-BR and Spanish.
- SDK-free monetization architecture and persistence are merged, but normal
  composition is disabled and ad-free. Only explicit Editor/development mock
  review flows exist; no real provider or store configuration is present.
- The latest behavior-changing baseline passed pinned compilation and 473/473
  EditMode tests in PR #56. Later PRs #58/#60 were documentation-only.
- The owner reviewed and approved the current Paws physical-iPhone appearance on
  2026-10-19; this closes the pending PR #54 branding appearance gate.

## Sprint outcomes still required

- [ ] Confirm durable bundle identifier, semantic version/build number and a
  reproducible clean iOS tester build containing only the intended game path.
- [ ] Reinspect the final archive's SDKs, capabilities and data behavior; prepare
  accurate beta metadata and privacy/export answers without inventing contacts.
- [x] Record owner approval of the current Paws physical-iPhone appearance,
  including branding, paw readability and localized visual fit.
- [ ] After trial-build settings change, hand the clean build to the owner for
  task-specific interaction/lifecycle review. Smaller-phone comfort and measured
  Medium/Hard latency remain unverified.
- [ ] Present the exact enrollment/distribution step for explicit approval before
  any purchase, upload or invitation.
- [ ] Collect additional versioned friend-trial responses in #22, summarize only
  repeated evidence and open focused follow-ups after owner prioritization.

## Boundaries

- No Apple Developer Program purchase, subscription, TestFlight upload, tester
  invitation, production merge, tag, store submission or release is authorized.
- Android physical validation remains deferred in #28 and does not block this sprint.
- Real ads/IAP, analytics SDKs, board save/resume, difficulty recalibration and
  speculative feature work are outside this sprint.
- The first recorded friend session is useful qualitative evidence, not proof of
  release readiness, visual direction or calibrated difficulty.
