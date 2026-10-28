# Issue #99 — tutorial wording

Follow-up to [#95](https://github.com/juarezweiss/Paws/issues/95) and
[Tester T02 in #22](https://github.com/juarezweiss/Paws/issues/22#issuecomment-6443532009).
The fresh branch starts at develop `01148933006f28334ed3b28da1c1f85e713bd866`.
Rendered runtime: `e9d9e3f164479b24f41a340d9018b3ac1b456ef1`.

## Copy and scope

[Owner CSV](tutorial_texts.csv) preserves the input content with line endings
normalized to LF for repository whitespace checks. The root input is unmodified;
its original SHA-256 is `aa83b4663321a9228d76f246631fb8678faf24da3caa852ed58d105d44ce4594`.
[Copy mapping](copy.json) matches each original lesson by content, including the
combined goal row mapped to the existing separate title/body. All 15 nonempty
updates change the corresponding top message in EN, pt-BR and ES. Seven blank
lessons, goal heading, buttons, bottom guidance and every other message are unchanged.
After stripping bold tags and normalizing whitespace, English matches Updated
exactly. Translations retain the instructional meaning and emphasized actions.

The first render found four-line text needing 78 units in a 60-unit compact
header: FirstColumnX, DiagonalPaw and SingleX in all locales, plus FourthPaw in
pt-BR and SecondRowX in ES. [Initial findings](initial-fit-findings.json) are
historical. Reflowing existing words fixed these cases: natural wrapping for the
long quota messages and an earlier explicit break in the hold instruction.
Practice still has a readable action break. No shorter wording, font reduction,
layout or runtime behavior change was needed.

Only runtime file changed: `TutorialLocalization.cs`. One obsolete localization
test assertion expecting the removed practice state-list was removed; localized
Paw labels, bold actions, the three-tap cycle and mandatory hold retain their
existing coverage. Tutorial controller, fixture, gestures, presentation/layout,
attention/fade mechanisms, progress, ordinary gameplay and monetization are
byte-identical to the branch base.

## Native desktop render review

All 22 tutorial steps × three locales × three viewports passed: **198 cases,
315 PNG captures**. Each run has 35 captures: initial and attentive phases of
all nine Continue/Start pages, an intermediate native fade, action lessons and
hold/release states. Safe areas are included. These are desktop review-player
captures at iPhone logical dimensions, not physical iPhone acceptance.

| Locale | Compact 320×568, safe 20/0 | Standard 390×844, safe 47/34 | Tall 430×932, safe 59/34 |
|---|---|---|---|
| EN | [All steps](en-320-all-steps.png) | [All steps](en-390-all-steps.png) | [All steps](en-430-all-steps.png) |
| pt-BR | [All steps](pt-BR-320-all-steps.png) | [All steps](pt-BR-390-all-steps.png) | [All steps](pt-BR-430-all-steps.png) |
| ES | [All steps](es-320-all-steps.png) | [All steps](es-390-all-steps.png) | [All steps](es-430-all-steps.png) |

[Compact initial/fade/attention/Start](pt-BR-320-phases.png) and
[standard Goal/Start phases](es-390-phases.png) show preserved guidance behavior.
Each `*-bounds.txt` records actual text and grid bounds. All messages fit at
16 units; no clipping, overlap or font shrinking. Top/bottom center errors are
at most 0.5 logical units, and every grid rectangle matches the earlier #95
baseline. Runtime layout checks also verify safe areas and action targets.

Primary review inspected all 198 top-message crops at native scale and the
representative full-screen phase panels. Independent read-only QA audited the
CSV mapping, all translations, formatting and scope, then inspected 23 compact
screenshots including every previously overflowing case; no blocking findings.
Standard/tall visual inspection was performed by the primary reviewer. Full-size
raw captures remain under ignored `.unity-results/issue99/captures/`; their hashes
and paths are in [the screenshot index](screenshots.json).

## Validation and reproduction

[Validation receipt](validation.json) records the final gates and render summary.
Final gates passed: repository validation, Unity compilation, **751/751 EditMode
tests** (zero failed/skipped/inconclusive), and **51/51 Python tests**. An earlier
run exposed the obsolete practice-copy assertion; a later run hit the previously
observed intermittent Settings safe-area test. The isolated Settings check and
final full run passed. All attempts and their dispositions are recorded in the
receipt; no unrelated menu fix was introduced.

The existing mounted tutorial harness checks the full sequence, actual pointer
tap/drag input, mandatory successful arrow hold/release, independent completion,
Continue timing, native intermediate opacity and interruption cleanup. The full
EditMode suite covers threshold/cancellation, rays, protected/completed cells,
Skip/replay/navigation/locale/lifecycle and progress regressions.

```sh
bash scripts/validate.sh
bash scripts/compile.sh
bash scripts/test.sh
python3 -m unittest discover -s scripts/tests -v

CATS_ARROWS_BOARD_REVIEW_OUTPUT="$PWD/.unity-results/issue99/PawsTutorialReview.app" \
  /Applications/Unity/Hub/Editor/6000.3.25f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "$PWD/UnityProject" \
  -executeMethod CatsAndArrows.UI.Editor.BoardReviewBuild.Build \
  -logFile "$PWD/.unity-results/issue99/build.log" -quit

.unity-results/issue99/PawsTutorialReview.app/Contents/MacOS/Paws \
  -screen-fullscreen 0 -screen-width 320 -screen-height 568 \
  --board-width=320 --board-height=568 --board-safe-top=20 --board-safe-bottom=0 \
  --board-locale=en --tutorial-check=1 \
  --game-captures="$PWD/.unity-results/issue99/captures/en-320" \
  -logFile "$PWD/.unity-results/issue99/en-320.log"
```

Repeat the player command for all locale/viewport/safe-area combinations in the
table. Use the pinned editor; close the GUI Editor before batch commands. Test
runs must complete with a passing NUnit report and successful editor exit.

## Remaining owner checks

Physical iPhone acceptance of **this new wording is pending**; #95's prior pass
does not cover it. On a future prepared build, replay Tutorial in Settings in
each language: read every message comfortably, confirm the separate goal and
bold actions, check compact spacing and delayed Continue/Start guidance, then
finish the mandatory hold and independent solve. Check Skip/replay, language
changes, navigation and background/foreground cleanup. Record device/OS, build,
languages, scope and explicit pass/fail. This task did not install a new build,
merge, release or change #95's merged history.
