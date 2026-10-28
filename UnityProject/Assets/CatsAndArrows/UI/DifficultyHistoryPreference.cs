using System.Globalization;
using CatsAndArrows.Gameplay;
using UnityEngine;

namespace CatsAndArrows.UI
{
    public sealed class DifficultyHistoryPreference : IDifficultyHistoryStore
    {
        public const string Key = "cats-arrows.difficulty-history.v1";
        public DifficultyHistory Load()
        {
            if (!PlayerPrefs.HasKey(Key)) return null;
            var fields = PlayerPrefs.GetString(Key, "").Split('|');
            if (fields.Length < 3 || !int.TryParse(fields[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var version) ||
                !int.TryParse(fields[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var count) ||
                !(version == 1 && fields.Length == 3 || version == DifficultyHistory.CurrentVersion && fields.Length <= 4))
                return new DifficultyHistory(null, 0);
            var intro = 3;
            if (version == DifficultyHistory.CurrentVersion && fields.Length == 4 &&
                !int.TryParse(fields[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out intro)) intro = 3;
            return DifficultyHistory.Normalize(new DifficultyHistory(fields[1], count, version, intro));
        }
        public bool TrySave(DifficultyHistory history)
        {
            history = DifficultyHistory.Normalize(history);
            if (history == null) return false;
            var existed = PlayerPrefs.HasKey(Key);
            var previous = PlayerPrefs.GetString(Key);
            try
            {
                PlayerPrefs.SetString(Key, history.Version.ToString(CultureInfo.InvariantCulture) + "|" + history.LastDifficulty + "|" +
                    history.ConsecutiveStarts.ToString(CultureInfo.InvariantCulture) + "|" +
                    history.IntroStarts.ToString(CultureInfo.InvariantCulture));
                PlayerPrefs.Save();
                return true;
            }
            catch (PlayerPrefsException)
            {
                // Keep the in-memory preference at its previous committed value
                // too, so reconstructing the shell cannot consume a failed start.
                try
                {
                    if (existed) PlayerPrefs.SetString(Key, previous); else PlayerPrefs.DeleteKey(Key);
                    PlayerPrefs.Save();
                }
                catch (PlayerPrefsException) { }
                return false;
            }
        }
    }
}
