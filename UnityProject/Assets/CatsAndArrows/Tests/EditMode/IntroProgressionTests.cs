using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CatsAndArrows.Core;
using CatsAndArrows.Gameplay;
using CatsAndArrows.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace CatsAndArrows.Tests
{
    // #96: exercise the ordinary admission transaction and existing shell entry points.
    public sealed class IntroProgressionTests
    {
        private bool hadLanguage;
        private string language;
        private GameObject host;
        private PanelSettings settings;
        private RenderTexture texture;

        [SetUp] public void PreserveLanguage()
        { hadLanguage = PlayerPrefs.HasKey(LanguagePreference.Key); language = PlayerPrefs.GetString(LanguagePreference.Key); }

        [TearDown] public void Cleanup()
        {
            if (host != null) UnityEngine.Object.DestroyImmediate(host);
            if (settings != null) UnityEngine.Object.DestroyImmediate(settings);
            if (texture != null) UnityEngine.Object.DestroyImmediate(texture);
            if (hadLanguage) PlayerPrefs.SetString(LanguagePreference.Key, language); else PlayerPrefs.DeleteKey(LanguagePreference.Key);
            PlayerPrefs.Save();
        }

        [Test] public void RealGeneratedAbandonedBoardsFollowIntroAcrossPolicyAndShellRestarts()
        {
            var store = new Store();
            var expected = new[] { "easy", "easy", "medium", "hard" };
            for (var i = 0; i < expected.Length; i++)
            {
                var draws = 0;
                var shell = Shell(store, bound => { draws++; Assert.That(bound, Is.EqualTo(3)); return 2; }, seed: 14 + i);
                Assert.That(shell.Loop.DifficultySelection.IntroStarts, Is.EqualTo(Math.Min(i, 3)));
                Assert.That(shell.StartNewGame(), Is.True, "real seed " + (14 + i));
                Assert.That(shell.Loop.SessionDifficulty.Id, Is.EqualTo(expected[i]));
                Assert.That(shell.ActiveSession.Puzzle.GeneratorVersion, Is.Not.EqualTo("tutorial-v1"));
                Assert.That(shell.ActiveSession.Puzzle.Seed, Is.EqualTo(14 + i));
                Assert.That(shell.ActiveSession.IsCompleted, Is.False, "Abandonment still counts a successful start.");
                Assert.That(store.Value.IntroStarts, Is.EqualTo(Math.Min(i + 1, 3)));
                Assert.That(store.Value.ConsecutiveStarts, Is.EqualTo(i == 1 ? 2 : 1));
                Assert.That(store.Writes, Is.EqualTo(i + 1));
                Assert.That(draws, Is.EqualTo(i < 3 ? 0 : 1));
            }
        }

        [Test] public void FinalHintsAndCompletionNeverCommitAgainAndNextAdvancesOnce()
        {
            var store = new Store(); var shell = Shell(store, _ => 0);
            foreach (var id in new[] { "easy", "easy", "medium" })
            {
                Assert.That(shell.StartNewGame(), Is.True);
                Assert.That(shell.Loop.SessionDifficulty.Id, Is.EqualTo(id));
                var committed = store.Value; var writes = store.Writes;
                for (var i = 0; i < 200 && !shell.ActiveSession.IsCompleted; i++) shell.RequestHint();
                Assert.That(shell.ActiveSession.IsCompleted, Is.True);
                Assert.That(shell.ActiveSession.Tap(shell.ActiveSession.Marks.Keys.First()), Is.False);
                shell.RequestHint(); shell.ClearBoard(); shell.TickCompletion(); shell.BackToCompletedBoard();
                Assert.That(store.Value, Is.SameAs(committed)); Assert.That(store.Writes, Is.EqualTo(writes));
            }
            Assert.That(store.Value.IntroStarts, Is.EqualTo(3));
        }

        [TestCase(false)] [TestCase(true)]
        public void TutorialSkipFromEitherStageAndCompletionPlayShareFreshOrdinaryPolicy(bool independent)
        {
            var store = new Store(); var seen = false;
            var shell = Shell(store, _ => { Assert.Fail("No difficulty draw during introduction."); return 0; },
                tutorialSeen: () => seen, markSeen: () => seen = true);
            shell.StartPlaying();
            Assert.That(seen, Is.True); Assert.That(store.Value, Is.Null);
            var fingerprint = GenerationFingerprints.ForBoard(shell.ActiveSession.Puzzle);
            Assert.That(fingerprint, Is.EqualTo(GenerationFingerprints.ForBoard(TutorialFixture.CreatePuzzle())));
            if (independent) AdvanceTo(shell, TutorialStep.Independent, store);
            Assert.That(shell.Loop.RequestedSeed, Is.Null);
            shell.SkipTutorial(); shell.SkipTutorial();
            Assert.That(store.Writes, Is.Zero);
            Assert.That(shell.StartNewGame(), Is.True);
            Assert.That(shell.Loop.SessionDifficulty.Id, Is.EqualTo("easy"));
            Assert.That(shell.Loop.RequestedSeed, Is.EqualTo(14));
            Assert.That(store.Value.IntroStarts, Is.EqualTo(1)); Assert.That(store.Writes, Is.EqualTo(1));
            Assert.That(shell.StartNewGame(), Is.True); Assert.That(shell.Loop.SessionDifficulty.Id, Is.EqualTo("easy"));
            Assert.That(shell.StartNewGame(), Is.True); Assert.That(shell.Loop.SessionDifficulty.Id, Is.EqualTo("medium"));
        }

        [Test] public void BothTutorialWinsAndSubsequentPlayConsumeOnlyFirstOrdinaryStart()
        {
            var store = new Store(); var shell = Shell(store, _ => 0);
            shell.StartTutorial();
            AdvanceTo(shell, TutorialStep.Independent, store);
            Assert.That(shell.ActiveSession.Marks.Values, Is.All.EqualTo(PlayerMark.Empty));
            AdvanceTo(shell, TutorialStep.Complete, store);
            Assert.That(shell.Q<Button>("action-play"), Is.Not.Null);
            Assert.That(shell.Loop.RequestedSeed, Is.Null); Assert.That(store.Writes, Is.Zero);
            shell.SkipTutorial();
            Assert.That(shell.StartNewGame(), Is.True);
            Assert.That(shell.Loop.SessionDifficulty.Id, Is.EqualTo("easy"));
            Assert.That(store.Value.IntroStarts, Is.EqualTo(1)); Assert.That(store.Writes, Is.EqualTo(1));
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)]
        public void ManualReplayAndRestartKeepCurrentIntroPosition(int starts)
        {
            var store = new Store(Snapshot(starts));
            var shell = Shell(store, _ => 2);
            shell.StartTutorial(); AdvanceTo(shell, TutorialStep.Complete, store);
            Assert.That(store.Writes, Is.Zero); Assert.That(shell.Loop.DifficultySelection.IntroStarts, Is.EqualTo(starts));
            var restarted = Shell(store, _ => 2);
            Assert.That(restarted.Loop.DifficultySelection.IntroStarts, Is.EqualTo(starts));
            restarted.StartTutorial(); restarted.SkipTutorial();
            Assert.That(restarted.StartNewGame(), Is.True);
            Assert.That(restarted.Loop.SessionDifficulty.Id, Is.EqualTo(starts < 2 ? "easy" : starts == 2 ? "medium" : "hard"));
            Assert.That(store.Value.IntroStarts, Is.EqualTo(Math.Min(starts + 1, 3)));
            Assert.That(store.Writes, Is.EqualTo(1));
        }

        [Test] public void MenuResumeLocaleClearHintAndLifecycleDoNotConsumeOrResetProgress()
        {
            var store = new Store(); var shell = Shell(store, _ => 0);
            Assert.That(shell.StartNewGame(), Is.True);
            var session = shell.ActiveSession; var snapshot = store.Value;
            shell.ShowMenu(); shell.StartPlaying();
            Assert.That(shell.ActiveSession, Is.SameAs(session));
            shell.SelectLanguage("es"); shell.BackToGame(); shell.ClearBoard(); shell.RequestHint();
            shell.SetApplicationFocus(false); shell.SetApplicationPaused(true);
            shell.SetApplicationPaused(false); shell.SetApplicationFocus(true);
            Assert.That(shell.ActiveSession, Is.SameAs(session)); Assert.That(store.Value, Is.SameAs(snapshot));
            Assert.That(store.Writes, Is.EqualTo(1));
            Assert.That(shell.StartNewGame(), Is.True);
            Assert.That(shell.Loop.SessionDifficulty.Id, Is.EqualTo("easy"));
            Assert.That(store.Value.IntroStarts, Is.EqualTo(2));
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)]
        public void GenerationFailuresRetainPendingAndOnlyNextSuccessfulSeedCommits(int starts)
        {
            foreach (var failure in new[] { "null", "exhausted", "cancelled", "missing-puzzle", "missing-assessment", "rejected", "wrong-profile", "invalid-puzzle" })
            {
                var original = Snapshot(starts); var store = new Store(original); var draws = 0;
                var policy = new DifficultySelection(_ => { draws++; return 0; }, store);
                EasyGameLoop loop = null; var requests = new List<string>(); var calls = 0;
                loop = new EasyGameLoop(14, seed =>
                {
                    requests.Add(loop.RequestedDifficulty.Id);
                    return ++calls == 1 ? Failed(failure, loop.RequestedDifficulty, seed) :
                        new SeededPuzzleGenerator().Generate(loop.RequestedDifficulty, seed);
                }, difficultySelection: policy);
                Assert.That(loop.NewGame(), Is.False, failure);
                Assert.That(loop.Session, Is.Null); Assert.That(loop.SessionDifficulty, Is.Null);
                Assert.That(store.Value, Is.SameAs(original)); Assert.That(store.Writes, Is.Zero);
                Assert.That(policy.IntroStarts, Is.EqualTo(starts)); var pending = policy.Pending;
                Assert.That(policy.Choose(), Is.SameAs(pending));
                Assert.That(loop.NewGame(), Is.True, failure + " retry");
                Assert.That(loop.RequestedSeed, Is.EqualTo(15)); Assert.That(requests, Is.EqualTo(new[] { pending.Id, pending.Id }));
                Assert.That(store.Value.IntroStarts, Is.EqualTo(Math.Min(starts + 1, 3)));
                Assert.That(store.Writes, Is.EqualTo(1)); Assert.That(draws, Is.EqualTo(starts == 3 ? 1 : 0));
            }
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)]
        public void PersistenceFailureCannotPublishOrPartiallyAdvanceAndRetryCommitsTogether(int starts)
        {
            // 4 means a legacy returning record whose first attempted upgrade fails.
            var original = starts == 4 ? new DifficultyHistory("hard", 2, 1) : Snapshot(starts);
            var store = new Store(original) { CanSave = false }; var draws = 0;
            var policy = new DifficultySelection(_ => { draws++; return 0; }, store);
            var loop = new EasyGameLoop(14, difficultySelection: policy);
            var before = policy.IntroStarts;
            Assert.That(loop.NewGame(), Is.False);
            Assert.That(loop.Session, Is.Null); Assert.That(loop.SessionDifficulty, Is.Null);
            Assert.That(store.Value, Is.SameAs(original)); Assert.That(policy.IntroStarts, Is.EqualTo(before));
            var reconstructed = new DifficultySelection(_ => 0, store);
            Assert.That(reconstructed.IntroStarts, Is.EqualTo(before));
            if (original != null)
            {
                Assert.That(reconstructed.History.LastDifficulty, Is.EqualTo(original.LastDifficulty));
                Assert.That(reconstructed.History.ConsecutiveStarts, Is.EqualTo(original.ConsecutiveStarts));
            }
            var pending = policy.Pending; store.CanSave = true;
            Assert.That(loop.NewGame(), Is.True);
            Assert.That(loop.RequestedSeed, Is.EqualTo(15)); Assert.That(loop.SessionDifficulty, Is.SameAs(pending));
            Assert.That(store.Value.IntroStarts, Is.EqualTo(Math.Min(before + 1, 3)));
            Assert.That(store.Value.LastDifficulty, Is.EqualTo(pending.Id));
            Assert.That(store.Value.ConsecutiveStarts, Is.EqualTo(original?.LastDifficulty == pending.Id ? original.ConsecutiveStarts + 1 : 1));
            Assert.That(store.Value.Version, Is.EqualTo(DifficultyHistory.CurrentVersion));
            Assert.That(store.SuccessfulWrites, Is.EqualTo(1)); Assert.That(draws, Is.EqualTo(before == 3 ? 1 : 0));
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)]
        public void ReentrantGenerationDoesNotConsumeAnotherSeedOrProgressStep(int starts)
        {
            var store = new Store(Snapshot(starts)); var policy = new DifficultySelection(_ => 0, store);
            EasyGameLoop loop = null; var calls = 0;
            loop = new EasyGameLoop(14, seed =>
            {
                calls++; Assert.That(loop.NewGame(), Is.False);
                return new SeededPuzzleGenerator().Generate(loop.RequestedDifficulty, seed);
            }, difficultySelection: policy);
            Assert.That(loop.NewGame(), Is.True);
            Assert.That(calls, Is.EqualTo(1)); Assert.That(loop.RequestedSeed, Is.EqualTo(14));
            Assert.That(store.Writes, Is.EqualTo(1)); Assert.That(store.Value.IntroStarts, Is.EqualTo(Math.Min(starts + 1, 3)));
        }

        [UnityTest] public IEnumerator DuplicateQueuedPlaySkipAndNextEventsAndStaleCallbackCommitOnlyOnce()
        {
            var store = new Store(); var seen = false;
            var shell = Shell(store, _ => 0, tutorialSeen: () => seen, markSeen: () => seen = true);
            yield return Mount(shell);
            shell.StartPlaying(); shell.StartPlaying();
            Assert.That(shell.Tutorial, Is.Not.Null); Assert.That(store.Writes, Is.Zero);
            shell.SkipTutorial(); shell.SkipTutorial(); shell.StartPlaying();
            Assert.That(shell.StartNewGame(), Is.True, "Direct acceptance invalidates the queued callback.");
            yield return DrainScheduledCallbacks(shell);
            Assert.That(store.Writes, Is.EqualTo(1)); Assert.That(shell.NewGameRequestId, Is.EqualTo(1));
            yield return WaitForNextButton(shell);
            var next = shell.Q<Button>("action-new-game"); Click(next); Click(next);
            Assert.That(shell.Modal, Is.EqualTo("loading"), "The first click must queue the command before its duplicate.");
            yield return WaitFor(() => store.Writes >= 2 && shell.Modal == null, "second ordinary admission");
            yield return DrainScheduledCallbacks(shell);
            Assert.That(store.Writes, Is.EqualTo(2)); Assert.That(shell.NewGameRequestId, Is.EqualTo(2));
            Assert.That(store.Value.IntroStarts, Is.EqualTo(2)); Assert.That(shell.Loop.SessionDifficulty.Id, Is.EqualTo("easy"));
            yield return WaitForNextButton(shell);
            Click(shell.Q<Button>("action-new-game"));
            Assert.That(shell.Modal, Is.EqualTo("loading"));
            yield return WaitFor(() => store.Writes >= 3 && shell.Modal == null, "third ordinary admission");
            yield return DrainScheduledCallbacks(shell);
            Assert.That(store.Writes, Is.EqualTo(3)); Assert.That(shell.NewGameRequestId, Is.EqualTo(3));
            Assert.That(shell.Loop.SessionDifficulty.Id, Is.EqualTo("medium"));
            Assert.That(shell.Loop.RequestedSeed, Is.EqualTo(16));
        }

        private static DifficultyHistory Snapshot(int starts) => starts == 0 ? null :
            new DifficultyHistory(starts < 3 ? "easy" : "medium", starts == 2 ? 2 : 1, introStarts: starts);

        private static GameShellView Shell(Store store, Func<int, int> next, int seed = 14,
            Func<bool> tutorialSeen = null, Action markSeen = null) => new GameShellView(seed, "en",
                new EasyGameLoop(seed, difficultySelection: new DifficultySelection(next, store)),
                tutorialSeen: tutorialSeen ?? (() => true), markTutorialSeen: markSeen ?? (() => { }));

        private static void AdvanceTo(GameShellView shell, TutorialStep target, Store store)
        {
            var before = store.Value; var writes = store.Writes;
            for (var i = 0; i < 40 && shell.Tutorial.Step != target; i++)
            {
                var tutorial = shell.Tutorial;
                if (tutorial.CanContinue) shell.ContinueTutorial();
                else
                {
                    if (tutorial.Step == TutorialStep.DiagonalPaw)
                        Assert.That(tutorial.RecordArrowHold(new BoardCoordinate(0, 3)), Is.True);
                    if (tutorial.Step == TutorialStep.Practice)
                        for (var tap = 0; tap < 3; tap++) Assert.That(tutorial.Tap(tutorial.Targets[0]), Is.True);
                    else
                    {
                        var cells = tutorial.Step == TutorialStep.Independent ? tutorial.Session.Puzzle.SolutionPaws.ToArray() : tutorial.Targets.ToArray();
                        foreach (var cell in cells)
                        {
                            var mark = tutorial.Session.Puzzle.IsSolutionPaw(cell) ? PlayerMark.Paw : PlayerMark.X;
                            for (var tap = 0; tap < 3 && tutorial.Session.GetMark(cell) != mark; tap++)
                                Assert.That(tutorial.Tap(cell), Is.True);
                        }
                    }
                    Assert.That(tutorial.EndGesture(), Is.True); shell.BackToGame();
                }
                Assert.That(store.Value, Is.SameAs(before)); Assert.That(store.Writes, Is.EqualTo(writes));
            }
            Assert.That(shell.Tutorial.Step, Is.EqualTo(target));
        }

        private static GenerationResult Failed(string kind, DifficultyProfile profile, int seed)
        {
            if (kind == "null") return null;
            var generator = new SeededPuzzleGenerator();
            if (kind == "exhausted") return generator.Generate(profile, seed, new GenerationOptions(maxAttempts: 0));
            if (kind == "cancelled") return generator.Generate(profile, seed, cancellationToken: new System.Threading.CancellationToken(true));
            if (kind == "wrong-profile") return generator.Generate(profile.Id == "easy" ? DifficultyProfile.Medium : DifficultyProfile.Easy, seed);
            var valid = generator.Generate(profile, seed);
            Assert.That(valid.IsSuccess, Is.True);
            var puzzle = kind == "missing-puzzle" ? null : kind == "invalid-puzzle" ?
                new PuzzleDefinition(profile.Size, seed, "invalid-test", new[] { new BoardCoordinate(-1, -1) }, Array.Empty<ArrowClue>()) : valid.Puzzle;
            var assessment = kind == "missing-assessment" ? null : kind == "rejected" ?
                PuzzleSolvabilityAssessment.Assess(null, profile) : valid.Assessment;
            // Controlled malformed results exercise admission defenses without adding production test seams.
            return (GenerationResult)Activator.CreateInstance(typeof(GenerationResult), BindingFlags.Instance | BindingFlags.NonPublic,
                null, new object[] { GenerationStatus.Success, puzzle, assessment, valid.Metadata, valid.Diagnostics }, null);
        }

        private IEnumerator Mount(GameShellView shell)
        {
            settings = ScriptableObject.CreateInstance<PanelSettings>(); settings.scaleMode = PanelScaleMode.ConstantPixelSize;
            texture = new RenderTexture(390, 844, 0); settings.targetTexture = texture;
            host = new GameObject("issue96-progression", typeof(UIDocument)); host.hideFlags = HideFlags.HideAndDontSave;
            var document = host.GetComponent<UIDocument>(); document.panelSettings = settings;
            document.rootVisualElement.style.width = 390; document.rootVisualElement.style.height = 844;
            document.rootVisualElement.Add(shell);
            yield return WaitFor(() => shell.panel != null && shell.worldBound.width > 0 && shell.worldBound.height > 0,
                "mounted shell layout");
        }

        private static IEnumerator WaitFor(Func<bool> ready, string reason)
        {
            var deadline = System.Diagnostics.Stopwatch.StartNew();
            while (!ready() && deadline.Elapsed.TotalSeconds < 5)
            {
                // EditMode yields run on EditorApplication.update. Runtime UIDocument
                // timers/layout need a player-loop update as well, even in batch mode.
                UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
                yield return null;
            }
            Assert.That(ready(), Is.True, "Timed out waiting for " + reason);
        }

        private static IEnumerator WaitForNextButton(GameShellView shell)
        {
            yield return WaitFor(() =>
            {
                var button = shell.Q<Button>("action-new-game");
                if (shell.Modal != null || button == null || button.panel == null || !button.enabledInHierarchy ||
                    button.worldBound.width <= 0 || button.worldBound.height <= 0) return false;
                var center = button.worldBound.center;
                if (float.IsNaN(center.x) || float.IsNaN(center.y)) return false;
                var picked = shell.panel.Pick(center);
                return picked == button || picked != null && button.Contains(picked);
            }, "current Next board button layout and hit target");
        }

        private static IEnumerator DrainScheduledCallbacks(GameShellView shell)
        {
            // This marker is due after the one-millisecond new-board callback.
            // Wait for the panel scheduler itself, not a guessed number of frames.
            var drained = false;
            shell.schedule.Execute(() => drained = true).StartingIn(10);
            yield return WaitFor(() => drained, "queued callback drain marker");
        }

        private static void Click(Button button)
        {
            var position = button.worldBound.center;
            using (var down = PointerDownEvent.GetPooled(new Event { type = EventType.MouseDown, button = 0, mousePosition = position }))
            { down.target = button; button.SendEvent(down); }
            using (var up = PointerUpEvent.GetPooled(new Event { type = EventType.MouseUp, button = 0, mousePosition = position }))
            { up.target = button; button.SendEvent(up); }
        }

        private sealed class Store : IDifficultyHistoryStore
        {
            public DifficultyHistory Value { get; private set; }
            public int Writes, SuccessfulWrites;
            public bool CanSave = true;
            public Store(DifficultyHistory value = null) { Value = value; }
            public DifficultyHistory Load() => Value;
            public bool TrySave(DifficultyHistory history)
            {
                Writes++; if (!CanSave) return false;
                SuccessfulWrites++; Value = history; return true;
            }
        }
    }
}
