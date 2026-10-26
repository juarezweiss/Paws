# Paws age assurance — #67 / PR #87

Owner decision: 2026-10-26, superseding the October 24 custom age-band selector.
Production activation, account/capability changes, submission and release remain gated.
This is a bounded technical assessment of primary sources, not a certification of
compliance in every jurisdiction. Recheck official requirements before distribution.

## Experience and policy

Normal launch shows the existing menu and Play. There is no custom age question,
first-launch age modal, birthday field or saved self-declared adult preference.
Enabled builds display a small, nonmodal Brazil A12 / advertising / in-app-purchase
disclosure at access. The actual App Store questionnaire must support that rating;
if Apple's result differs, reconcile this disclosure before shipping. A rating never
establishes the player's age, consent or purchase ownership.

Apple's regional requirement check runs without a Paws modal. A native age-sharing
request runs only when Apple reports it required, or after an explicit supported
Settings > Age range from Apple action. Required requests wait for an unobstructed,
foreground menu/settings surface; Play can proceed while ad eligibility is unresolved.
There is no timer-based re-prompt after decline. Unsupported/unavailable/declined,
malformed and unconfirmed adult results remain Unknown, with no Google bridge construction, ad initialization or ad requests and free eligible Hints. Minors receive the same no-ad protection.
IAP and Restore remain available; age never grants ownership.

Only a coherent Apple range with lower bound at least 18 and an Apple-confirmed
method unlocks the adult ad path. Missing upper bound alone, regional eligibility,
A12, prior consent and the old local choice are insufficient. Payment/ID/other
checked methods (including guardian checked methods) and Apple's newer `confirmed`
case qualify; self/guardian declaration alone does not. Raw numeric bounds stay
inside the native adapter. No birthday, age log, developer-server age record,
analytics identity or persistent age band is introduced. The former local preference
key is ignored and cannot authorize ads.

The result lasts only for the current foreground session. Actual backgrounding
invalidates it, including an in-flight result; a potential native sheet retains its
input guard until its completion. Focus changes from native sheets do not invalidate
it. After real foreground return, regional requirements are checked again. Optional
regions require explicit sharing again; this deliberately reduces ad reach rather
than trusting stale age/account information. There is no silent-age API guarantee.

## Apple research and OS behavior

