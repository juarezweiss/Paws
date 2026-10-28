using System;
using CatsAndArrows.Core;
using CatsAndArrows.Gameplay;
using CatsAndArrows.UI;

namespace CatsAndArrows.Tests
{
    // Profile-specific regressions use finished-intro history (#96).
    // New-player tests explicitly inject an empty store instead.
    internal static class PlayTestFactory
    {
        public static DifficultySelection Selection(string id = "easy", IDifficultyHistoryStore store = null)
        {
            if (store == null)
            {
                store = new MemoryDifficultyHistoryStore();
                store.TrySave(new DifficultyHistory(null, 0));
            }
            return new DifficultySelection(bound => Math.Min(id == "hard" ? 2 : id == "medium" ? 1 : 0, bound - 1), store);
        }
        public static EasyGameLoop Loop(int initialSeed = 14, Func<int, GenerationResult> generate = null,
            IPuzzleGenerator generator = null, DifficultySelection difficultySelection = null) =>
            new EasyGameLoop(initialSeed, generate, generator, difficultySelection ?? Selection());
        public static GameShellView Shell(int initialSeed = 14, string locale = null, EasyGameLoop loop = null,
            bool useInlineHints = true, bool enableTutorial = true, Func<bool> tutorialSeen = null, Action markTutorialSeen = null) =>
            new GameShellView(initialSeed, locale, loop ?? Loop(initialSeed), useInlineHints, enableTutorial,
                tutorialSeen ?? (() => true), markTutorialSeen ?? (() => { }));
    }
}
