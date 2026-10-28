using System;
using CatsAndArrows.Core;

namespace CatsAndArrows.Gameplay
{
    public sealed class DifficultyHistory
    {
        public const int CurrentVersion = 2;
        public int Version { get; }
        public string LastDifficulty { get; }
        public int ConsecutiveStarts { get; }
        public int IntroStarts { get; }
        public DifficultyHistory(string lastDifficulty, int consecutiveStarts, int version = CurrentVersion, int introStarts = 3)
        { LastDifficulty = lastDifficulty; ConsecutiveStarts = consecutiveStarts; Version = version; IntroStarts = introStarts; }

        // Null alone means no ordinary history. Present invalid/legacy data must
        // never enroll a returning installation in the introduction (#96).
        public static DifficultyHistory Normalize(DifficultyHistory saved)
        {
            if (saved == null) return null;
            var supported = saved.Version == 1 || saved.Version == CurrentVersion;
            var validStreak = supported && saved.ConsecutiveStarts >= 1 && saved.ConsecutiveStarts <= 2 &&
                (saved.LastDifficulty == "easy" || saved.LastDifficulty == "medium" || saved.LastDifficulty == "hard");
            if (!validStreak) return new DifficultyHistory(null, 0);
            var intro = saved.Version == CurrentVersion && saved.IntroStarts >= 0 && saved.IntroStarts <= 3
                ? saved.IntroStarts : 3;
            // A partial introduction has one precise committed streak. Preserve
            // a valid streak but bypass inconsistent introductory metadata.
            if (intro == 0 || intro == 1 && (saved.LastDifficulty != "easy" || saved.ConsecutiveStarts != 1) ||
                intro == 2 && (saved.LastDifficulty != "easy" || saved.ConsecutiveStarts != 2)) intro = 3;
            return saved.Version == CurrentVersion && intro == saved.IntroStarts ? saved :
                new DifficultyHistory(saved.LastDifficulty, saved.ConsecutiveStarts, introStarts: intro);
        }
    }

    public interface IDifficultyHistoryStore
    {
        DifficultyHistory Load();
        bool TrySave(DifficultyHistory history);
    }

    public sealed class MemoryDifficultyHistoryStore : IDifficultyHistoryStore
    {
        private DifficultyHistory history;
        public DifficultyHistory Load() => history;
        public bool TrySave(DifficultyHistory value) { history = value; return true; }
    }

    // Issue #43: selection owns no puzzle generation or Unity state. Failed starts
    // keep their pending choice; only an admitted ordinary session commits a run.
    public sealed class DifficultySelection
    {
        private static readonly DifficultyProfile[] Modes =
            { DifficultyProfile.Easy, DifficultyProfile.Medium, DifficultyProfile.Hard };
        private readonly Func<int, int> nextIndex;
        private readonly IDifficultyHistoryStore store;
        public DifficultyHistory History { get; private set; }
        public int IntroStarts => History?.IntroStarts ?? 0;
        public DifficultyProfile Pending { get; private set; }

        public DifficultySelection(Func<int, int> nextIndex, IDifficultyHistoryStore store = null)
        {
            this.nextIndex = nextIndex ?? throw new ArgumentNullException(nameof(nextIndex));
            this.store = store ?? new MemoryDifficultyHistoryStore();
            History = DifficultyHistory.Normalize(this.store.Load());
        }

        public static DifficultySelection CreateSeeded(int seed, IDifficultyHistoryStore store = null)
        {
            var random = new SplitMix64Random(SplitMix64Random.SeedToState(seed));
            return new DifficultySelection(bound =>
            {
                if (!random.TryNextInt(bound, 128, out var index))
                    throw new InvalidOperationException("Difficulty random draw budget exhausted.");
                return index;
            }, store);
        }

        public DifficultyProfile Choose()
        {
            if (Pending != null) return Pending;
            if (IntroStarts < 3)
            {
                Pending = IntroStarts < 2 ? DifficultyProfile.Easy : DifficultyProfile.Medium;
                return Pending;
            }
            var excluded = History?.ConsecutiveStarts == 2 ? History.LastDifficulty : null;
            var eligible = Array.FindAll(Modes, mode => mode.Id != excluded);
            var index = nextIndex(eligible.Length);
            if (index < 0 || index >= eligible.Length)
                throw new InvalidOperationException("Random choice must be an eligible index.");
            Pending = eligible[index];
            return Pending;
        }

        public bool CommitStart()
        {
            if (Pending == null) return false;
            var count = History?.LastDifficulty == Pending.Id ? History.ConsecutiveStarts + 1 : 1;
            var committed = new DifficultyHistory(Pending.Id, count, introStarts: Math.Min(3, IntroStarts + 1));
            if (!store.TrySave(committed)) return false;
            History = committed;
            Pending = null;
            return true;
        }
    }
}
