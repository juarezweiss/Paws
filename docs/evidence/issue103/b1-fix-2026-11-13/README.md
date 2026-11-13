# #103 / PR #104 B1 fix and merge reassessment — 2026-11-13

**MERGE READY for develop integration, subject to green CI and explicit owner
approval. No remaining implementation merge blocker found.** This is not release
or complete #103 acceptance: G1–G6 and conditional D1–D4 remain open.

Runtime fix source: `190693d3ca1c2db61f3f35f3a714076519056480`.
[Source hashes and validation](validation.json) link the exact checked inputs.
No unrelated game, monetization, scene, entitlement or ProjectSettings change.

## B1 resolution

The26.4+ branch consumes `declaredAgeRangeRequired` separately from parental/adult
significant-update flags. It now calls a pure feature-specific routing entry
point that cannot consume generic regional eligibility. Update-only flags are
relevant only when an actual applicable significant change is pending. The
previous `regulated = !features.isEmpty` assignment is removed.

Mandatory range checks still request age. Existing protective checkpoints,
explicit user rechecks and actual applicable significant updates remain reachable.
Pre26,26.0–26.1 and26.2–26.3 routing is unchanged, including the older eligibility
fallback. Permission/age policy, transient-data handling, account/request guards,
protection persistence and free eligible gameplay are untouched. This follows
Apple's [separate regulatory features](https://developer.apple.com/documentation/declaredagerange/agerangeservice/regulatoryfeature)
without changing the entitlement, frameworks or approved ages.

## Fresh verification

- **147/147 host native workflow checks:** all83 previous checks retained plus64
  modern combinations of required-range, parent/adult flags, actual change,
  checkpoint and user action. This includes all three original update-only/no-
  change counterexamples and mandatory/recovery/real-update positive controls.
  Tests use the same modern entry point called by the production bridge.
- Real iPhoneOS SDK typechecks pass at ARM64 deployment16.3/26.0/26.2/26.4/27.0.
  The installed MacOSX27.2 SDK was supplied explicitly for the host runner; no
  toolchain/SDK install or test-runner change was needed. Known legacy availability/
  deprecated-control warnings remain scoped to unchanged/deferred code.
- Pinned Unity6000.3.25f1 discovery and compilation pass; fresh EditMode report
  **806/806 passed**, zero failed/skipped/inconclusive and successful editor exit.
  Python contracts **63/63 passed**; repository and whitespace validation pass.
- Independent read-only QA: **No blocking findings**, including mandatory checks,
  legacy behavior, prior protection and new regression coverage.

The [original3/5 failing host reproduction](../acceptance-2026-11-13/routing-repro.json)
is preserved as pre-fix evidence at its original source. The new tests exercise
the fixed modern path; the old fixture's generic legacy call is not relabeled a
post-fix production-path test. No result above is actual Apple response/Sandbox
or physical-device coverage.

## Retained release boundaries

The [full G1–G6/D1–D4 assessment](../acceptance-2026-11-13/README.md) remains the
release/activation checklist; only B1's merge-blocking status is resolved.
Named regional Sandbox scenarios, older-OS runtime/mandatory-gap disposition,
parental launch revocation, #91 legal/public/retention/trigger decisions, #90
complete binary exclusion and #92/#21 final candidate/distribution acceptance
remain required before submission. No mandatory obligation is waived.

The shipping significant-change registry is still empty. Account-safe restart
approval durability, dormant legacy/account/rating handling and actual update
responses remain conditional activation gates. A current legal trigger would
promote those requirements to current release blockers.

The [owner iPhone17/iOS27.0.1 pass](../OWNER_REVIEW.md) and [November12 signed app](../development-signing-2026-11-12.json)
are preserved at source `b9793cc`. They are not extended to the changed native
source. Before submission, produce/reinspect the selected fixed candidate and
obtain appropriate targeted native/human evidence under the existing release
workflow. No new production export/archive, signing, installation, device control,
Sandbox session, merge or release was performed by this fix.

Next action: after final CI success, await the owner's explicit approval to merge
PR104 into develop. Source rollback is a revert of this focused fix; do not ship
a rollback that discards mandatory access protections.
