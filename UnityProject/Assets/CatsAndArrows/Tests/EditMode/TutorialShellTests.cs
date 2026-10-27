using System;
using System.Linq;
using CatsAndArrows.Core;
using CatsAndArrows.Gameplay;
using CatsAndArrows.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace CatsAndArrows.Tests
{
    public sealed class TutorialShellTests
    {
        private bool hadLanguage, hadSeen;
        private string language;
        private int seenValue;
        [SetUp] public void PreservePreferences()
        {
            hadLanguage = PlayerPrefs.HasKey(LanguagePreference.Key);
            language = PlayerPrefs.GetString(LanguagePreference.Key);
            hadSeen = PlayerPrefs.HasKey(TutorialPreference.Key);
            seenValue = PlayerPrefs.GetInt(TutorialPreference.Key);
        }
        [TearDown] public void RestorePreferences()
        {
            if (hadLanguage) PlayerPrefs.SetString(LanguagePreference.Key, language); else PlayerPrefs.DeleteKey(LanguagePreference.Key);
            if (hadSeen) PlayerPrefs.SetInt(TutorialPreference.Key, seenValue); else PlayerPrefs.DeleteKey(TutorialPreference.Key);
            PlayerPrefs.Save();
        }
        [Test] public void FirstStartRoutesOnceAndSavesSeenOnlyWhenPresented()
        {
            var seen = false; var writes = 0;
            var shell = PlayTestFactory.Shell(14, "en", tutorialSeen: () => seen, markTutorialSeen: () => { seen = true; writes++; });
            Assert.That(writes, Is.Zero);
            Assert.That(shell.Q<Button>("menu-settings"), Is.Not.Null);
            shell.StartPlaying();
            Assert.That(shell.Tutorial.Step, Is.EqualTo(TutorialStep.Welcome));
            Assert.That(shell.ActiveSession, Is.SameAs(shell.Tutorial.Session));
            Assert.That(shell.Loop.Session, Is.Null);
            Assert.That(seen, Is.True); Assert.That(writes, Is.EqualTo(1));
            var tutorial = shell.Tutorial;
            shell.ShowMenu(); shell.StartPlaying();
            Assert.That(shell.Tutorial, Is.SameAs(tutorial));
            Assert.That(shell.Modal, Is.Null);
            shell.SkipTutorial();
            Assert.That(shell.Modal, Is.EqualTo("loading"));
            Assert.That(writes, Is.EqualTo(1));
            Assert.That(shell.StartNewGame(), Is.True);
            Assert.That(shell.Tutorial, Is.Null);
            Assert.That(shell.ActiveSession, Is.SameAs(shell.Loop.Session));
        }
        [Test] public void ManualReplayIsFreshAndDoesNotConsumeRandomSeeds()
        {
            var seen = false;
            var shell = PlayTestFactory.Shell(14, "en", tutorialSeen: () => seen, markTutorialSeen: () => seen = true);
            shell.StartTutorial(); var first = shell.Tutorial;
            while (first.CanContinue) shell.ContinueTutorial();
            first.Tap(new BoardCoordinate(0, 0)); first.EndGesture();
            Assert.That(first.Session.GetMark(new BoardCoordinate(0, 0)), Is.EqualTo(PlayerMark.X));
            shell.StartTutorial();
            Assert.That(shell.Tutorial, Is.Not.SameAs(first));
            Assert.That(shell.Tutorial.Step, Is.EqualTo(TutorialStep.Welcome));
            Assert.That(shell.ActiveSession.Marks.Values, Is.All.EqualTo(PlayerMark.Empty));
            Assert.That(shell.NewGameRequestId, Is.Zero);
            Assert.That(shell.StartNewGame(), Is.True);
            Assert.That(shell.Loop.RequestedSeed, Is.EqualTo(14));
        }
        [TestCase("en")][TestCase("pt-BR")][TestCase("es")]
        public void TutorialMessagesLocalizedAndMenuLanguageResumePreservesProgress(string locale)
        {
            var shell = PlayTestFactory.Shell(14, locale, tutorialSeen: () => true, markTutorialSeen: () => { });
            shell.StartTutorial(); while (shell.Tutorial.CanContinue) shell.ContinueTutorial();
            var tutorial = shell.Tutorial; tutorial.Tap(new BoardCoordinate(0, 0)); tutorial.EndGesture();
            var marks = tutorial.Session.Marks.ToArray();
            shell.ShowMenu(); shell.SelectLanguage(locale == "en" ? "pt-BR" : "en"); shell.BackToGame();
            Assert.That(shell.Tutorial, Is.SameAs(tutorial));
            Assert.That(shell.Tutorial.Step, Is.EqualTo(TutorialStep.Practice));
            Assert.That(shell.ActiveSession.Marks, Is.EquivalentTo(marks));
            Assert.That(shell.Q<Button>("action-hint"), Is.Null);
            Assert.That(shell.Q<Button>("action-clear"), Is.Null);
            Assert.That(shell.Q<Button>("action-skip"), Is.Not.Null);
            Assert.That(shell.BoardView.Q<Label>("completion-status").text, Is.Not.Empty);
            var before = shell.ActiveSession.Marks.ToArray(); shell.RequestHint(); shell.ClearBoard();
            Assert.That(shell.ActiveSession.Marks, Is.EquivalentTo(before));
            var catalog = GameLocalization.CreateCatalog();
            var resource = catalog.GetResource(locale).Messages;
            var keys = catalog.GetResource("en").Messages.Keys.Where(key => key.StartsWith("tutorial.")).ToArray();
            Assert.That(keys.Length, Is.GreaterThanOrEqualTo(22));
            foreach (var key in keys) Assert.That(resource[key], Is.Not.Empty, key);
            Assert.That(resource.ContainsKey("tutorial.arrow-hold-intro"), Is.False);
            Assert.That(resource["tutorial.practice"], Does.Contain("\n"));
            Assert.That(resource["tutorial.begin"], Does.Not.Contain("little"));
        }
        [TestCase("en", "<b>Tap</b>", "<b>swipe</b>", "<b>Press and hold</b>", "<b>tap</b>", "<b>Empty</b>")]
        [TestCase("pt-BR", "<b>Toque</b>", "<b>arraste</b>", "<b>Pressione e segure</b>", "<b>toque</b>", "<b>Vazia</b>")]
        [TestCase("es", "<b>Toca</b>", "<b>desliza</b>", "<b>Mantén pulsada</b>", "<b>toca</b>", "<b>Vacía</b>")]
        public void GuidedActionsAndMergedHoldLessonAreLocalizedAndBold(string locale,
            string tap, string swipe, string hold, string followupTap, string empty)
        {
            var messages = GameLocalization.CreateCatalog().GetResource(locale).Messages;
            foreach (var key in new[] { "tutorial.practice", "tutorial.first-paw", "tutorial.second-paw",
                         "tutorial.fourth-paw", "tutorial.single-x", "tutorial.final-paw" })
                Assert.That(messages[key], Does.Contain(tap), key);
            foreach (var key in new[] { "tutorial.zero-column", "tutorial.zero-row",
                         "tutorial.first-column-x", "tutorial.second-row-x" })
            {
                Assert.That(messages[key], Does.Contain(swipe), key);
                Assert.That(messages[key], Does.Contain(empty), "Every swipe explicitly starts Empty: " + key);
            }
            Assert.That(messages["tutorial.diagonal-paw"], Does.Contain(hold));
            Assert.That(messages["tutorial.diagonal-paw"], Does.Contain(followupTap));
            Assert.That(messages["tutorial.diagonal-paw"], Does.Contain("\n"));
        }

        [TestCase("en")][TestCase("pt-BR")][TestCase("es")]
        public void EveryStepHasOnlyItsApprovedLocalizedControlsAndGuidance(string locale)
        {
            var shell = PlayTestFactory.Shell(14, locale);
            shell.StartTutorial();
            var messages = GameLocalization.CreateCatalog().GetResource(locale).Messages;
            foreach (var step in LessonSequence)
            {
                Assert.That(shell.Tutorial.Step, Is.EqualTo(step));
                var helper = shell.Q<Label>("tutorial-continue-guidance");
                var showHelper = step == TutorialStep.Welcome || step == TutorialStep.Goal || step == TutorialStep.StatesIntro;
                Assert.That(Visible(helper), Is.EqualTo(showHelper), step + " helper visibility");
                if (showHelper)
                {
                    Assert.That(helper.text, Is.EqualTo(messages["tutorial.continue-guidance"]));
                    Assert.That(helper.pickingMode, Is.EqualTo(PickingMode.Ignore));
                    Assert.That(helper.focusable, Is.False);
                }
                var heading = shell.BoardView.Q<Label>("tutorial-goal-title");
                Assert.That(Visible(heading), Is.EqualTo(step == TutorialStep.Goal), step + " goal heading visibility");
                if (step == TutorialStep.Goal)
                    Assert.That(heading.text, Is.EqualTo(messages["tutorial.goal-title"]));
                var body = shell.BoardView.Q<Label>("completion-status");
                Assert.That(body.text, Is.EqualTo(messages[shell.Tutorial.MessageKey]), step + " localized body");
                Assert.That(body.text, Does.Not.Contain(messages["tutorial.continue-guidance"]));
                var advance = shell.Q<Button>("action-continue");
                Assert.That(advance != null, Is.EqualTo(shell.Tutorial.CanContinue), step + " Continue availability");
                if (advance != null)
                    Assert.That(advance.text, Is.EqualTo(messages[step == TutorialStep.Begin ? "tutorial.start" : "tutorial.continue"]));
                Assert.That(shell.Q<Button>("action-skip") != null, Is.EqualTo(step != TutorialStep.Complete));
                Assert.That(shell.Q<Button>("action-play") != null, Is.EqualTo(step == TutorialStep.Complete));
                Assert.That(shell.Q<Button>("action-menu"), Is.Not.Null);
                Assert.That(shell.Q<Button>("action-hint"), Is.Null);
                Assert.That(shell.Q<Button>("action-clear"), Is.Null);
                if (step != TutorialStep.Complete) AdvanceLesson(shell);
            }
            shell.SkipTutorial(); shell.StartNewGame();
            Assert.That(Visible(shell.Q<Label>("tutorial-continue-guidance")), Is.False);
            Assert.That(Visible(shell.BoardView.Q<Label>("tutorial-goal-title")), Is.False);
        }

        [Test]
        public void EveryActiveStepSupportsResumeLocaleFocusSkipAndFreshReplay()
        {
            foreach (var step in LessonSequence.Where(step => step != TutorialStep.Complete))
            {
                var shell = PlayTestFactory.Shell(14, "en");
                shell.StartTutorial();
                while (shell.Tutorial.Step != step) AdvanceLesson(shell);
                var tutorial = shell.Tutorial;
                var marks = shell.ActiveSession.Marks.ToArray();
                shell.SetApplicationFocus(false); shell.SetApplicationPaused(true);
                shell.SetApplicationPaused(false); shell.SetApplicationFocus(true);
                shell.ShowMenu(); shell.SelectLanguage("pt-BR"); shell.StartPlaying();
                Assert.That(shell.Tutorial, Is.SameAs(tutorial), step + " resume preserves controller");
                Assert.That(shell.Tutorial.Step, Is.EqualTo(step));
                Assert.That(shell.ActiveSession.Marks, Is.EquivalentTo(marks));
                Assert.That(shell.Locale, Is.EqualTo("pt-BR"));
                shell.SkipTutorial();
                Assert.That(shell.Tutorial, Is.Null, step + " Skip exits tutorial");
                Assert.That(shell.Modal, Is.EqualTo("loading"));
                Assert.That(shell.StartNewGame(), Is.True);
                Assert.That(shell.ActiveSession, Is.SameAs(shell.Loop.Session));
                Assert.That(shell.ActiveSession.Puzzle.GeneratorVersion, Is.Not.EqualTo("tutorial-v1"));
                shell.ShowMenu(); shell.ShowSettings(); shell.StartTutorial();
                Assert.That(shell.Tutorial.Step, Is.EqualTo(TutorialStep.Welcome));
                Assert.That(shell.ActiveSession.Marks.Values, Is.All.EqualTo(PlayerMark.Empty));
            }
        }

        [Test]
        public void SuccessfulHoldSurvivesResumeAndLocaleButReplayRequiresItAgain()
        {
            var shell = PlayTestFactory.Shell(14, "en");
            shell.StartTutorial();
            while (shell.Tutorial.Step != TutorialStep.DiagonalPaw) AdvanceLesson(shell);
            var arrow = new BoardCoordinate(0, 3);
            var paw = new BoardCoordinate(2, 1);
            shell.SetApplicationFocus(false); shell.SetApplicationFocus(true);
            Assert.That(shell.Tutorial.CanTap(paw), Is.False, "Focus cannot grant an unsuccessful hold.");
            Assert.That(shell.Tutorial.RecordArrowHold(arrow), Is.True);
            shell.SetApplicationPaused(true); shell.SetApplicationPaused(false);
            shell.ShowMenu(); shell.SelectLanguage("es"); shell.StartPlaying();
            Assert.That(shell.Tutorial.Step, Is.EqualTo(TutorialStep.DiagonalPaw));
            Assert.That(shell.Tutorial.CanTap(paw), Is.True);
            Assert.That(shell.Q<Button>("action-continue"), Is.Null);
            Assert.That(shell.Q<Label>("tutorial-continue-guidance"), Is.Null);
            shell.StartTutorial();
            while (shell.Tutorial.Step != TutorialStep.DiagonalPaw) AdvanceLesson(shell);
            Assert.That(shell.Tutorial.CanTap(paw), Is.False, "A replay must perform its own hold.");
        }

        [Test] public void SequentialDemoSymbolsAreBoldReadOnlyAndFixedKindsAreSeparate()
        {
            var shell = PlayTestFactory.Shell(14, "en", tutorialSeen: () => true, markTutorialSeen: () => { });
            shell.StartTutorial();
            Assert.That(shell.Q<Button>("action-continue").style.borderLeftWidth.value, Is.EqualTo(3));
            while (shell.Tutorial.Step != TutorialStep.EmptyState) shell.ContinueTutorial();
            foreach (var kind in new[] { BoardCellKind.Empty, BoardCellKind.X, BoardCellKind.Paw })
            {
                Assert.That(shell.BoardView.Q<VisualElement>("cell-" + shell.Tutorial.DemoCoordinate.Value.Row + "-" + shell.Tutorial.DemoCoordinate.Value.Column).Q<BoardSymbolElement>().Kind, Is.EqualTo(kind));
                Assert.That(shell.ActiveSession.Marks.Values, Is.All.EqualTo(PlayerMark.Empty));
                Assert.That(shell.BoardView.InteractionEnabled, Is.True);
                Assert.That(shell.Tutorial.CanTap(new BoardCoordinate(0, 0)), Is.False);
                Assert.That(shell.BoardView.Q<Label>("completion-status").text, Does.Contain("<b>"));
                shell.ContinueTutorial();
            }
            Assert.That(shell.Tutorial.Step, Is.EqualTo(TutorialStep.HatchedCells));
            Assert.That(shell.Tutorial.HighlightedCells, Is.EquivalentTo(shell.ActiveSession.ProtectedCells));
            shell.ContinueTutorial();
            Assert.That(shell.Tutorial.Step, Is.EqualTo(TutorialStep.Arrows));
            Assert.That(shell.Tutorial.HighlightedCells, Is.EquivalentTo(shell.ActiveSession.Puzzle.Arrows.Keys));
            shell.ContinueTutorial();
            Assert.That(shell.ActiveSession.Marks.Values, Is.All.EqualTo(PlayerMark.Empty));
        }
        [Test] public void SkipFailureUsesOrdinaryRetryWithoutRepeatingTutorial()
        {
            var seen = false; var attempts = 0;
            var generator = new SeededEasyPuzzleGenerator();
            var loop = PlayTestFactory.Loop(14, seed => ++attempts == 1 ? null : generator.Generate(DifficultyProfile.Easy, seed));
            var shell = PlayTestFactory.Shell(14, "en", loop, tutorialSeen: () => seen, markTutorialSeen: () => seen = true);
            shell.StartPlaying(); shell.SkipTutorial();
            Assert.That(shell.Tutorial, Is.Null);
            Assert.That(shell.Modal, Is.EqualTo("loading"));
            Assert.That(shell.StartNewGame(), Is.False);
            Assert.That(shell.Screen, Is.EqualTo("failure"));
            Assert.That(shell.Q<Button>("modal-retry"), Is.Not.Null);
            Assert.That(seen, Is.True);
            Assert.That(shell.StartNewGame(), Is.True);
            Assert.That(shell.Tutorial, Is.Null);
            Assert.That(shell.ActiveSession, Is.SameAs(loop.Session));
        }
        [Test] public void RollbackDisablesAutomaticAndManualEntryWithoutWritingPreference()
        {
            var writes = 0;
            var shell = PlayTestFactory.Shell(14, "en", enableTutorial: false, tutorialSeen: () => false, markTutorialSeen: () => writes++);
            shell.ShowSettings();
            Assert.That(shell.Q<Button>("settings-tutorial").enabledSelf, Is.False);
            shell.ShowMenu();
            shell.StartTutorial(); Assert.That(shell.Tutorial, Is.Null);
            shell.StartPlaying(); Assert.That(shell.Modal, Is.EqualTo("loading"));
            Assert.That(writes, Is.Zero);
        }
        private static readonly TutorialStep[] LessonSequence =
        {
            TutorialStep.Welcome, TutorialStep.Goal, TutorialStep.Begin, TutorialStep.StatesIntro,
            TutorialStep.EmptyState, TutorialStep.XState, TutorialStep.PawState, TutorialStep.HatchedCells,
            TutorialStep.Arrows, TutorialStep.Practice, TutorialStep.ZeroColumn, TutorialStep.ZeroRow,
            TutorialStep.FirstPaw, TutorialStep.FirstColumnX, TutorialStep.SecondPaw, TutorialStep.SecondRowX,
            TutorialStep.DiagonalPaw, TutorialStep.FourthPaw, TutorialStep.SingleX, TutorialStep.FinalPaw,
            TutorialStep.Independent, TutorialStep.Complete
        };

        private static bool Visible(VisualElement element) => element != null && element.style.display.value != DisplayStyle.None;

        // Mounted pointer/timing tests separately exercise the gesture recognizers. This
        // replay isolates shell controls, localization and lifecycle at every actual step.
        private static void AdvanceLesson(GameShellView shell)
        {
            var tutorial = shell.Tutorial;
            var previous = tutorial.Step;
            if (tutorial.CanContinue) shell.ContinueTutorial();
            else
            {
                shell.ContinueTutorial();
                Assert.That(tutorial.Step, Is.EqualTo(previous), "Action steps reject Continue.");
                if (previous == TutorialStep.DiagonalPaw)
                    Assert.That(tutorial.RecordArrowHold(new BoardCoordinate(0, 3)), Is.True);
                if (previous == TutorialStep.Practice)
                {
                    foreach (var unused in Enumerable.Range(0, 3))
                        Assert.That(tutorial.Tap(tutorial.Targets[0]), Is.True);
                }
                else
                {
                    var isX = previous == TutorialStep.ZeroColumn || previous == TutorialStep.ZeroRow ||
                        previous == TutorialStep.FirstColumnX || previous == TutorialStep.SecondRowX || previous == TutorialStep.SingleX;
                    var cells = previous == TutorialStep.Independent ? tutorial.Session.Puzzle.SolutionPaws.ToArray() : tutorial.Targets.ToArray();
                    foreach (var cell in cells)
                        while (tutorial.Session.GetMark(cell) != (isX ? PlayerMark.X : PlayerMark.Paw))
                            Assert.That(tutorial.Tap(cell), Is.True);
                }
                Assert.That(tutorial.Step, Is.EqualTo(previous), "No transition before gesture end.");
                Assert.That(tutorial.EndGesture(), Is.True);
                shell.BackToGame();
            }
            Assert.That(tutorial.Step, Is.EqualTo(LessonSequence[Array.IndexOf(LessonSequence, previous) + 1]));
        }

        [Test] public void SeenPreferenceSurvivesReloadAndDirectRandomPrimitiveDoesNotMarkSeen()
        {
            PlayerPrefs.DeleteKey(TutorialPreference.Key);
            Assert.That(TutorialPreference.HasSeen(), Is.False);
            var writes = 0;
            var shell = PlayTestFactory.Shell(14, "en", markTutorialSeen: () => writes++);
            Assert.That(shell.StartNewGame(), Is.True);
            Assert.That(writes, Is.Zero);
            TutorialPreference.MarkSeen();
            Assert.That(TutorialPreference.HasSeen(), Is.True);
        }
    }
}
