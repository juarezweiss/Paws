using System.Linq;
using CatsAndArrows.Gameplay;
using CatsAndArrows.UI;
using NUnit.Framework;
using UnityEngine;

namespace CatsAndArrows.Tests
{
    // Issue #43 adapter/migration tests are the only Play tests touching durable
    // preferences. Restore every touched key, including absence, for the owner.
    public sealed class PlayPreferenceTests
    {
        private readonly string[] keys = { LanguagePreference.Key, TutorialPreference.Key,
            FirstPlayPreference.Key, DifficultyHistoryPreference.Key };
        private bool[] existed;
        private string language, history;
        private int seen, played;
        [SetUp] public void PreserveOwnerPreferences()
        {
            existed = keys.Select(PlayerPrefs.HasKey).ToArray();
            language = PlayerPrefs.GetString(LanguagePreference.Key);
            history = PlayerPrefs.GetString(DifficultyHistoryPreference.Key);
            seen = PlayerPrefs.GetInt(TutorialPreference.Key);
            played = PlayerPrefs.GetInt(FirstPlayPreference.Key);
            foreach (var key in keys.Skip(1)) PlayerPrefs.DeleteKey(key);
        }
        [TearDown] public void RestoreOwnerPreferences()
        {
            if (existed[0]) PlayerPrefs.SetString(keys[0], language); else PlayerPrefs.DeleteKey(keys[0]);
            if (existed[1]) PlayerPrefs.SetInt(keys[1], seen); else PlayerPrefs.DeleteKey(keys[1]);
            if (existed[2]) PlayerPrefs.SetInt(keys[2], played); else PlayerPrefs.DeleteKey(keys[2]);
            if (existed[3]) PlayerPrefs.SetString(keys[3], history); else PlayerPrefs.DeleteKey(keys[3]);
            PlayerPrefs.Save();
        }

        [TestCase("easy", 1)] [TestCase("medium", 2)] [TestCase("hard", 2)]
        public void HistoryRoundTripsThroughNewAdapterInstances(string id, int count)
        {
            Assert.That(new DifficultyHistoryPreference().TrySave(new DifficultyHistory(id, count)), Is.True);
            var reloaded = new DifficultyHistoryPreference().Load();
            Assert.That(reloaded.Version, Is.EqualTo(2)); Assert.That(reloaded.LastDifficulty, Is.EqualTo(id));
            Assert.That(reloaded.ConsecutiveStarts, Is.EqualTo(count));
            Assert.That(reloaded.IntroStarts, Is.EqualTo(3));
        }

        [TestCase("")] [TestCase("junk")] [TestCase("1|easy")] [TestCase("1|easy|bad")]
        [TestCase("99|easy|2")] [TestCase("1|other|2")] [TestCase("1|hard|4")]
        public void CorruptOrUnsupportedRecordCannotRestrictFreshSelection(string record)
        {
            PlayerPrefs.SetString(DifficultyHistoryPreference.Key, record);
            var policy = new DifficultySelection(bound => { Assert.That(bound, Is.EqualTo(3)); return 0; },
                new DifficultyHistoryPreference());
            Assert.That(policy.History.LastDifficulty, Is.Null); Assert.That(policy.History.ConsecutiveStarts, Is.Zero);
            Assert.That(policy.IntroStarts, Is.EqualTo(3)); Assert.That(policy.Choose().Id, Is.EqualTo("easy"));
        }

        [Test]
        public void AbsentHistoryAloneEnrollsNewPlayerWithoutWritingOnLoad()
        {
            var store = new DifficultyHistoryPreference();
            Assert.That(store.Load(), Is.Null);
            var policy = new DifficultySelection(_ => { Assert.Fail("intro must not draw randomness"); return 0; }, store);
            Assert.That(policy.IntroStarts, Is.Zero); Assert.That(policy.Choose().Id, Is.EqualTo("easy"));
            Assert.That(PlayerPrefs.HasKey(DifficultyHistoryPreference.Key), Is.False);
            Assert.That(policy.CommitStart(), Is.True);
            Assert.That(PlayerPrefs.GetString(DifficultyHistoryPreference.Key), Is.EqualTo("2|easy|1|1"));
        }

