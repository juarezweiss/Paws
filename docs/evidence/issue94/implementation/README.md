# #94 — Approved pale-sage Option B: native Unity evidence

Owner decision **2026-10-28** selects the **right column B** of the attached
`sage-390-normal-color.png`, superseding later prototype recommendations.
[Immutable approved mockup/specification](https://github.com/juarezweiss/Paws/blob/f68967b4dbe2f7e61cd2e66bae8f33b53f52c659/docs/evidence/issue94/sage-refinement/README.md).
The attachment and historical PNG have the same SHA-256:
`d9a07e0a4c711275737355d7f3fdc7978fdff70792dc5977ee62b54a134d7678`.

Implementation source: **7c346d101adfd3d38fb83a4b75527e2b70c7ef78**, branched from
fresh `develop` **a6cfa5576cbddc0f520b297df428af84aca55ac9**. The runtime change is
limited to `PortraitBoardView` and `BoardSymbolElement`: arrow cells/legend use
`#DFE7DC`; the existing dark shafted arrow paths remain; only Paw fill ink becomes
`#24332A` and its existing `.69` artwork scale is multiplied by exactly `1.10`.
Paw paths/proportions/light cell fill, X, protection, borders, grid/hit geometry,
input and gameplay are unchanged. No triangles, green-filled cells, new runtime
dependencies, player rebuild, main change or merge.

## Actual boards versus approved B

Columns show **approved mockup B → actual Unity normal play → actual Unity ~95%
marked**. Rows show real **Easy seed 3 → Hard seed 14**. The latter states exactly
retain the approved board marks: Easy 6 paws/13 X/1 Empty (19/20 editable = 95.0%);
Hard 13 paws/26 X/2 Empty (39/41 = 95.1%). One solution paw stays unplaced to keep
both boards active under the existing immediate-win rule.

| Portrait geometry | Color | Grayscale |
|---|---|---|
| Standard 390×844 | [Comparison](approved-vs-native-390-color.png) | [Comparison](approved-vs-native-390-gray.png) |
| Compact 320×568 | [Comparison](approved-vs-native-320-color.png) | [Comparison](approved-vs-native-320-gray.png) |

Actual boards are native UI Toolkit/Painter2D output captured from a mounted
`PortraitBoardView` into a RenderTexture in graphical EditMode, cropped with all
clues intact. No board/paw imagery was substituted. Comparison crops are placed
at native scale without resampling; tiny differences in rasterization and the
mockup's Arial approximation versus existing Unity LegacyRuntime are expected.
The mockup is a reconstructed reference, not a baseline Unity screenshot. All
native typography/grid code stays unchanged.

Grayscale applies linear-sRGB relative luminance (`.2126 R + .7152 G + .0722 B`),
then encodes back to sRGB. It changes neither pixel geometry nor image scale.
430×932 captures are also checked; their hashes/layout summaries, all twelve
capture checks and the native Paw probe are in [native-review.json](native-review.json).
No physical iPhone pass is claimed from simulated portrait output.

## Tests and reproducibility

- Pinned Unity **6000.3.25f1** compilation: pass (`compile.CqwXO9`).
- Graphical focused **97/97**: rendering, presentation, layout, tap/drag and arrow
  hold (`issue94-focused.sw4xfN`), complete passing XML, successful editor exit.
- Full required EditMode **754/754** (`editmode.3ocpya`), no failed/skipped/
  inconclusive cases, successful exit.
- Repository structure/JSON/TOML and whitespace: pass; **322 SDK pins** verified;
  Python runner/repository contracts **51/51** pass.
- Native checks on **12** Easy/Hard normal/dense frames at 320×568, 390×844,
  430×932: correct arrow/protected fills, original other-cell styling, no clipped
  Paw glyphs. Existing native fractional-cell edge snapping is allowed; specified
  cell sizes still equal BoardLayout exactly.
- Native 128px Paw probe: ink `#24332A`, 1,414 approved-ink pixels; footprint
  **60×58px**, matching continuous **60.23×58.78px** with pixel-edge tolerance.
  Old scale would be ~55px wide and fails this check. The production code uses
  exact `1.10f` scaling; raster extents necessarily quantize.
- Independent read-only QA: **No blocking findings.**

Run `ApprovedBoardRenderingTests` with the existing pinned Editor's graphical
EditMode test runner and `PAWS_94_CAPTURE` set to an absolute output directory;
omit `-nographics`. The other focused classes are `BoardPresentationTests`,
`BoardLayoutTests`, `BoardGestureControllerTests`, `ArrowHoldControllerTests`.
The default headless suite checks mounted palette/geometry; its optional pixel
branch does not substitute for the separately recorded graphical run. Raw PNGs,
bounds CSVs, logs and XML remain under ignored `.unity-results/`; reproduce them
from the same source and test fixture without a player build.

The first sandboxed launch could not open Unity's licensing/UPM socket and was
stopped; the fresh permitted run supplies the passing compile receipt. Initial
new-test assertions were corrected to account for existing native pixel snapping
and UI Toolkit's typed Length value; only the final passing reports above are
gates. No runtime geometry change was made to satisfy an assertion.

## Owner physical acceptance — 2026-10-28

The owner reported **“all pass”** for the installed #94 review build: source
`39f806901834e7eba0af674941eeaee244c03cb2`, Paws **0.1.0 (1)**, **iPhone 17 /
iOS 27.0.1**. This covers the handoff's approved sage-arrow/larger-paw readability
on Easy/Hard boards, tap/drag and arrow-hold behavior. It closes the #94 physical
review gate for that exact installation; no automated device interaction was
performed. [Exact owner report and scope](owner-validation-2026-10-28.json).

Other-device, unreported lifecycle, store/distribution and production acceptance
are not inferred. This record changes documentation only; runtime source and
previous passing test/native-render inputs remain unchanged. The owner also
explicitly authorized merging PR #101 into develop and cleanup. Main remains
outside that authorization. Rollback is a revert of the focused #94 commits;
no data migration or gameplay rollback is required.
