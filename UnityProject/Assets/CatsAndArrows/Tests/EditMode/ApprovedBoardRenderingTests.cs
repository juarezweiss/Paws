using System;
using System.Collections;
using System.IO;
using System.Linq;
using CatsAndArrows.Core;
using CatsAndArrows.Gameplay;
using CatsAndArrows.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace CatsAndArrows.Tests
{
    // #94: mounted geometry/palette contracts; opt-in native pixels reuse the same tests without a player build.
    public sealed class ApprovedBoardRenderingTests
    {
        private GameObject host;
        private PanelSettings settings;
        private RenderTexture target;
        private VisualElement root;
        private string Captures => Environment.GetEnvironmentVariable("PAWS_94_CAPTURE");

        [TearDown]
        public void Cleanup()
        {
            if (host != null) Object.DestroyImmediate(host);
            if (settings != null) Object.DestroyImmediate(settings);
            if (target != null) Object.DestroyImmediate(target);
        }

        private void Mount(int width, int height)
        {
            Cleanup();
            settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.scaleMode = PanelScaleMode.ConstantPixelSize; settings.scale = 1;
            target = new RenderTexture(width, height, 0); settings.targetTexture = target;
            host = new GameObject("issue94-approved-rendering", typeof(UIDocument));
            var document = host.GetComponent<UIDocument>(); document.panelSettings = settings;
            root = document.rootVisualElement;
            root.style.width = width; root.style.height = height;
            root.style.backgroundColor = PortraitBoardView.Tile;
        }
        private static IEnumerator Settle()
        {
            for (var frame = 0; frame < 5; frame++) { EditorApplication.QueuePlayerLoopUpdate(); yield return null; }
        }

        [UnityTest]
        public IEnumerator ApprovedPalettePreservesRealBoardGeometryAndMarksAtPortraitSizes()
        {
            foreach (var size in new[] { new Vector2Int(320, 568), new Vector2Int(390, 844), new Vector2Int(430, 932) })
            foreach (var profile in new[] { DifficultyProfile.Easy, DifficultyProfile.Hard })
            {
                var seed = profile.Id == "easy" ? 3 : 14;
                var result = new SeededPuzzleGenerator().Generate(profile, seed);
                Assert.That(result.IsSuccess, Is.True);
                var puzzle = result.Puzzle;
                var session = new GameSession(puzzle, ClueProtection.Derive(PuzzleClues.FromPuzzle(puzzle), profile));
                foreach (var paw in puzzle.SolutionPaws.Take(2)) { session.Tap(paw); session.Tap(paw); }
                var editableEmpty = session.Marks.Keys.Where(c => !puzzle.IsSolutionPaw(c)).OrderBy(c => c.Row).ThenBy(c => c.Column).ToArray();
                foreach (var c in editableEmpty.Take(profile.Id == "easy" ? 5 : 10)) session.Tap(c);
                Mount(size.x, size.y);
                var view = new PortraitBoardView(); view.SetPlayLayout(); root.Add(view); view.Bind(session, "en");
                yield return Settle();
                var top = size.x == 320 ? 20 : 47; var bottom = size.x == 320 ? 0 : 34;
                view.ApplySafeArea(new Rect(0, bottom, size.x, size.y - top - bottom), new Vector2(size.x, size.y));
                yield return Settle();
                var layout = view.Layout;
                var marks = session.Marks.ToArray();
                AssertPaletteAndGeometry(view, size, top, bottom);
                Assert.That(session.Marks, Is.EquivalentTo(marks));
                Capture(profile.Id + "-" + size.x + "-normal", view);

                session.Clear();
                foreach (var c in puzzle.SolutionPaws.Take(puzzle.SolutionPaws.Count - 1)) { session.Tap(c); session.Tap(c); }
                foreach (var c in editableEmpty.Take(editableEmpty.Length - (profile.Id == "hard" ? 1 : 0))) session.Tap(c);
                view.Refresh(); yield return Settle();
                Assert.That(session.IsCompleted, Is.False);
                Assert.That(session.Marks.Count(m => m.Value != PlayerMark.Empty), Is.EqualTo(profile.Id == "easy" ? 19 : 39));
                Assert.That(view.Layout.CellSize, Is.EqualTo(layout.CellSize));
                Assert.That(view.Layout.Gap, Is.EqualTo(layout.Gap));
                AssertPaletteAndGeometry(view, size, top, bottom);
                Capture(profile.Id + "-" + size.x + "-dense", view);
            }
        }

        private static void AssertPaletteAndGeometry(PortraitBoardView view, Vector2Int viewport, int top, int bottom)
        {
            foreach (var cell in view.Presentation.Cells)
            {
                var tile = view.Q<VisualElement>("cell-" + cell.Coordinate.Row + "-" + cell.Coordinate.Column);
                var expected = cell.Kind == BoardCellKind.Arrow ? new Color(223 / 255f, 231 / 255f, 220 / 255f)
                    : cell.Kind == BoardCellKind.Protected ? new Color(231 / 255f, 229 / 255f, 220 / 255f) : PortraitBoardView.Tile;
                Assert.That(tile.resolvedStyle.backgroundColor, Is.EqualTo(expected));
                Assert.That(tile.resolvedStyle.borderLeftWidth, Is.EqualTo(1));
                Assert.That(tile.resolvedStyle.borderTopLeftRadius, Is.EqualTo(7));
                Assert.That(tile.resolvedStyle.borderLeftColor, Is.EqualTo(new Color(126 / 255f, 140 / 255f, 131 / 255f)));
                Assert.That(tile.style.width.value.value, Is.EqualTo(view.Layout.CellSize));
                // Native panel bounds snap fractional cell edges to device pixels.
                Assert.That(tile.worldBound.width, Is.InRange(Mathf.Floor(view.Layout.CellSize), Mathf.Ceil(view.Layout.CellSize)));
                Assert.That(tile.worldBound.xMin, Is.GreaterThanOrEqualTo(16 - .05f));
                Assert.That(tile.worldBound.xMax, Is.LessThanOrEqualTo(viewport.x - 16 + .05f));
                Assert.That(tile.worldBound.yMin, Is.GreaterThanOrEqualTo(top + 16 - .05f));
                Assert.That(tile.worldBound.yMax, Is.LessThanOrEqualTo(viewport.y - bottom - 16 + .05f));
                Assert.That(tile.Q<BoardSymbolElement>().Kind, Is.EqualTo(cell.Kind));
                Assert.That(tile.Q<BoardSymbolElement>().pickingMode, Is.EqualTo(PickingMode.Ignore));
            }
        }

        [Test]
        public void LegendUsesSageForArrowsAndLeavesProtectedAndOtherSwatchesUnchanged()
        {
            var puzzle = new SeededPuzzleGenerator().Generate(DifficultyProfile.Easy, 3).Puzzle;
            var view = new PortraitBoardView(); view.Bind(new GameSession(puzzle), "en");
            var legend = view.Query<BoardSymbolElement>().ToList().Where(s => s.parent.name == "").ToArray();
            Assert.That(legend.Length, Is.EqualTo(5));
            foreach (var symbol in legend)
                Assert.That(symbol.parent.style.backgroundColor.value, Is.EqualTo(symbol.Kind == BoardCellKind.Arrow
                    ? new Color(223 / 255f, 231 / 255f, 220 / 255f)
                    : symbol.Kind == BoardCellKind.Protected ? PortraitBoardView.ImmovableTile : PortraitBoardView.Tile));
        }

        [UnityTest]
        public IEnumerator PawNativePixelsUseApprovedInkAndExactlyTenPercentLargerArtworkWhenCapturing()
        {
            // Default headless gates still mount the real element. The separately recorded graphical run
            // opts into pixel assertions and fails rather than claiming a capture on a null graphics device.
            Mount(128, 128);
            var paw = new BoardSymbolElement(BoardCellKind.Paw); paw.style.width = 128; paw.style.height = 128; root.Add(paw);
            yield return Settle();
            Assert.That(paw.contentRect.size, Is.EqualTo(new Vector2(128, 128)));
            if (string.IsNullOrEmpty(Captures)) yield break;
            var image = ReadTarget();
            try
            {
                var minX = 128; var minY = 128; var maxX = -1; var maxY = -1; var approvedInkPixels = 0;
                for (var y = 0; y < 128; y++) for (var x = 0; x < 128; x++)
                {
                    var c = (Color32)image.GetPixel(x, y);
                    if (Math.Abs(c.r - 36) <= 2 && Math.Abs(c.g - 51) <= 2 && Math.Abs(c.b - 42) <= 2) approvedInkPixels++;
                    if (c.r >= 128 || c.g >= 128 || c.b >= 128) continue;
                    minX = Math.Min(minX, x); minY = Math.Min(minY, y); maxX = Math.Max(maxX, x); maxY = Math.Max(maxY, y);
                }
                Assert.That(approvedInkPixels, Is.GreaterThan(100), "actual Paw branch must paint #24332A");
                Assert.That(maxX - minX + 1, Is.EqualTo(.62f * .69f * 1.10f * 128).Within(1.5f));
                Assert.That(maxY - minY + 1, Is.EqualTo(.605f * .69f * 1.10f * 128).Within(1.5f));
                Assert.That(minX, Is.GreaterThan(0)); Assert.That(maxX, Is.LessThan(127));
                Assert.That(minY, Is.GreaterThan(0)); Assert.That(maxY, Is.LessThan(127));
                Directory.CreateDirectory(Captures); File.WriteAllBytes(Path.Combine(Captures, "paw-128.png"), image.EncodeToPNG());
            }
            finally { Object.DestroyImmediate(image); }
        }

        private Texture2D ReadTarget()
        {
            Assert.That(SystemInfo.graphicsDeviceType, Is.Not.EqualTo(UnityEngine.Rendering.GraphicsDeviceType.Null));
            var previous = RenderTexture.active;
            var image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            try { RenderTexture.active = target; image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); image.Apply(); }
            finally { RenderTexture.active = previous; }
            return image;
        }
        private void Capture(string name, PortraitBoardView view)
        {
            if (string.IsNullOrEmpty(Captures)) return;
            var image = ReadTarget();
            try
            {
                Directory.CreateDirectory(Captures);
                File.WriteAllBytes(Path.Combine(Captures, name + ".png"), image.EncodeToPNG());
                File.WriteAllLines(Path.Combine(Captures, name + "-bounds.csv"), new[] { "row,column,kind,x,y,width,height" }.Concat(
                    view.Presentation.Cells.Select(c =>
                    {
                        var tile = view.Q<VisualElement>("cell-" + c.Coordinate.Row + "-" + c.Coordinate.Column);
                        var r = tile.worldBound;
                        return FormattableString.Invariant($"{c.Coordinate.Row},{c.Coordinate.Column},{c.Kind},{r.x},{r.y},{r.width},{r.height}");
                    })));
            }
            finally { Object.DestroyImmediate(image); }
        }
    }
}