        [TestCase("easy", 1)] [TestCase("easy", 2)] [TestCase("medium", 1)]
        [TestCase("medium", 2)] [TestCase("hard", 1)] [TestCase("hard", 2)]
        public void LegacyPreferencePreservesStreakAndMigratesOnFirstSuccessfulStart(string id, int count)
        {
            var legacy = "1|" + id + "|" + count;
            PlayerPrefs.SetString(DifficultyHistoryPreference.Key, legacy);
            var policy = new DifficultySelection(bound => { Assert.That(bound, Is.EqualTo(count == 2 ? 2 : 3)); return 0; },
                new DifficultyHistoryPreference());
            Assert.That(policy.IntroStarts, Is.EqualTo(3));
            Assert.That(policy.History.LastDifficulty, Is.EqualTo(id));
            Assert.That(policy.History.ConsecutiveStarts, Is.EqualTo(count));
            Assert.That(PlayerPrefs.GetString(DifficultyHistoryPreference.Key), Is.EqualTo(legacy));
            policy.Choose(); Assert.That(policy.CommitStart(), Is.True);
            var reloaded = new DifficultyHistoryPreference().Load();
            Assert.That(reloaded.Version, Is.EqualTo(2)); Assert.That(reloaded.IntroStarts, Is.EqualTo(3));
            Assert.That(reloaded.LastDifficulty, Is.EqualTo(policy.History.LastDifficulty));
            Assert.That(reloaded.ConsecutiveStarts, Is.EqualTo(policy.History.ConsecutiveStarts));
        }

        [TestCase("2|easy|1|1", 1, "easy", 1, "easy")]
        [TestCase("2|easy|2|2", 2, "easy", 2, "medium")]
        [TestCase("2|medium|1|3", 3, "medium", 1, "hard")]
        [TestCase("2|easy|2|-1", 3, "easy", 2, "hard")]
        [TestCase("2|easy|2|4", 3, "easy", 2, "hard")]
        [TestCase("2|easy|2|bad", 3, "easy", 2, "hard")]
        [TestCase("2|easy|2|", 3, "easy", 2, "hard")]
        [TestCase("2|easy|2", 3, "easy", 2, "hard")]
        [TestCase("2|easy|2|999999999999", 3, "easy", 2, "hard")]
        [TestCase("2|easy|2|1", 3, "easy", 2, "hard")]
        [TestCase("2|other|2|1", 3, null, 0, "hard")]
        [TestCase("2|hard|4|1", 3, null, 0, "hard")]
        [TestCase("99|easy|2|1", 3, null, 0, "hard")]
        [TestCase("2|easy|2|2|extra", 3, null, 0, "hard")]
        public void ExtendedPreferencesRestoreIntroOrConservativelyRecover(string record, int intro, string last, int run, string next)
        {
            PlayerPrefs.SetString(DifficultyHistoryPreference.Key, record);
            var policy = new DifficultySelection(bound => bound - 1, new DifficultyHistoryPreference());
            Assert.That(policy.IntroStarts, Is.EqualTo(intro));
            Assert.That(policy.History.LastDifficulty, Is.EqualTo(last));
            Assert.That(policy.History.ConsecutiveStarts, Is.EqualTo(run));
            Assert.That(policy.Choose().Id, Is.EqualTo(next));
            Assert.That(PlayerPrefs.GetString(DifficultyHistoryPreference.Key), Is.EqualTo(record), "load and choose never write");
        }

        [Test]
        public void MissingExtendedCounterPreservesExclusionUntilSuccessfulUpgrade()
        {
            const string damaged = "2|easy|2";
            PlayerPrefs.SetString(DifficultyHistoryPreference.Key, damaged);
            var policy = new DifficultySelection(bound => { Assert.That(bound, Is.EqualTo(2)); return 0; },
                new DifficultyHistoryPreference());
            Assert.That(policy.Choose().Id, Is.EqualTo("medium"));
            Assert.That(PlayerPrefs.GetString(DifficultyHistoryPreference.Key), Is.EqualTo(damaged));
            Assert.That(policy.CommitStart(), Is.True);
            Assert.That(PlayerPrefs.GetString(DifficultyHistoryPreference.Key), Is.EqualTo("2|medium|1|3"));
        }

        [Test]
        public void CorruptPresentHistoryNeverBecomesAbsentAcrossAdapterReconstruction()
        {
            PlayerPrefs.SetString(DifficultyHistoryPreference.Key, "");
            for (var i = 0; i < 2; i++)
            {
                var loaded = new DifficultyHistoryPreference().Load();
                Assert.That(loaded, Is.Not.Null); Assert.That(loaded.IntroStarts, Is.EqualTo(3));
                var policy = new DifficultySelection(_ => 2, new DifficultyHistoryPreference());
                Assert.That(policy.Choose().Id, Is.EqualTo("hard"));
                Assert.That(PlayerPrefs.GetString(DifficultyHistoryPreference.Key), Is.Empty);
            }
        }

        [TestCase(false)] [TestCase(true)]
        public void TutorialSeenOrFirstPlayAloneCannotBypassOrdinaryIntroduction(bool firstPlay)
        {
            if (firstPlay) FirstPlayPreference.MarkPlayed(); else TutorialPreference.MarkSeen();
            Assert.That(FirstPlayPreference.HasPlayed(), Is.True);
            foreach (var expected in new[] { "easy", "easy", "medium" })
            {
                var shell = ShellWithDurableHistory();
                Assert.That(shell.StartNewGame(), Is.True);
                Assert.That(shell.Loop.SessionDifficulty.Id, Is.EqualTo(expected));
            }
        }

