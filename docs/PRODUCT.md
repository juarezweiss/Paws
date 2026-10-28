# Product Definition

## Approved 1.0 release scope — 2026-10-26 (#88)

Paws 1.0 is FREE, ad-free, IAP disabled, with unlimited free eligible Hints.
Ship no purchase/Restore/Offer Code UI, ads, UMP or unnecessary age prompts;
exclude monetization providers/SDKs from the binary while retaining implemented
source for separately approved 1.1+ work (#67/#82, preserved PR #87).
The [release checklist](RELEASE_RC_1_0.md) governs the six focused delivery issues.
This owner decision changes launch scope; it does not change puzzle/Hint eligibility,
authorize implementation or waive trial, physical review or production approvals.

## Product promise
Paws is a calm, compact mobile logic puzzle: infer where all hidden paws are,
mark impossibilities, and finish a board without time pressure or punishment.

## Experience pillars
1. **Pure deduction** — every shipped/generated board is solvable without the Hint button.
2. **Calm interaction** — no timers, lives, failure animations, or penalties for experimentation.
3. **Fast tactile play** — portrait-first sessions, tap cycling, and drag-to-mark for repetitive eliminations.
4. **Readable logic** — clues are aligned with the grid and visually distinct without relying on color alone.
5. **Infinite replayability** — seeded procedural boards can produce effectively unbounded puzzles within validated difficulty bands.

## Localization readiness (Issue #3)

Initial languages are English (`en`), Brazilian Portuguese (`pt-BR`), and Spanish
(`es`). Player-facing text uses stable resource keys, with English as the fallback
for unsupported locales or missing translations. Additional locale resources must
not require changes to puzzle logic. Support explicit language selection and
device-language detection, locale-aware formatting, accented characters, and
layouts that accommodate longer translations as UI is built.

Structured deductions can be rendered in the selected language. The current Hint
flow applies one guaranteed fact immediately without explanation UI (#23);
localization still preserves stable deduction resources for diagnostics or a
separately approved future presentation. Regional Spanish variants can be added later.

## Initial audience
Mobile puzzle players who enjoy Picross/Nonograms, Minesweeper-like deduction, and low-pressure daily/short-session games.

Owner-approved eligibility (2026-10-27): minimum 16 in the EU and 13 elsewhere;
under-18s require parental/guardian permission. Reliably known below-minimum users
cannot access gameplay, including free play. No custom age questionnaire or
birthday collection; Unknown is never verified Adult. Ordinary startup remains
Menu → Play, subject to legally required native handling. See
[age/access/advertising policy and rating reconciliation](AGE_ELIGIBILITY.md).
This approval does not enable monetization or establish implementation acceptance.

## Approved master plan
[Master Project Plan](../planning/MASTER_PROJECT_PLAN.md) governs scope, acceptance
and budget. Version 1.0 was approved 2026-10-08; the current Paws reconciliation
is tracked in #61. Every purchase or subscription needs separate explicit owner approval.

## MVP
- iOS-first; owner playtest and small tester trial before public release. TestFlight
  enrollment and distribution retain their separate approval gates.
- Easy 5×5, Medium 6×6 and Hard 7×7 ordinary play. New ordinary players start
  procedural Easy → Easy → Medium, then use the existing random policy and
  persistent two-repeat limit (#96/PR #98); legacy ordinary histories bypass the
  introduction. Every board retains independent uniqueness/no-guess admission.
- Fixed Easy first-play tutorial and Settings replay. Completion/Skip enters the
  same ordinary progression; tutorial wins/replays do not advance/reset its starts.
  #95/PR #97 clarity/centering/delayed guidance and #99/PR #100 wording are merged.
- Approved pale-sage arrow cells, original dark arrows and 1.10× stronger-ink paw
  artwork (#94/PR #101), with unchanged interaction geometry and puzzle rules.
- Procedural seeded board generation.
- Row and column paw-count clues.
- Eight-direction arrow clues embedded in cells, including exact 45-degree diagonals.
- Empty/X/Paw player marking.
- Horizontal/vertical drag-to-X across eligible empty cells.
- Clear, New Game, Menu, Hint.
- Immediate victory detection.
- Timed ordinary-board celebration and return to the same locked finished board.
- Hold-to-highlight arrow paths and non-moving Hint attention after board inactivity.
- English, Brazilian Portuguese and Spanish; language preference and shape-based readability.
- Current Paws name/paw artwork, guided tutorial and unlimited clue-derived hints
  while monetization remains disabled.

Start the first tester session with the fixed Easy tutorial; fresh ordinary
starts follow Easy → Easy → Medium → random, using current generators rather
than authored boards. Returning-player history follows the #96 migration contract. The approved #35 pilot limits
Hard lines to 1–3 paws and prunes redundant
Medium/Hard arrows with both solvability proofs intact; density ranges remain
soft targets. Current profile rules live in [GAME_DESIGN.md](GAME_DESIGN.md); validation
and physical-device evidence live in [TESTING.md](TESTING.md). Keep the first trial
focused on player understanding, comfortable input and concrete feedback.

## Release status — 2026-10-28

Develop `3cb814f` includes the completed work above and its scoped owner review
receipts. [Release ledger](RELEASE_RC_1_0.md) distinguishes those passes from
pending #89 UI cleanup, #90 final binary exclusion, parallel #91 public content/
access/compliance, #92 signed RC/human QA and #21 distribution readiness.
Two #22 sessions are recorded; trial sufficiency/residual coverage remains an
owner decision, without reopening completed gameplay or assuming a new tester quota.

## Not MVP
- Accounts/cloud save.
- Multiplayer.
- Competitive leaderboards.
- Live ops backend.
- Production advertising.
- Paid user acquisition.
- Calibrated difficulty scoring, persistent board resume, analytics SDKs and
  extensive polish before player feedback.
