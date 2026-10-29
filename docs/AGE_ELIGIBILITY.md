# Age eligibility and advertising policy

## Free-release ownership clarification — 2026-10-28

The October 27 approved minimum/permission policy below is unchanged; this update
makes no new legal determination. #91 owns free-1.0 territory/access assessment,
required signals/native handling, public Terms/Privacy/support and the recorded
owner disposition. The [release scope matrix](RELEASE_RC_1_0.md#90--91-age-and-access-scope-reconciliation)
separates unconditional monetization exclusion from unresolved standalone access.
#90 excludes **monetization-only** age components; any required standalone access
component must be precisely scoped, separately authorized and verified, with no
monetization SDK/provider pulled into the free binary. Necessity/implementation
is not established by these docs. Final #90/#92 acceptance waits for that scope;
final #91 metadata uses actual artifact evidence and is accepted before #21.

The later #67 implementation/test list describes future monetization. It does
not defer any applicable free-1.0 access requirement until 1.1+. Known-ineligible
restrictions/correction must be addressed by the free-release #91 disposition;
Unknown is not Adult, and unresolved jurisdiction alone is not known ineligibility.
Normal startup has no custom questionnaire. No gameplay implementation or testing
is performed by this documentation reconciliation.

Owner approval: 2026-10-27. Traceability: [#91 store/compliance](https://github.com/juarezweiss/Paws/issues/91)
and [#67 future monetization](https://github.com/juarezweiss/Paws/issues/67).
This is the approved policy and implementation contract, **not an implemented or
legally cleared candidate**. Current free-1.0 scope remains unchanged. PR #87 and
its evidence remain preserved; its universal confirmed-adult advertising rule is
superseded as a future requirement, not silently fixed in its existing source.

## Approved product rules

- Minimum age is 16 within the European Union and 13 elsewhere. Non-EU EEA
  countries are outside the EU for this contractual rule; their data-consent laws
  still apply independently. Players under 18 require parental/guardian permission.
- Reliable information establishing an age below the applicable minimum restricts
  game access. There is **no free-play exception** for known ineligible users.
  Permission from a parent does not override this minimum.
- Ordinary startup remains Menu → Play. No custom age questionnaire, birthday,
  identity-document collection or universal proof of adulthood. Only request age
  information when necessary; use Apple's required native mechanisms where
  applicable. Mandatory native age/parental controls remain genuine exceptions.
- Unknown age is not Adult or proven eligibility. Terms acceptance, installation,
  App Store rating, purchase history and UMP consent are not age verification.
  Unknown age alone does not create a universal gameplay restriction; applicable
  legal access or parental-permission obligations may require one.
- Parental permission under these Terms is distinct from verifiable parental
  data consent, permission for a significant update, and consent for advertising.
  Never infer any of these from another. A known denial/revocation of a required
  access permission restricts access; use required native permission handling.
- Terms, Privacy Policy and support remain accessible in Settings and through
  public HTTPS URLs. A restricted user must retain a limited information/recovery
  surface for these documents, native recheck and support, without puzzle access,
  advertising or new purchase offers. Existing-owner purchase recovery must remain
  available where platform/legal rules permit; restriction must not erase ownership.

### Terms eligibility clause for publication review

> You may use Paws only if you are at least 16 years old in the European Union,
> or at least 13 years old elsewhere. If you are under 18, you must also have
> permission from your parent or legal guardian. Parental permission does not
> allow use below the applicable minimum age. Where reliable age information
> establishes that you are below that minimum, access to gameplay is restricted.

This clause records the approved eligibility decision, not a complete EULA or
Privacy Policy. Public URL, publisher/contact details, localized legal copy,
applicable-law review and publication remain #91 gates. No public URL is invented
or claimed published. Do not add an assent screen as a substitute for age signals.

## App Store rating reconciliation

Apple requires an override when a EULA minimum exceeds the calculated rating;
the override applies across regions, with local rating values. Therefore prepare
**a global 16+ override as the minimum target**, or a higher calculated rating if
the truthful questionnaire requires it. This is the conservative mapping of the
approved EU minimum, pending verification in App Store Connect. A region-specific
13+/16+ override is not assumed available. [Apple rating setup](https://developer.apple.com/help/app-store-connect/manage-app-information/set-an-app-age-rating/).

The global override may limit discovery/downloads for 13–15-year-olds outside
the EU; it does not change their contractual eligibility into 16+ or 18+.
Do not retain A12 as an approved target, invent mature/suggestive content, or
declare advertising/IAP in free 1.0 merely to imitate Arrows. Answer from the
actual candidate. Not intended for the Kids Category.

Before submission, record calculated rating, selected override, country-specific
values (including Brazil), the **Operating Systems Earlier than Version 26**
panel, questionnaire and any age-suitability URL in #91. Older OS values can
differ; do not promise a specific legacy mapping without checking the panel.
[Apple rating values](https://developer.apple.com/help/app-store-connect/reference/app-information/age-ratings-values-and-definitions).

The rating is neither individual age evidence nor complete access enforcement:
Apple permits parental download exceptions. Required Declared Age Range checks
still apply even to highly rated apps. [Apple age-assurance Q&A](https://developer.apple.com/support/age-assurance/).

## Separate access, age, consent and commerce decisions

Future implementation must keep the following independent:

| Decision/input | Required behavior |
| --- | --- |
| Age range and reliability | Use coarse boundaries 13, 16, 18, source/method and freshness; no birthday. A returned interval must establish the threshold, not guess it. A range straddling a minimum is unresolved, not proof of ineligibility/adulthood. |
| Applicable jurisdiction | Use a documented defensible source; EU membership is not EEA membership. Storefront, language, timezone and UMP `NotRequired` do not establish residence/applicable law. Resolve conflicting obligations protectively. |
| Known below minimum | Deny puzzle entry/resume, Hints and new commerce; preserve information/recovery access. Recheck when reliable information changes. Never silently allow a known child after restart. |
| Unresolved jurisdiction, known under 13 | Ineligible everywhere under approved Terms. |
| Unresolved jurisdiction, known 13–15 | Keep affected ads unavailable. Do not infer EU residence or restrict gameplay solely from unresolved jurisdiction: restrict only when reliable evidence establishes below-minimum status under the applicable rule, or a documented mandatory access obligation requires it. No custom age/region questionnaire. |
| Legal age/permission obligation | Perform required native check; honor denial, pending required permission, significant-update restrictions and revocation. An unresolved mandatory access check is not merely an ad failure. |
| Advertising policy unresolved | No affected SDK initialization/load/show; otherwise eligible gameplay continues with free eligible Hints. This does not override an access restriction. |
| Consent | UMP update and required form/privacy-options handling remain independent. `CanRequestAds`, `Obtained` or `NotRequired` do not prove age. No NPA request bypasses necessary storage/data consent. |
| Ownership | Only verified Apple entitlement grants Remove Ads. Age/Terms/consent never grants, revokes or deletes purchase ownership; owned users suppress both ad placements. |

Do not discard protective knowledge when a refresh returns Unknown, fails,
backgrounds or restarts. Retain only the minimum necessary protective restriction
and provenance locally, with disclosure and a native correction/recheck path.
Relax only with sufficiently reliable superseding information; do not guess a
birthday, permanently freeze a restriction or use age data for targeting.

## Advertising matrix for future enablement

This preserves the investigation's jurisdiction-specific policy. **Candidate**
means a potentially supportable route after the listed assessment/consent/provider
gates, not a production clearance. **Hold** means ads unavailable pending that
specific work, not a statutory claim that everyone must prove adulthood. All rows
first apply the approved access minimum and parental-access requirements above.

| Jurisdiction | Reliably eligible teen | Unknown age |
| --- | --- | --- |
| United States, general-audience baseline | Candidate protected Teen NPA; state overlays still apply. | Candidate NPA under an assessed genuinely general-audience profile without actual child knowledge. Keep age Unknown, never Adult. COPPA does not generally require investigating every general-audience user, but Terms 13+ alone do not establish audience classification. |
| California / other applicable state privacy rules | Candidate after minor sale/sharing, targeted-ad and consent controls; NPA alone is insufficient. | Conditional on assessed state profile, applicable opt-outs/GPP/RDP and required platform signals; otherwise hold. |
| Texas / Utah / Louisiana / other required platform-age cases | Candidate only after applicable native age/parental-access obligations. | Hold affected advertising pending required signals/permissions; block access only when the actual access obligation requires it. Recheck current effective requirements per territory. |
| EU | 16–17 candidate protected Teen NPA with valid necessary consent. Known under 16 cannot play. | Standard NPA hold pending consent capacity or an assessed lawful protective alternative; no blanket 18+ rule. |
| Non-EU EEA | 13–17 may meet Terms, but national digital-consent ages/capacity can require stronger protection or parental consent. | Hold standard NPA pending capacity/protective lawful alternative. EU contractual 16 is not automatically imported. |
| UK | Candidate with valid consent and Children's Code protections. | All-users protective standards may avoid individual verification, but actual SDK processing and consent capacity must be assessed; hold until demonstrated. |
| Switzerland | Candidate after applicable capacity/consent and Google's Swiss CMP requirements. | Hold pending capacity/protective assessment; not automatically EU contractual 16. |
| Brazil | 13–17 candidate only with lawful processing, best interests, required age signals and no prohibited profiling for advertising. | Hold pending an adequate age/protective approach. `npa=1` alone is not proof that all processing avoids profiling. |
| Canada, including Quebec | Candidate meaningful youth consent; Quebec under-14 requirements need separate assessment even though Terms start at 13. | Hold pending adequate capacity/parental-consent or protective approach. |
| Australia | Candidate after privacy/consent-capacity and applicable current child protections. | Hold pending assessment; do not mistake draft children's-code requirements or social-media rules for a universal puzzle-game adult gate. |
| Singapore | Candidate when capable of understanding consent; parental handling where required. | Hold pending adequate capacity/protective assessment. |
| Unassessed / unresolved territory | Hold pending a documented profile. | Hold; otherwise eligible gameplay/free Hints continue. |

Google supports Child, Teen and Unspecified treatments. Apply treatment based on
reliable age and applicable under-consent protections, not Terms acceptance. An
eligible chronological teen can still need child/under-consent treatment under
the relevant law. Known below-minimum players receive no ads because gameplay is
restricted. An approved Unknown profile uses an explicitly assessed treatment,
not an Adult label. [Google age treatment](https://support.google.com/admob/answer/6219315?hl=en).

NPA still uses identifiers/storage for reporting/capping and can require consent.
UMP's under-age setting and GMA request treatment require separate configuration.
Limited Ads is a different mode and is not implicitly approved here.
[Google NPA](https://support.google.com/admob/answer/7676680?hl=en),
[UMP requirements](https://developers.google.com/admob/ios/privacy/gdpr).

Keep NPA only, no ATT/cross-app tracking, first-party ID and SDK crash reporting
disabled, G-rated ad content, no additional analytics/attribution/telemetry SDKs.
Ad availability must not gate eligible gameplay; free-Hint fallback remains.

MAX/Usercentrics remains a separate provider evaluation, not a replacement chosen
to bypass this policy. MAX currently prohibits initialization/use for a legal
child; the definition varies by jurisdiction. A CMP does not establish age or
remove publisher obligations. No SDK may bootstrap before its initialization
conditions hold. [AppLovin publisher terms](https://legal.applovin.com/policies-publishers/).

## Apple OS behavior and future implementation work

| OS | Required handling |
| --- | --- |
| Supported iOS before 26 | No Declared Age Range API; keep age Unknown absent reliable existing evidence. Apply the jurisdiction profile, not universal ad-off merely because adult verification is unavailable. Missing legally mandatory assurance needs a territory-specific resolution; do not invent a questionnaire. |
| 26.0–26.1 | Native range API available; later regulatory preflight is unavailable. No unnecessary first-launch request; document the capability gap for applicable obligations. |
| 26.2–26.3 | Use regional age-feature eligibility and returned range/source with availability guards; required native flows may interrupt. |
| 26.4+ | Evaluate the full `requiredRegulatoryFeatures`, including required range checks and significant-update/parental acknowledgement, rather than reducing it to one Boolean. |

Implementation starts from latest fetched develop and preserves newer game/assets/
ProjectSettings. PR #87 evidence head `4acfb61d9625b271f3203ec8d63a608102fbabfa`
is historical. Its `AudienceAdService`, `GoogleAdsBridge` constructor/configuration,
coordinator Hint branch and shell labels all assume Adult; change these together
behind one independently testable access/ad policy. Extend its coarse age model
to distinguish 13–15 from 16–17, preserve reliability and protective knowledge,
and add the Terms URL. Do not revive the custom questionnaire or simulated age.

The free 1.0 binary exclusion gate must exclude **monetization-only** age code;
#91 must first determine whether this approved access policy or regional law
requires a standalone native age component in free 1.0. Such a prerequisite must
be recorded explicitly and reconciled with #90, rather than silently stripping
required assurance or shipping monetization providers. Free pricing is no exemption.

## Pending evidence and release boundaries

- #91: reviewed/localized Terms and public links, selected-territory legal/access
  assessment and defensible regional policy, required native handling for free
  1.0, truthful questionnaire/global override/local/legacy values. No store edits
  or publication performed by this policy document.
- #67: future implementation and tests for known-below-minimum entry/resume,
  no free-Hint bypass, ranges crossing thresholds, EU/non-EU/jurisdiction changes,
  known minor → Unknown/error/restart, consent withdrawal, permissions/revocation,
  and SDK-init/load/show spies. Unknown US candidate and unresolved-region hold
  must be separate tests; include eligible teens and paid-owner recovery.
- Native/physical: availability guards on older OS, native required/optional/
  declined/error responses, 26.2+ Sandbox regional/age/permission scenarios and
  26.4+ significant updates. Physical interaction/lifecycle review is human-only;
  simulated scenarios do not prove real-user age or actual parental approval.
- IAP/Restore/verified ownership/finalization, ad suppression/privacy, SDK-free
  rollback, exact-source compilation/archive integrity and signed-candidate QA
  remain separate future enablement gates. #82 stays open with its narrowly
  accepted pending-button restart issue; paid-entitlement safeguards are not waived.
- No main/develop merge, production activation, live-ad request, account/console
  change, purchase, publication, upload, submission or release is authorized.

## Investigation sources and limits

Official sources checked during the 2026-10-26/27 investigation. Revalidate when
implementing/selecting territories; unresolved matrix rows are intentionally not
represented as final legal/provider clearance.

- [FTC general-audience COPPA guidance](https://www.ftc.gov/business-guidance/resources/complying-coppa-frequently-asked-questions)
  and [California privacy rules](https://www.oag.ca.gov/privacy/ccpa).
- [GDPR Article 8](https://eur-lex.europa.eu/eli/reg/2016/679/),
  [UK proportionate assurance/all-users protection](https://ico.org.uk/for-organisations/uk-gdpr-guidance-and-resources/childrens-information/childrens-code-guidance-and-resources/age-appropriate-design-a-code-of-practice-for-online-services/3-age-appropriate-application/).
- [Brazil ECA Digital, including Articles 13, 14 and 22](https://www.planalto.gov.br/ccivil_03/_ato2023-2026/2025/lei/l15211.htm),
  [implementing decree](https://www.planalto.gov.br/ccivil_03/_ato2023-2026/2026/decreto/d12880.htm).
- [Canada meaningful consent](https://www.priv.gc.ca/en/privacy-topics/privacy-for-businesses/appropriate-handling-of-personal-information/collecting-personal-information-and-consent/consent/gl_omc_201805/),
  [Quebec Law 25](https://www.cai.gouv.qc.ca/protection-renseignements-personnels/sujets-et-domaines-dinteret/principaux-changements-loi-25),
  [Australia children's code status](https://www.oaic.gov.au/privacy/privacy-registers/privacy-codes/childrens-online-privacy-code),
  [Singapore children's data guidance](https://www.pdpc.gov.sg/-/media/files/pdpc/pdf-files/advisory-guidelines/advisory-guidelines-on-the-pdpa-for-children%27s-personal-data-in-the-digital-environment_mar24.pdf).

The owner-supplied Arrows 0.23.1 Android evidence describes inspected code paths,
not iOS behavior, dynamic consent/waterfall configuration, actual execution or
compliance. It supplies no instruction or assurance for Paws. Its Terms were a
reference; Paws' rules above derive from the owner's explicit approval.