        [Test]
        public void OrdinaryHistoryBypassesIntroductionWithoutConsumingFreshFirstPlayGate()
        {
            PlayerPrefs.SetString(DifficultyHistoryPreference.Key, "1|easy|2");
            var policy = new DifficultySelection(_ => 1, new DifficultyHistoryPreference());
            var shell = new GameShellView(14, "en", loop: new EasyGameLoop(14, difficultySelection: policy));
            Assert.That(FirstPlayPreference.HasPlayed(), Is.False);
            shell.StartPlaying(); Assert.That(shell.Tutorial, Is.Not.Null);
            Assert.That(policy.IntroStarts, Is.EqualTo(3)); Assert.That(policy.Pending, Is.Null);
            shell.SkipTutorial(); Assert.That(shell.StartNewGame(), Is.True);
            Assert.That(shell.Loop.SessionDifficulty.Id, Is.EqualTo("hard"));
            Assert.That(policy.History.IntroStarts, Is.EqualTo(3));
        }

        [Test]
        public void RestartMenuAndTutorialKeepSuccessfulOrdinaryStreak()
        {
            var first = ShellWithDurableHistory();
            Assert.That(first.StartNewGame(), Is.True);
            first.ShowMenu(); first.BackToGame();
            Assert.That(first.Loop.DifficultySelection.History.ConsecutiveStarts, Is.EqualTo(1));
            var second = ShellWithDurableHistory();
            Assert.That(second.StartNewGame(), Is.True);
            Assert.That(second.Loop.SessionDifficulty.Id, Is.EqualTo("easy"));
            second.StartTutorial(); second.StartTutorial();
            Assert.That(new DifficultyHistoryPreference().Load().ConsecutiveStarts, Is.EqualTo(2));
            var third = ShellWithDurableHistory();
            Assert.That(third.StartNewGame(), Is.True);
            Assert.That(third.Loop.SessionDifficulty.Id, Is.EqualTo("medium"));
            Assert.That(new DifficultyHistoryPreference().Load().ConsecutiveStarts, Is.EqualTo(1));
        }

        [Test]
        public void FreshManualTutorialDoesNotConsumeFirstPlayAcrossRestart()
        {
            var manual = FreshShell();
            Assert.That(PlayerPrefs.HasKey(FirstPlayPreference.Key), Is.True, "migration freezes before manual entry");
            manual.StartTutorial();
            Assert.That(FirstPlayPreference.HasPlayed(), Is.False);
            // Even legacy writers after initialization cannot change the frozen gate.
            TutorialPreference.MarkSeen();
            var firstPlay = FreshShell(); firstPlay.StartPlaying();
            Assert.That(firstPlay.Tutorial, Is.Not.Null);
            Assert.That(FirstPlayPreference.HasPlayed(), Is.True);
            Assert.That(TutorialPreference.HasSeen(), Is.True, "legacy rollback retains first-Play history");
            Assert.That(firstPlay.Loop.RequestedSeed, Is.Null);
            var nextPlay = FreshShell(); nextPlay.StartPlaying();
            Assert.That(nextPlay.Tutorial, Is.Null); Assert.That(nextPlay.Modal, Is.EqualTo("loading"));
        }

        [Test]
        public void LegacyReturningUserMigratesOnceAndAvoidsRepeatedFirstPlay()
        {
            TutorialPreference.MarkSeen();
            var returning = FreshShell();
            Assert.That(FirstPlayPreference.HasPlayed(), Is.True);
            returning.StartPlaying();
            Assert.That(returning.Tutorial, Is.Null); Assert.That(returning.Modal, Is.EqualTo("loading"));
            PlayerPrefs.DeleteKey(TutorialPreference.Key);
            Assert.That(FirstPlayPreference.HasPlayed(), Is.True);
        }

        [Test]
        public void OrdinaryDirectStartsAndHistoryDoNotConsumeFirstPlayGate()
        {
            var shell = FreshShell(); Assert.That(shell.StartNewGame(), Is.True);
            Assert.That(FirstPlayPreference.HasPlayed(), Is.False);
            var session = shell.ActiveSession;
            shell.ShowMenu(); shell.StartPlaying();
            Assert.That(shell.ActiveSession, Is.SameAs(session));
            Assert.That(shell.Tutorial, Is.Null);
            Assert.That(FirstPlayPreference.HasPlayed(), Is.False);
            var restarted = FreshShell(); restarted.StartPlaying();
            Assert.That(restarted.Tutorial, Is.Not.Null);
            Assert.That(FirstPlayPreference.HasPlayed(), Is.True);
        }

        private static GameShellView FreshShell() => new GameShellView(14, "en",
            loop: PlayTestFactory.Loop(), difficultyHistory: new MemoryDifficultyHistoryStore());
        private static GameShellView ShellWithDurableHistory() => PlayTestFactory.Shell(loop:
            PlayTestFactory.Loop(difficultySelection: new DifficultySelection(_ => 0, new DifficultyHistoryPreference())));
    }
}
