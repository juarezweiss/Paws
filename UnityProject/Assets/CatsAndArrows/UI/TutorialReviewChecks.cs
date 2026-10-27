#if !PAWS_PRODUCTION
using System;
using System.Collections;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using CatsAndArrows.Core;
using CatsAndArrows.Gameplay;
using UnityEngine;
using UnityEngine.UIElements;

namespace CatsAndArrows.UI
{
    // Issue #26: opt-in native mounted-panel evidence, not a physical touch test.
    internal static class TutorialReviewChecks
    {
        private static Rect initialGrid;
        private static readonly List<string> layoutFailures = new List<string>();
        public static IEnumerator Run(GameShellView shell, string captures)
        {
            for (var i = 0; i < 12; i++) yield return new WaitForEndOfFrame();
            initialGrid = default;
            layoutFailures.Clear();
            if (!Check(() => Click(shell, "menu-start"))) yield break;
            yield return new WaitForEndOfFrame();
            if (!Check(() =>
            {
                Require(shell.Tutorial?.Step == TutorialStep.Welcome, "first Start enters tutorial");
                Require(shell.Q<Button>("action-continue").style.borderLeftWidth.value == 3, "welcome Continue highlighted");
            })) yield break;
            yield return Capture(shell, captures, "welcome");
            while (shell.Tutorial.CanContinue)
            {
                if (!Check(() => Click(shell, "action-continue"))) yield break;
                yield return new WaitForEndOfFrame();
                if (!Check(() =>
                {
                    if (shell.Tutorial.DemoMark.HasValue)
                    {
                        Require(shell.ActiveSession.Marks.Values.All(mark => mark == PlayerMark.Empty), "demo leaves all player marks Empty");
                        var expected = shell.Tutorial.DemoMark.Value == PlayerMark.X ? BoardCellKind.X
                            : shell.Tutorial.DemoMark.Value == PlayerMark.Paw ? BoardCellKind.Paw : BoardCellKind.Empty;
                        Require(shell.BoardView.Q<VisualElement>("cell-" + shell.Tutorial.DemoCoordinate.Value.Row + "-" + shell.Tutorial.DemoCoordinate.Value.Column).Q<BoardSymbolElement>().Kind == expected, "demo symbol matches lesson");
                        Require(shell.BoardView.Q<Label>("completion-status").text.Contains("<b>"), "state keyword bold");
                    }
                })) yield break;
                yield return Capture(shell, captures, "intro-" + shell.Tutorial.Step);
            }
            if (!Check(() =>
            {
                Require(shell.Tutorial.Step == TutorialStep.Practice, "practice after information");
                Tap(shell, 4, 4); // Off-target input is ignored.
                Require(shell.ActiveSession.Marks.Values.All(mark => mark == PlayerMark.Empty), "off-target no mutation");
                var p = Point(shell, 0, 0); Send(Grid(shell), p, TouchPhase.Began);
                shell.CancelInteraction(); Send(Grid(shell), p, TouchPhase.Ended);
                Require(shell.ActiveSession.GetMark(new BoardCoordinate(0, 0)) == PlayerMark.Empty, "cancel no tap");
                Tap(shell, 0, 0); Tap(shell, 0, 0);
                Require(shell.Tutorial.Step == TutorialStep.Practice, "practice requires complete cycle");
                Tap(shell, 0, 0);
                Require(shell.Tutorial.Step == TutorialStep.ZeroColumn, "practice full cycle");
            })) yield break;
            yield return Capture(shell, captures, "zero-column");
            if (!Check(() =>
            {
                var grid = Grid(shell); var start = Point(shell, 0, 0); var end = Point(shell, 4, 0);
                Send(grid, start, TouchPhase.Began); Send(grid, end, TouchPhase.Moved);
                Require(shell.Tutorial.Step == TutorialStep.ZeroColumn, "no advance during swipe");
                Send(grid, end, TouchPhase.Ended);
                Require(shell.Tutorial.Step == TutorialStep.ZeroRow, "advance on release even without new marks");
                Require(shell.ActiveSession.GetMark(new BoardCoordinate(0, 1)) == PlayerMark.Empty, "no next-step spill");
            })) yield break;
            yield return Capture(shell, captures, "zero-row");
            if (!Check(() => { Drag(shell, 0, 1, 0, 4); Require(shell.Tutorial.Step == TutorialStep.FirstPaw, "zero row skips fixed/intersection"); })) yield break;
            yield return Capture(shell, captures, "first-paw");
            if (!Check(() => { Tap(shell, 1, 4); Tap(shell, 1, 4); Require(shell.Tutorial.Step == TutorialStep.FirstColumnX, "first paw"); })) yield break;
            yield return Capture(shell, captures, "column-x");
            if (!Check(() => { Drag(shell, 2, 4, 4, 4); Require(shell.Tutorial.Step == TutorialStep.SecondPaw, "column quota"); })) yield break;
            yield return Capture(shell, captures, "second-paw");
            if (!Check(() => { Tap(shell, 4, 2); Tap(shell, 4, 2); Require(shell.Tutorial.Step == TutorialStep.SecondRowX, "second paw"); })) yield break;
            yield return Capture(shell, captures, "row-x");
            if (!Check(() =>
            {
                Drag(shell, 4, 1, 4, 3);
                Require(shell.Tutorial.Step == TutorialStep.DiagonalPaw, "swipe enters combined arrow-hold lesson");
                Require(shell.Q<Button>("action-continue") == null, "mandatory hold has no Continue bypass");
                Require(shell.Tutorial.HighlightedCells.SequenceEqual(new[] { new BoardCoordinate(0, 3) }), "lesson highlights only the diagonal arrow");
                Require(shell.ActiveSession.GetMark(new BoardCoordinate(4, 2)) == PlayerMark.Paw, "existing paw preserved");
            })) yield break;
            yield return Capture(shell, captures, "arrow-hold");
            if (!Check(() =>
            {
                Require(shell.Tutorial.Step == TutorialStep.DiagonalPaw, "interactive arrow hold");
                var grid = Grid(shell); var arrow = Point(shell, 0, 3);
                Send(grid, arrow, TouchPhase.Began); Send(grid, arrow, TouchPhase.Ended);
                Tap(shell, 2, 1); Tap(shell, 2, 1);
                Require(shell.ActiveSession.GetMark(new BoardCoordinate(2, 1)) == PlayerMark.Empty, "short arrow press cannot unlock paw input");
                // A pending hold lost to focus must not unlock later, even if its
                // old scheduled tick runs after the threshold.
                Send(grid, arrow, TouchPhase.Began);
                shell.SetApplicationFocus(false);
                shell.SetApplicationFocus(true);
            })) yield break;
            yield return new WaitForSecondsRealtime((float)ArrowHoldController.HoldSeconds + .05f);
            if (!Check(() =>
            {
                var grid = Grid(shell); var arrow = Point(shell, 0, 3);
                Send(grid, arrow, TouchPhase.Ended);
                Require(!shell.Tutorial.CanTap(new BoardCoordinate(2, 1)), "cancelled pending hold cannot unlock paw");
                Send(grid, Point(shell, 1, 3), TouchPhase.Began); // Wrong cardinal arrow.
                Send(grid, Point(shell, 1, 3), TouchPhase.Ended);
                Require(!shell.Tutorial.CanTap(new BoardCoordinate(2, 1)), "wrong arrow cannot unlock paw");
                Send(grid, arrow, TouchPhase.Began);
                Send(grid, Point(shell, 0, 4), TouchPhase.Moved);
                Send(grid, Point(shell, 0, 4), TouchPhase.Ended);
                Require(!shell.Tutorial.CanTap(new BoardCoordinate(2, 1)), "off-origin hold cannot unlock paw");
            })) yield break;
            var holdGrid = Grid(shell); var holdArrow = Point(shell, 0, 3);
            Send(holdGrid, holdArrow, TouchPhase.Began);
            yield return new WaitForSecondsRealtime((float)ArrowHoldController.HoldSeconds + .05f);
            if (!Check(() =>
            {
                foreach (var coordinate in new[] { new BoardCoordinate(0, 3), new BoardCoordinate(1, 2), new BoardCoordinate(2, 1), new BoardCoordinate(3, 0) })
                    Require(shell.BoardView.Q<VisualElement>("cell-" + coordinate.Row + "-" + coordinate.Column).style.borderLeftWidth.value == 3, "held diagonal path outlined");
            })) yield break;
            yield return Capture(shell, captures, "hold-active");
            Send(holdGrid, holdArrow, TouchPhase.Ended);
            if (!Check(() =>
            {
                Require(shell.BoardView.Q<VisualElement>("cell-0-3").style.borderLeftWidth.value == 3, "tutorial arrow outline restored");
                foreach (var coordinate in new[] { new BoardCoordinate(1, 2), new BoardCoordinate(2, 1), new BoardCoordinate(3, 0) })
                    Require(shell.BoardView.Q<VisualElement>("cell-" + coordinate.Row + "-" + coordinate.Column).style.borderLeftWidth.value == 1, "released path cleared");
            })) yield break;
            yield return Capture(shell, captures, "hold-released");
            // Active cancellation uses the same cleanup; a completed successful
            // hold remains learned until replay, as before #95.
            Send(holdGrid, holdArrow, TouchPhase.Began);
            yield return new WaitForSecondsRealtime((float)ArrowHoldController.HoldSeconds + .05f);
            if (!Check(() =>
            {
                var marks = shell.ActiveSession.Marks.ToArray();
                shell.SetApplicationPaused(true); shell.SetApplicationPaused(false);
                Send(holdGrid, holdArrow, TouchPhase.Ended);
                foreach (var coordinate in new[] { new BoardCoordinate(1, 2), new BoardCoordinate(2, 1), new BoardCoordinate(3, 0) })
                    Require(shell.BoardView.Q<VisualElement>("cell-" + coordinate.Row + "-" + coordinate.Column).style.borderLeftWidth.value == 1, "paused active path cleared");
                Require(shell.ActiveSession.Marks.SequenceEqual(marks) && shell.Tutorial.Step == TutorialStep.DiagonalPaw, "hold cancellation cannot mark or advance");
                Require(shell.Tutorial.CanTap(new BoardCoordinate(2, 1)), "successful hold gate survives pause");
            })) yield break;
            if (!Check(() =>
            {
                Tap(shell, 2, 1); Tap(shell, 2, 1);
                Require(shell.Tutorial.Step == TutorialStep.FourthPaw, "held arrow unlocks diagonal paw");
            })) yield break;
            yield return Capture(shell, captures, "diagonal");
            yield return Capture(shell, captures, "two-paw-row");
            if (!Check(() => { Tap(shell, 2, 3); Tap(shell, 2, 3); Require(shell.Tutorial.Step == TutorialStep.SingleX, "two paw row"); })) yield break;
            yield return Capture(shell, captures, "single-x");
            if (!Check(() => { Tap(shell, 3, 3); Require(shell.Tutorial.Step == TutorialStep.FinalPaw, "single X cleanup"); })) yield break;
            yield return Capture(shell, captures, "last-paw");
            if (!Check(() =>
            {
                Tap(shell, 3, 1); Tap(shell, 3, 1);
                Require(shell.Tutorial.Step == TutorialStep.Independent && !shell.ActiveSession.IsCompleted, "guided solve enters independent practice");
                Require(shell.ActiveSession.Marks.Values.All(mark => mark == PlayerMark.Empty), "same board reset to Empty");
                Require(shell.BoardView.InteractionEnabled && shell.Q<Button>("action-play") == null, "independent input enabled without random Play");
                Require(shell.Q<Button>("action-hint") == null && shell.Q<Button>("action-clear") == null, "no normal commands during tutorial");
            })) yield break;
            yield return Capture(shell, captures, "independent");
            if (!Check(() =>
            {
                var session = shell.ActiveSession;
                var locale = shell.Locale;
                var hadLanguage = PlayerPrefs.HasKey(LanguagePreference.Key);
                var previousLanguage = PlayerPrefs.GetString(LanguagePreference.Key);
                try
                {
                    shell.ShowMenu(); shell.SelectLanguage(locale == "en" ? "pt-BR" : "en"); shell.BackToGame();
                    Require(shell.Tutorial.Step == TutorialStep.Independent && shell.ActiveSession == session && shell.BoardView.InteractionEnabled, "independent survives menu/language return");
                    shell.ShowMenu(); shell.SelectLanguage(locale); shell.BackToGame();
                }
                finally
                {
                    if (hadLanguage) PlayerPrefs.SetString(LanguagePreference.Key, previousLanguage); else PlayerPrefs.DeleteKey(LanguagePreference.Key);
                    PlayerPrefs.Save();
                }
            })) yield break;
            yield return new WaitForEndOfFrame();
            if (!Check(() =>
            {
                Tap(shell, 0, 1); Tap(shell, 0, 1); // A wrong extra paw must prevent completion.
                Drag(shell, 0, 0, 4, 0); // Free input supports the same ordinary swipe rules.
                foreach (var paw in shell.ActiveSession.Puzzle.SolutionPaws.ToArray())
                { Tap(shell, paw.Row, paw.Column); Tap(shell, paw.Row, paw.Column); }
                Require(shell.Tutorial.Step == TutorialStep.Independent && !shell.ActiveSession.IsCompleted, "wrong extra paw prevents victory");
                Tap(shell, 0, 1); // Correct the hypothesis; non-paw cells may remain Empty.
                Require(shell.Tutorial.IsComplete && shell.ActiveSession.IsCompleted, "independent solve completes tutorial");
                var marks = shell.ActiveSession.Marks.ToArray(); Tap(shell, 3, 1);
                Require(shell.ActiveSession.Marks.SequenceEqual(marks) && !shell.BoardView.InteractionEnabled, "completed board locked");
            })) yield break;
            yield return Capture(shell, captures, "complete");
            if (!Check(() => Click(shell, "action-play"))) yield break;
            yield return Ready(shell);
            if (!Check(() => Require(shell.Tutorial == null && shell.ActiveSession == shell.Loop.Session && shell.ActiveSession != null, "Play opens random"))) yield break;
            if (!Check(() => Click(shell, "action-menu"))) yield break;
            yield return new WaitForEndOfFrame();
            if (!Check(() => Click(shell, "menu-settings"))) yield break;
            yield return new WaitForEndOfFrame();
            if (!Check(() => Click(shell, "settings-tutorial"))) yield break;
            yield return new WaitForEndOfFrame();
            if (!Check(() => { Require(shell.Tutorial.Step == TutorialStep.Welcome && shell.ActiveSession.Marks.Values.All(mark => mark == PlayerMark.Empty), "manual replay fresh"); })) yield break;
            while (shell.Tutorial.CanContinue)
            {
                if (!Check(() => Click(shell, "action-continue"))) yield break;
                yield return new WaitForEndOfFrame();
            }
            if (!Check(() =>
            {
                Tap(shell, 0, 0); Tap(shell, 0, 0); Tap(shell, 0, 0);
                var grid = Grid(shell); var start = Point(shell, 0, 0); var partial = Point(shell, 2, 0);
                Send(grid, start, TouchPhase.Began); Send(grid, partial, TouchPhase.Moved);
                Require(grid.HasPointerCapture(PointerId.touchPointerIdBase), "active swipe before Skip");
                Click(shell, "action-skip"); Send(grid, partial, TouchPhase.Ended);
                Require(!grid.HasPointerCapture(PointerId.touchPointerIdBase), "Skip cancels active swipe");
            })) yield break;
            yield return Ready(shell);
            if (!Check(() => Require(shell.Tutorial == null && shell.ActiveSession != null && shell.ActiveSession.Marks.Values.All(mark => mark == PlayerMark.Empty), "Skip opens random"))) yield break;
            if (!Check(() => Click(shell, "action-menu"))) yield break;
            yield return new WaitForEndOfFrame();
            if (!Check(() => Click(shell, "menu-start"))) yield break;
            yield return Ready(shell);
            if (!Check(() => { Require(shell.Tutorial == null && shell.ActiveSession != null, "first play not repeated"); Debug.Log("TUTORIAL_REVIEW_PASS locale=" + shell.Locale + " source=synthetic-touch"); })) yield break;
            Debug.Log("TUTORIAL_LAYOUT_FAILURES " + layoutFailures.Count);
            Application.Quit(layoutFailures.Count == 0 ? 0 : 1);
        }
        private static IEnumerator Ready(GameShellView shell)
        {
            for (var i = 0; i < 120 && shell.Modal == "loading"; i++) yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
        }
        private static IEnumerator Capture(GameShellView shell, string directory, string name)
        {
            for (var i = 0; i < 2; i++) yield return new WaitForEndOfFrame();
            try
            {
                var board = Grid(shell); var layout = shell.BoardView.Layout;
                if (initialGrid == default) initialGrid = board.worldBound;
                Require(Vector2.Distance(initialGrid.position, board.worldBound.position) < .6f &&
                    Vector2.Distance(initialGrid.size, board.worldBound.size) < .6f, "grid geometry stays fixed: " + name);
                var center = board.LocalToWorld(new Vector2(layout.ClueGutter + layout.GridSize / 2, layout.ClueGutter + layout.GridSize / 2));
                Require(Vector2.Distance(center, board.parent.worldBound.center) < .6f, "tile grid centered");
                var label = shell.BoardView.Q<Label>("completion-status");
                var measured = label.MeasureTextSize(label.text, label.contentRect.width, VisualElement.MeasureMode.Exactly, 0, VisualElement.MeasureMode.Undefined);
                Debug.Log("TUTORIAL_LAYOUT step=" + shell.Tutorial.Step + " locale=" + shell.Locale +
                    " viewport=" + shell.contentRect.size + " measured=" + measured + " label=" + label.worldBound +
                    " clueTop=" + board.Q<Label>("column-clue-0").worldBound.yMin + " grid=" + board.worldBound);
                Require(measured.y <= label.contentRect.height + 1, "instruction fits header: " + name + " needs=" + measured.y + " has=" + label.contentRect.height);
                Require(label.worldBound.yMax <= board.Q<Label>("column-clue-0").worldBound.yMin + 1, "instruction stays above column clues: " + name);
                Require(Mathf.Abs(label.resolvedStyle.fontSize - 16) < .1f, "body remains 16 units");
                var goal = shell.BoardView.Q<Label>("tutorial-goal-title");
                Require((goal.resolvedStyle.display != DisplayStyle.None) == (shell.Tutorial.Step == TutorialStep.Goal), "goal heading only on Goal");
                if (shell.Tutorial.Step == TutorialStep.Goal)
                {
                    Require(goal.worldBound.yMax <= label.worldBound.yMin + 1, "goal heading above explanation");
                    Fits(goal);
                }
                var guidance = shell.Q<Label>("tutorial-continue-guidance");
                var needsGuidance = shell.Tutorial.Step == TutorialStep.Welcome || shell.Tutorial.Step == TutorialStep.Goal || shell.Tutorial.Step == TutorialStep.StatesIntro;
                Require((guidance != null) == needsGuidance, "Continue guidance on exactly three steps");
                if (guidance != null)
                {
                    Fits(guidance);
                    var button = shell.Q<Button>("action-continue");
                    Require(guidance.worldBound.yMax <= button.worldBound.yMin && button.worldBound.yMin - guidance.worldBound.yMax <= 16, "guidance adjacent above Continue");
                    Require(guidance.worldBound.yMin >= board.worldBound.yMax, "guidance clears grid");
                    Require(!guidance.focusable && guidance.pickingMode == PickingMode.Ignore, "guidance cannot intercept touch/focus");
                }
                foreach (var button in shell.Q<VisualElement>("game-controls").Query<Button>().ToList())
                {
                    Require(button.worldBound.height >= 48 && button.worldBound.width >= 48, "48-unit action targets");
                    Fits(button);
                    var safe = shell.Q<VisualElement>("game-safe-frame").worldBound;
                    Require(safe.Contains(button.worldBound.min) && safe.Contains(button.worldBound.max - Vector2.one * .1f), "actions respect safe area");
                }
                if (string.IsNullOrEmpty(directory)) yield break;
                Require(Path.IsPathRooted(directory), "absolute capture directory"); Directory.CreateDirectory(directory);
                var filename = shell.Tutorial.Step + "-" + name + ".png";
                File.AppendAllText(Path.Combine(directory, "layout.tsv"), string.Join("\t", new[] {
                    shell.Locale, shell.Tutorial.Step.ToString(), filename,
                    shell.contentRect.width.ToString(CultureInfo.InvariantCulture), shell.contentRect.height.ToString(CultureInfo.InvariantCulture),
                    measured.y.ToString(CultureInfo.InvariantCulture), label.contentRect.height.ToString(CultureInfo.InvariantCulture),
                    label.worldBound.yMax.ToString(CultureInfo.InvariantCulture), board.Q<Label>("column-clue-0").worldBound.yMin.ToString(CultureInfo.InvariantCulture),
                    board.worldBound.ToString(), shell.Q<VisualElement>("game-safe-frame").worldBound.ToString()
                }) + "\n");
            }
            catch (Exception error)
            {
                var failure = shell.Tutorial.Step + ": " + error.Message;
                layoutFailures.Add(failure);
                Debug.LogError("TUTORIAL_LAYOUT_FAIL " + failure);
            }
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
                ScreenCapture.CaptureScreenshot(Path.Combine(directory, shell.Tutorial.Step + "-" + name + ".png"));
            }
            yield return new WaitForSecondsRealtime(.15f);
        }
        private static void Fits(TextElement label)
        {
            var size = label.MeasureTextSize(label.text, label.contentRect.width, VisualElement.MeasureMode.Exactly, 0, VisualElement.MeasureMode.Undefined);
            Require(size.y <= label.contentRect.height + 1, "text fits " + label.name);
        }
        private static VisualElement Grid(GameShellView shell) => shell.BoardView.Q<VisualElement>("board-grid");
        private static Vector2 Point(GameShellView shell, int row, int column) => Grid(shell).LocalToWorld(shell.BoardView.Layout.GetCellRect(new BoardCoordinate(row, column)).center);
        private static void Tap(GameShellView shell, int row, int column) { var grid = Grid(shell); var point = Point(shell, row, column); Send(grid, point, TouchPhase.Began); Send(grid, point, TouchPhase.Ended); }
        private static void Drag(GameShellView shell, int r1, int c1, int r2, int c2) { var grid = Grid(shell); Send(grid, Point(shell, r1, c1), TouchPhase.Began); Send(grid, Point(shell, r2, c2), TouchPhase.Moved); Send(grid, Point(shell, r2, c2), TouchPhase.Ended); }
        private static void Send(VisualElement target, Vector2 position, TouchPhase phase)
        {
            var touch = new Touch { fingerId = 0, position = position, rawPosition = position, phase = phase, type = TouchType.Direct, pressure = 1, maximumPossiblePressure = 1 };
            if (phase == TouchPhase.Began) { using var e = PointerDownEvent.GetPooled(touch, EventModifiers.None); e.target = target; target.SendEvent(e); }
            else if (phase == TouchPhase.Ended) { using var e = PointerUpEvent.GetPooled(touch, EventModifiers.None); e.target = target; target.SendEvent(e); }
            else { using var e = PointerMoveEvent.GetPooled(touch, EventModifiers.None); e.target = target; target.SendEvent(e); }
        }
        private static void Click(GameShellView shell, string name)
        {
            var button = shell.Q<Button>(name); Require(button != null && button.enabledInHierarchy && button.worldBound.width > 0, "mounted " + name);
            var position = button.worldBound.center; var picked = shell.panel.Pick(position);
            Require(picked == button || (picked != null && button.Contains(picked)), "button picked " + name);
            using (var e = PointerDownEvent.GetPooled(new Event { type = EventType.MouseDown, button = 0, mousePosition = position })) { e.target = button; button.SendEvent(e); }
            using (var e = PointerUpEvent.GetPooled(new Event { type = EventType.MouseUp, button = 0, mousePosition = position })) { e.target = button; button.SendEvent(e); }
        }
        private static bool Check(Action action) { try { action(); return true; } catch (Exception e) { Debug.LogException(e); Application.Quit(1); return false; } }
        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException("Tutorial review failed: " + message); }
    }
}

#endif
