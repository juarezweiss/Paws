# Paws Roadmap

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

Reconciled against `develop` and owner-approved Issues/PRs on 2026-10-19. Scope,
budget and production gates follow the [Master Project Plan](MASTER_PROJECT_PLAN.md).
Every purchase or subscription still requires separate explicit owner approval.

## Completed — product foundation and playable loop

- Unity 6.3 LTS project, deterministic puzzle model, human-style solver,
  exhaustive uniqueness verification and reproducible generation (#1–#5, #14).
- Portrait mobile board, safe-area layout, tap cycling, axis-locked drag marking,
  immediate clue-derived Hint, Clear/Menu, exact-paw victory and completed-board
  lock (#6–#8, #19, #23).
- English, Brazilian Portuguese and Spanish presentation plus a fixed Easy
  guided/independent first-play tutorial (#26).

## Completed — difficulty, replay and player-facing polish

- Easy 5×5, Medium 6×6 and Hard 7×7 generation with profile-specific rules,
  deterministic pruning and independent solvability/uniqueness admission
  (#32, #35, #37).
- `Play` and `Next board` choose among all three difficulties, prevent more than
  two consecutive successful starts in one difficulty and persist that history;
  first-ever Play routes through the tutorial (#43).
- Hold-to-highlight arrow paths, six-second non-moving Hint attention and the
  timed ordinary-board completion celebration with static-board return
  (#47, #48, #44).
- Player-facing and gameplay terminology is **Paws** with paw artwork, display
  name and icon; current README/repository links are aligned. Compatibility-sensitive
  `CatsAndArrows` project identifiers remain intentionally unchanged
  (#53, #55, #57, #59).

## Completed — monetization foundation, inactive by default

- Provider-agnostic policy and ADR for every-fourth-completion opportunities,
  rewarded hints and one-time Remove All Ads ownership (#29).
- SDK-free coordinator, durable state, purchase/ad ports, unavailable-provider
  defaults and Editor/development-only no-charge mock flows are integrated (#30).
- No provider, ad/IAP SDK, product, price, store configuration, live charge or
  production traffic exists. Normal builds remain free with direct unlimited Hint.

## Current milestone — iOS tester readiness and first trial

- Finish the still-open clean-build and distribution-readiness work in
  [#21](https://github.com/juarezweiss/Paws/issues/21): durable bundle/version/build
  identity, trial-only build contents, final SDK/data inspection, beta metadata
  and enrollment/signing prerequisite review.
- The owner approved the current Paws physical-iPhone appearance on 2026-10-19.
  A future clean tester build still needs its own task-specific technical/device
  checks when build configuration changes.
- Continue the qualitative friend-trial tracker in
  [#22](https://github.com/juarezweiss/Paws/issues/22). One unversioned session is
  recorded; broader feedback, repeated-obstacle triage and a visual-direction
  decision remain pending.
- Enrollment, purchase, TestFlight upload, invitations and public release remain
  separate owner-approval gates.

## Next milestones — evidence-gated

1. Produce a versioned, reproducible iOS tester build tied to a commit and hand
   it to the owner for the required physical review.
2. Run the small friend trial and create focused follow-up Issues only for repeated
   blockers or an owner-selected improvement.
3. Prepare an iOS release candidate, store materials and accurate declarations
   after the trial; public merge/submission requires explicit owner approval.
4. Re-estimate later work from trial evidence rather than reusing the obsolete
   pre-playable-project forecast.

## Later or separately approved

- Android physical-device input/comfort validation (#28), deferred and not an
  iOS-trial blocker.
- Board save/resume, calibrated difficulty thresholds, additional locales and
  broader visual refinement when feedback justifies them.
- Real monetization-provider integration or activation, analytics and acquisition
  only through separate tracked decisions, validation and production approval.

Do not delay the first controlled trial for a backend, final art, live monetization
or speculative hardening.

</details>
