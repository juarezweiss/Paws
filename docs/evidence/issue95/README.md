# Issue #95 — tutorial clarity evidence

Runtime/test source: `dbb6f1521e3497777ffef0deb77ad3f2f0dda233`, based on fetched
develop `3a44192697b8f1c416cc49afabeb839c5a0c3dc7`. Subsequent documentation and
evidence commits do not change those runtime inputs. The owner explicitly approved
the 18 shorter locale/message pairs proposed in
[#95's fit correction](https://github.com/juarezweiss/Paws/issues/95#issuecomment-6454961390).
Other approved copy is unchanged.

[Validation receipt](validation.json) records hashes, test reports, prior failures,
limits and the pending physical review. [Screenshot index](screenshots.json)
records all 225 original captures and their SHA-256 hashes. Original captures,
logs and the local macOS review player remain under ignored `.unity-results/issue95/`.
The nine contact sheets below show all 22 steps in order; the diagonal tile shows
the active hold. Additional original captures show hold release and the unchanged
fourth-paw stage.

| Logical portrait viewport / safe top, bottom | EN | pt-BR | ES |
|---|---|---|---|
| 320×568 / 20, 0 | [All steps](en-320-all-steps.png) | [All steps](pt-BR-320-all-steps.png) | [All steps](es-320-all-steps.png) |
| 390×844 / 47, 34 | [All steps](en-390-all-steps.png) | [All steps](pt-BR-390-all-steps.png) | [All steps](es-390-all-steps.png) |
| 430×932 / 59, 34 | [All steps](en-430-all-steps.png) | [All steps](pt-BR-430-all-steps.png) | [All steps](es-430-all-steps.png) |

All nine desktop runs exited zero and passed the complete simulated-touch
walkthrough, including short/pending-cancelled/wrong-arrow/off-origin holds,
successful hold, release and active-pause cancellation, guided-to-independent
rebind, wrong-paw recovery, completed lock, navigation, replay and Skip.
All 198 step/locale/size cases passed text bounds, 16-unit body font, 48-unit
targets, exact helper/heading visibility, safe areas and stationary grid checks.
Per-run `*-bounds.txt` files retain actual geometry. The nine instruction contact
sheets were visually inspected, with these full compact examples checked:

- [Goal and adjacent Continue guidance, pt-BR](pt-BR-320-Goal-intro-Goal.png)
- [Exact diagonal path while held, ES](es-320-DiagonalPaw-hold-active.png)
- [Forward-ray explanation, EN](en-320-Arrows-intro-Arrows.png)

Pinned Unity 6000.3.25f1 compilation passed; final EditMode **674/674**, zero
failed/skipped/inconclusive, successful exit. Python contracts **51/51** and
repository/whitespace validation passed. Independent QA: **No blocking findings**.
One earlier menu safe-area assertion failure is preserved in the receipt; it did
not recur in isolated tests (3/3) or the final full suite. No root cause or
unrelated menu fix is claimed. Earlier overflowing copy and review-harness timing
failures are also retained separately from the passing final evidence.

Reproduce with the existing `BoardReviewBuild.Build` local macOS entry point,
then run its player with `--tutorial-check=1 --board-locale=en`,
`--board-width=320 --board-height=568 --board-safe-top=20 --board-safe-bottom=0`
and an absolute `--game-captures=` directory. Repeat the table's nine combinations.
The tutorial-only review mode uses constant logical pixel scale; the production
panel asset and all fixture/gesture/game-rule source files remain unchanged.

## Owner physical-iPhone review — pending

On the prepared candidate, record source/version/build, reviewer, model/iOS,
locale(s), pass/fail for each item and any gaps:

- Read the Goal heading/body and nearby Continue guidance; confirm it appears on
  Welcome/Goal/StatesIntro only. Check readable text and comfortable controls.
- Complete the three-tap practice and swipe from Empty, especially R1C2 after
  R1C1 is X. Confirm existing marks and protected cells behave normally.
- Hold the outlined diagonal arrow; confirm its path, release/cancel cleanup and
  required successful hold before placing the paw.
- Finish guided play, then independently solve the reset same board; confirm
  immediate victory, readonly completion and Play.
- Check Skip, Settings replay, Home resume, language changes and background/focus
  return. Verify replay resets practice/hold progress.

No iPhone build/install or physical review was performed for this task. Desktop
simulations, screenshots and automated checks do not satisfy this owner gate.
No merge or release is authorized or performed.
