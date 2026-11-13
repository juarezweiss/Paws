# #103 / PR #104 final acceptance review — 2026-11-13

**Historical pre-fix assessment.** B1 is resolved at source `190693d`;
[current fix/reassessment](../b1-fix-2026-11-13/README.md) supersedes the merge
verdict below. Original reproduction/results are preserved. G1–G6 and conditional
D1–D4 retain their release/activation meaning; they are not marked complete.

**NOT READY for develop merge: one current implementation blocker (B1).**
Review head `734037ffb446f6523731ad8cd3abb4de10acafef`; runtime/source-linked
artifacts `b9793cc36ec5589ebbc538709c78a328b32ee13a`; develop
`a08e254444742bfaec6572f7552d4b1cbc06aceb`. Fetched references show no missing
develop commits. This assessment changes documentation/evidence only, and does
not authorize a merge, code fix, signing, installation or release.

BLOCKER means required before develop merge; RELEASE GATE means integration may
proceed after B1 is fixed, but submission remains prohibited until the gate is
satisfied. DEFERRED is conditional work that must precede its activation trigger.
None of these classifications turns an untested requirement into a pass.

## Accepted evidence and limits

- [Retained validation](../validation.json): pinned Unity6000.3.25f1 compilation,
  806/806 EditMode, 63/63 Python and 83 native workflow checks; actual SDK
  typechecks for ARM64 deployment16.3/26.0/26.2/26.4/27.0. This review checked the
  retained NUnit XML/checksum/count and Python/native log checksums. Runtime and
  scripts remain identical to the tested source; broad suites were not rerun.
