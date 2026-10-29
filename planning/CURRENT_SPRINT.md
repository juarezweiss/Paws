# Current Sprint — free 1.0 release readiness

## Current delivery reconciliation — 2026-10-28 (#88/#93)

Paws 1.0 is FREE, ad-free, IAP disabled with unlimited free eligible Hints.
Retain monetization code/pins/storage for 1.1+; exclude monetization providers/SDKs
from shipped binary. No commerce UI/placeholders, ads, UMP or unnecessary age
prompts. Approved access eligibility remains separate and unresolved for final
candidate acceptance in #91; no automatic legal exemption or new component.

Baseline: fetched develop `3cb814f7be0dc117dc4f585bf9e8c8fd65f9d1cd`.
Completed/closed: #94/PR #101 sage arrows/larger paw, #95/PR #97 tutorial clarity,
#99/PR #100 wording, #96/PR #98 fresh Easy/Easy/Medium/random and history migration.
Their exact test receipts and scoped owner iPhone passes are in the
[release ledger](../docs/RELEASE_RC_1_0.md#completed-development-and-accepted-evidence).
No final release-candidate pass is inferred. #63 identity/tooling and #64–#66
provider foundations are integrated; current null monetization composition is
free, but disabled Settings commerce rows still require #89 cleanup.

Execution: **#88 / PR #93 contract → #89 UI → #90 binary → #92 signed RC/human
QA → #21 readiness**. #91 runs in parallel; settle access scope before final
binary/RC acceptance and accept artifact-based metadata/compliance before #21.
#89 is the next implementation task after contract acceptance. No new feature
scope, completed-gameplay reopening, SDK upgrade or build is part of this task.

#22 has two qualitative sessions. Focused approved follow-ups above are complete;
trial sufficiency and residual findings/device coverage need owner disposition,
not an invented 5–8 tester gate. #67/#82 stay future-only; PR #87 stays untouched.
The [checklist](../docs/RELEASE_RC_1_0.md) owns current remaining work and owner
inputs: version/build, public URLs/contacts, territories/access/declarations,
signing/account readiness and distribution route/approvals. The $500 ceiling,
per-charge approval and all human production boundaries remain unchanged.

## Historical planning snapshot — 2026-10-19

The dated baseline, forecasts/priorities and uncompleted-work statements below
are retained for traceability. Where they conflict, the explicit owner decisions
of October 26/27 and current delivery reconciliation above supersede them.
Historical results remain tied to their original source; no failure is erased.

<details>
<summary>October 19 planning record (superseded where explicitly reconciled above)</summary>

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

</details>