[Apple's request guide](https://developer.apple.com/documentation/declaredagerange/requesting-people-share-their-age-range-with-your-app)
documents a native permission sheet, parental sharing controls, regional overrides
of requested thresholds and the required capability. Paws requests 13 and 18; it
handles returned bounds rather than assuming those exact intervals. Native policy
tests include Apple's [Sandbox scenarios](https://developer.apple.com/documentation/storekit/testing-age-assurance-in-sandbox),
whose examples use inclusive upper bounds 12/15/17. Ambiguous intervals stay restricted.
Never infer adulthood from the absence of an upper bound.

| iOS | Paws behavior |
| --- | --- |
| 16.3–18.x | Framework unavailable: no call, no age UI, ads off, free eligible Hints. |
| 26.0–26.1 | Base age API exists, but the regional preflight used here is unavailable; same conservative fallback, no optional row. |
| 26.2–26.3 | `isEligibleForAgeFeatures` determines regional requirement; request only when required or explicitly requested in Settings. |
| 26.4+ | `requiredRegulatoryFeatures` / `declaredAgeRangeRequired` determines requirement; other outcomes remain optional, not adult. |
| 26.5+ / 27.x | Also recognize the newer `confirmed` declaration; unknown future values fail closed. |

These availability boundaries are supported by [Apple's Q&A](https://developer.apple.com/br/support/age-assurance/)
and the installed Xcode 27.1 / iOS 27.1 SDK interfaces. API availability and Swift
ARM64 typechecking do **not** constitute execution on each OS. The minimum remains
16.3; the framework is weak-linked. No country, language, IP address or timezone is
used to infer age or hardcode a regulatory exemption.

The [license agreement, section 3.3.3(O)](https://developer.apple.com/support/terms/apple-developer-program-license-agreement/)
limits age requests to age-appropriate features or legal obligations. Paws uses the
result to suppress age-inappropriate advertising, not for profiling, marketing
segments, sale or age-based targeting. Only confirmed adults reach Google's generic
Unspecified treatment; raw Apple age information is never passed to Google.

[Declared Age Range entitlement](https://developer.apple.com/documentation/bundleresources/entitlements/com.apple.developer.declared-age-range)
and a compatible explicit Paws provisioning profile are required. Export adds the
app entitlement and weak framework link; signed archive inspection checks both the
actual signature and embedded profile. The existing local development profile
`Builds/Issue67-LocalValidation/Paws.app/embedded.mobileprovision` was decoded read-only:
capability absent; expires 2026-10-27 17:36:44 UTC. No account/profile was changed.
The older signed copy and October 24 captures are historical, not this flow's evidence.

Significant app changes are separate from age sharing. This preparation assumes
Paws 1.0 is the initial store release; before submission the owner must verify that
assumption and assess any required parental permission/adult acknowledgment. If an
existing published app is being materially changed, required PermissionKit/system
acknowledgment and consent-revocation handling must be completed before distributing
that change. The new regional preflight is not a waiver or implementation of those
flows. A12 alone does not exempt an app. Apple's platform download/IAP parental
approval remains authoritative; a native age result never substitutes for it.

## Google treatment and privacy

The pinned Google Unity 11.6.0 / GMA 13.11.0 support
[Child / Teen / Unspecified](https://developers.google.com/admob/unity/targeting).
The binding maps reliable under-13 / teen / adult bands respectively, and rejects
Unknown configuration. Paws 1.0 deliberately blocks all minor/unknown requests, so
Child/Teen mappings do not enable advertising to those players. Unspecified means
no special age treatment; it is used only after the separate confirmed-adult gate.
[Google's treatment definitions](https://support.google.com/admob/answer/6219315?hl=en)
are SDK controls, not proof of age or full legal compliance.

UMP still updates before Google initialization, displays required consent, supports
Privacy Options and gates loads/shows through `CanRequestAds`. NPA does not remove
consent duties. [Google documents](https://developers.google.com/admob/unity/privacy/gdpr)
that UMP does not itself determine age or forward its under-age flag to ad requests.
Suppressed cold launches make no UMP call and never ask a minor for initial adult
ad consent. An already-required Privacy Options entry remains usable after age
invalidation: explicit modification uses only the retained UMP form, with no consent
update, ad initialization or load. Re-sharing adult age cannot restart ads under
that native surface. [Google's privacy-options requirement](https://developers.google.com/admob/ios/privacy#privacy_options)
is independent of ad eligibility. Cold unresolved launches have no current UMP
Required status; Paws does not fabricate a consent cache or start Google to discover
one. Public privacy/contact rights remain necessary; validate this cold-state
withdrawal path and published contact with the owner before activating production. Approved NPA, no ATT/cross-app tracking, FPID off, crash reports off,
G content and no added analytics/attribution/telemetry SDKs are retained. Consent
failure/unavailability grants eligible free Hints; actual voluntary ad closure
without reward still grants none. Existing SDK data disclosures remain necessary.

## Brazil research and genuine release checks

[Law 15.211/2025](https://www.planalto.gov.br/ccivil_03/_ato2023-2026/2025/lei/l15211.htm)
applies to likely child/teen access, not just Kids Category products. Articles 8(V),
10–14 and 22 concern access-time classification information, age-appropriate
experiences/receipt of platform signals, limited age-data use and the ban on profiling
for child/teen commercial targeting. Article 9's strict each-access verification
addresses prohibited adult content; Paws has no such content, chat or loot boxes.
Our interpretation is that these provisions do not mandate a universal custom age
question before this all-ages puzzle gameplay. This is not an exemption from the
remaining safety, transparency, parental-support and privacy obligations.

[Decree 12.880/2026](https://www.planalto.gov.br/ccivil_03/_ato2023-2026/2026/decreto/d12880.htm)
requires proportionality, minimization, appropriate age signals, more protective
handling on conflicts and prevention of prohibited profiling (arts.24–27,33).
Suppressing unresolved/minor advertising is the conservative implementation;
blocking ordinary puzzle play or collecting birth dates is not added as a substitute.
The published privacy/support pages must explain age processing, safe defaults,
Apple Screen Time/family tools, correction through Apple Account > Personal
Information > Age Range for Apps, and a usable privacy/support contact.

ANPD's [implementation page](https://www.gov.br/anpd/pt-br/assuntos/eca-digital) and
[September progress report](https://www.gov.br/anpd/pt-br/assuntos/noticias/eca-digital-completa-um-ano-e-e-marco-na-protecao-de-criancas-e-adolescentes-na-internet)
show ongoing guidance/regulation work and staged monitoring. The schedule is not
permission to ignore the effective law; other enforcement remains possible.
No final universal Paws exemption was established. Before Brazil activation, owner
review must confirm current ANPD guidance, the app's classification/disclosures,
platform-signal adequacy on older OS versions, and public privacy/parental-support
information. If that assessment finds a mandatory pre-play restriction for the
actual product/territory, treat it as a precise release blocker for that territory;
do not silently replace it with a custom birthday/age popup.

## Human validation addendum

Record exact SHA/build/config/device/iOS/scenario and PASS/FAIL/NOT TESTED in #67.
No automated physical-iPhone interaction. On representative supported systems:

1. Older iOS / unavailable capability / offline preflight: open menu and Play without
   custom/native age popup; free eligible Hints, IAP/Restore accessible; no Google.
2. Supported optional region: no launch age request. Explicit Settings native sharing;
   decline/unconfirmed adult => no ads; confirmed adult => required UMP before test
   ads. Repeat background/account/sharing changes: no stale adult authorization; an already-required Privacy Options entry still permits modification without resharing age or restarting ads. Verify the cold unresolved-state privacy/contact path separately.
3. Apple age-assurance Sandbox regulated scenarios (including Brazil and applicable
   US account regions): required native flow at safe menu, child/teen/unconfirmed/
   confirmed adult, refused/unavailable outcomes, region-overridden bounds and late
   completion after background. Verify no gameplay input under a real native sheet.
4. Exact build: restore/paid Sandbox ownership and suppression plus test-ad consent,
   privacy, cadence, rewarded completion/early close and lifecycle checks from
   [RELEASE_RC_1_0.md](RELEASE_RC_1_0.md). Apple age-assurance Sandbox scenarios,
   priced-product Sandbox purchases and Xcode StoreKit Test simulated Ask to Buy
   are distinct evidence; none may be relabeled as another environment's approval.

Production IDs/public URLs/UMP publication, compatible signing capability, actual
OS/native/regional behavior and exact-candidate human tests remain release gates.
No distribution, live ad activation, submission or account changes are authorized.

## Current preparation evidence — 2026-10-26

[Exact results](evidence/issue67/native-age/validation.json): source `c0622a3`,
727/727 EditMode, 86/86 scripts, pinned/production-only managed compilation,
19 age + 11 IAP Swift assertions, ARM64 typecheck, 322 SDK pins, CI and bounded
independent QA re-review pass. Both exact-source unsigned native archives pass:
[enabled real-IAP/test-ad](evidence/issue67/native-age/enabled-ios-release-receipt.json)
and [disabled rollback](evidence/issue67/native-age/disabled-ios-release-receipt.json).
Actual weak linkage/provider symbols, bundled runtime config and dSYMs are checked;
rollback excludes age and Google providers. Nine desktop menu renders fit all
supported locales/aspects. None establishes physical/native Apple age behavior.
[Corrected findings](evidence/issue67/native-age/failures-corrected.json) preserve
scope and verification. [Profile audit](evidence/issue67/native-age/profile-preflight.json)
and [production preflight](evidence/issue67/native-age/production-preflight.json)
record actual remaining configuration/signing gates without sensitive identities.
No newer signed app was produced by stripping the required capability.