- [Review-head CI](https://github.com/juarezweiss/Paws/actions/runs/43500112343)
  passed. Its only annotations are action-runtime deprecation and runner-image
  notices, not #103 failures. CI structure success does not establish native API behavior.
- [November12 signing receipt](../development-signing-2026-11-12.json): exact
  owner profile, app/profile XML/plist/DER age entitlement, existing certificate,
  strict app/framework signatures, weak loads and ARM64/dSYMs verified. Profile
  expires 2027-11-12T03:37:53Z; development signing is not App Store distribution.
- [Owner report](../OWNER_REVIEW.md): **iPhone17 / iOS27.0.1, all supplied human
  checklist items passed**. Preserve startup, gameplay/free offline Hints,
  recovery/lifecycle/controls/locales and owner-reported available age-case scope.
  Named Sandbox cases/regions were not enumerated. No numeric Sandbox count,
  other-OS pass, actual significant-update flow or launch-revocation pass is inferred.
- [Earlier actual controls](../native-verification-2026-11-02/verification-summary.json):
  unentitled isolated iOS17.0.1/18.5 bridge controls passed8/8,80/80 assertions;
  entitlement-bearing ad-hoc probes failed launch; unentitled27.0 control timed
  out. These remain separate from the later signed-candidate human pass.
- Independent read-only QA and native specialist reviewed current code. No new
  gameplay-rule regression, route bypass or commercial dependency was found.
  Access assemblies/native imports are standalone; source changes do not integrate
  IAP/AdMob/UMP/PR87. Complete final-binary exclusion remains G6.

## B1 — BLOCKER: feature-specific age request routing

`PawsNativeAccess.swift:111–130` derives `regulated = !features.isEmpty` on26.4+.
`PawsAccessWorkflow.swift:11–13` then ORs that into `needsRange`, reaching the
request at native line148. With no prior protection, no user action, the shipping
`changes=[]`, and only parental/adult **update** flags (no required-range flag),
ordinary startup still requests a range. This violates #103's minimal-request/no
unnecessary-prompt contract. A request may be cached; an actual sheet on an
identified regional device has **not** been reproduced.

Apple documents [distinct regulatory feature cases](https://developer.apple.com/documentation/declaredagerange/agerangeservice/regulatoryfeature)
and [skipping significant-change flows without an applicable requirement](https://developer.apple.com/documentation/declaredagerange/implementing-age-assurance-and-permissions).
No guarantee was found that update flags always include required range sharing.
The precise requirement must not be replaced by “any feature is present.”

[Focused host reproduction](routing-repro.json), using the unchanged committed
helper and [test-only harness](RoutingAcceptanceRepro.swift): **3 failures / 5
cases**, exit1. All three update-only combinations request a range unexpectedly;
empty-feature and required-range controls pass. This is a deterministic mapping/
workflow reproduction, **not an Apple response, actual prompt or Sandbox test**.
The initial compiler invocation did not run the test because its default SDK
could not load the standard library; an explicit already-installed MacOSX27.2 SDK
resolved that setup issue without downloads or toolchain changes.

Minimum next fix: on26.4+, route from required-range, prior protection, explicit
recheck and an actual pending applicable change; keep26.2–26.3 applicability logic
separate. Add the three no-request regressions plus required/recovery/real-update
controls. Run affected native checks/SDK compilation and independent re-review.
This review does not implement that fix or waive mandatory range checks.
Both independent reviewers identified B1; final read-only documentation QA found
no blocking inaccuracies in this evidence/classification report.

## Remaining RELEASE GATES

| ID | Unverified requirement / evidence | Required before submission; why integration can be separate |
| --- | --- | --- |
| G1 | Exact native Sandbox age, optional/mandatory sharing, correction, recovery and regional/account scenarios are not enumerated in the owner report. | Record named scenario, required/optional context, OS/candidate and pass/fail/unavailable, without personal IDs/raw responses. Cover applicable age categories and required-versus-optional behavior. Apple's [Sandbox procedure](https://developer.apple.com/documentation/storekit/testing-age-assurance-in-sandbox) supplies cases; its production validation recommendation is not satisfied by host tests. Missing evidence alone is not another demonstrated code defect. |
| G2 | Actual entitled Paws execution on exact16.3,26.0–26.1 and26.2–26.3 is absent. The human pass covers one26.4+ device only. | Complete supported-band/native applicability evidence and disposition of any demonstrated mandatory-check gap in #91/#92. Weak links, guards and SDK checks support integration, not runtime passes. [Apple's OS matrix](https://developer.apple.com/support/age-assurance/) distinguishes the capabilities and says existing18-and-earlier accounts are unaffected; that is not a blanket exemption. Retain16.3; no universal upgrade gate or age questionnaire. |
| G3 | Actual parental-consent launch revocation and restoration have no named platform/human evidence. | Validate supported launch-block/recovery behavior or obtain an explicit applicable-law/platform disposition before affected release. Apple [documents OS launch prevention](https://developer.apple.com/support/age-assurance/) while the Sandbox button documents a server notification only. Local-only Paws has no server access to revoke; there is no demonstrated app-side revocation API bug or justified speculative backend change. Reliance on platform enforcement is not recorded as tested. |
| G4 | Final territory/account applicability, reliable EU jurisdiction and under18 product-permission evidence outside exposed native signals remain #91 decisions. | Accept a dated global-launch assessment. Native applicability is not EU residence or parental approval; initial download permission remains SystemManaged/not exposed. The approved16EU/13elsewhere policy and Unknown handling are unchanged. [Apple](https://developer.apple.com/support/age-assurance/) retains developer responsibility; no full worldwide clearance is claimed by a develop merge. |
| G5 | Legal acceptance of minimal retained workflow, complete public Terms/Privacy/support and significant-change trigger assessment are unfinished. | #91 must settle deletion/retention rationale and publish/review reachable localized content; current AccessLocalization.cs38–41 provides local rules/help, not verified policy URLs. The [Texas statute §§121.053–.055](https://capitol.texas.gov/tlodocs/89R/billtext/html/SB02420F.htm) covers changes, verification and deletion. Storage contains neutral app-created phase/baseline/question IDs, no DOB/ID/age/raw response/answer/region; that technical fact is not final legal clearance. Confirm current1.0 is genuinely initial/no applicable prior significant change; an empty registry alone is not a legal determination. |
| G6 | Final #90 commercial-exclusion/privacy audit, #91 declarations/rating, #92 distribution candidate and final #21 readiness remain incomplete. | Audit the exact selected production binary and distribution profile, allocate owner-approved version/build and recheck applicable candidate behavior after any B1 code change. The old physical pass stays accepted for its original source; it is not copied to a changed artifact. Development signing is evidence, not distribution acceptance. These issues explicitly follow integrated native scope; they are separate release work. |

The dated legal baseline is not “Texas paused”: [Apple June3 notice](https://developer.apple.com/news/?id=sg176nne)
implements new-account Texas handling from June4,2026; the [Supreme Court docket](https://www.supremecourt.gov/search.aspx?filename=/docket/docketfiles/html/public/25a1389.html)
records July6 denial of the application to vacate the stay. No contrary later
order was identified in this review; recheck at submission. Current [Utah code](https://le.utah.gov/xcode/Title13/Chapter76/C13-76_2025050720250507.pdf)
starts developer duties May6,2027. Louisiana's [official enacted-law digest](https://legis.la.gov/Legis/ViewDocument.aspx?d=1486866)
records replacement of Act481 and relevant provisions from July1,2027; the full
Act185 endpoint timed out in this review, so do not call it a new full-text audit.
Brazil/other launch jurisdictions still require final G4 assessment rather than
an invented universal18+ restriction. No hardcoded territorial exclusions are added.

For submission on/after November16, #91 must also consider Apple's [new rating
descriptor declarations](https://developer.apple.com/news/?id=yxkh08us) against the
actual candidate. No account, questionnaire or store settings were changed.

## DEFERRED — conditional activation gates, not completed criteria

| ID | Work / current reason | Must resolve before |
| --- | --- | --- |
| D1 | Account-safe durable significant-update approval/no-repeat across process restart is incomplete. Handled approvals are session-only; app question/change identity persists and restart fails protectively rather than inheriting another account's approval. Native46 has an empty registry, so no actual update can currently enter the flow. | First legally significant update or nonempty registry. Establish account-scoped/idempotent approval handling and native pending/denied/approved/restart/account-change evidence. If G5 finds a current trigger, promote this to a current release blocker; do not ship it as deferred. |
| D2 | Future26.2–26.3 update logic native160–162 additionally relies on the legacy parental-control flag. Current [Apple guidance](https://developer.apple.com/documentation/declaredagerange/requesting-people-share-their-age-range-with-your-app) directs eligibility-plus-age handling instead. Full parent/adult/update/error paths lack actual native evidence while no change is configured. | Enabling a real update on those OS bands; correct the legacy dependency and validate real permission responses. Also verify account invalidation: NSUbiquityIdentityDidChange is not documented as a universal Apple Account signal. These are dormant flow gaps, not current1.0 permission passes. |
| D3 | Automatic rating-change trigger detection is absent; [AppStore.ageRatingCode](https://developer.apple.com/documentation/storekit/appstore/ageratingcode) supports comparing prior/current ratings. Current source has no configured update and no prior published cohort is established by this review. | A real rating/content/Terms/privacy change requiring renewed consent. G5 must confirm initial-release status before deferring. Native flags alone do not identify the app's meaningful change. No IAP integration is implied. |
| D4 | Server notification backend and monetization integrations are outside this local-only free candidate. | Reassess notification handling if server-side access appears or G3/G4 establishes a current necessity; future monetization stays #67/#82/PR87 and must never become a prerequisite here. No backend/service, paid resource or fake significant update is introduced. |

The earlier ad-hoc harness launch failures/unentitled27.0 timeout are diagnostic
failures, not demonstrated signed-Paws defects. Further investigation is DEFERRED
unless reproduced on an appropriately entitled candidate. The later27.0.1 human
startup pass remains scoped; it does not turn the old failed controls into passes.

D1/D2 keep #103's original significant-update acceptance criterion incomplete;
this is an explicit activation restriction, not silent checkbox completion.

## Acceptance disposition and next action

#103 standalone capability, deterministic age/permission policy, recovery routes,
privacy-minimal technical storage, availability/weak links and development signing
have evidence. Safe/proportionate native requesting fails B1. Complete scope,
actual regional/older-OS/revocation, public/legal and final-artifact criteria retain
G1–G6; significant-update completeness retains D1–D3. No original unchecked
criterion is silently checked off and #103 is not closed.

1. Resolve only B1 in PR104, add regressions and re-review the changed runtime.
2. After passing review/checks, seek the owner's explicit develop-merge approval.
3. Before submission complete G1–G6 with exact-candidate evidence; keep D1–D4
   dormant until their stated triggers are satisfied. No automatic production action.
