using Quaver.API.Maps.Parsers.O2Jam.EventPackages;
using System.Collections.Generic;
using System.Linq;

namespace Quaver.API.Maps.Parsers.O2Jam
{
    public class OjnNoteChart
    {
        public O2JamDifficulty Difficulty;
        public int Level { get; set; }
        public int NoteCount { get; set; }
        public int PlayableNoteCount { get; set; }
        public int MeasureCount { get; set; }
        public int PackageCount { get; set; }
        public int Duration { get; set; }
        public int StartingNoteOffset { get; set; }
        public List<O2JamMainPackage> MainPackages { get; set; }

        public OjnNoteChart(O2JamDifficulty difficulty, int level, int noteCount, int playableNoteCount, int measureCount, int packageCount, int duration, int startingNoteOffset, List<O2JamMainPackage> packages)
        {
            Difficulty = difficulty;
            Level = level;
            NoteCount = noteCount;
            PlayableNoteCount = playableNoteCount;
            MeasureCount = measureCount;
            PackageCount = packageCount;
            Duration = duration;
            StartingNoteOffset = startingNoteOffset;
            MainPackages = packages;

        }

        public static string LevelToString(int level) => "Lv. " + level;

        public int[] GetActualPlayableNoteCounts()
        {
            var noteCountArray = new int[7];

            foreach (var mainPackage in MainPackages)
                if (mainPackage.Channel >= 2 && mainPackage.Channel <= 8) // is lane
                    foreach (var eventPackage in mainPackage.EventPackages) // foreach note
                        if (((O2JamNoteEventPackage)eventPackage).IndexIndicator > 0) // 0 = no note
                            noteCountArray[mainPackage.Channel - 2]++;

            return noteCountArray;
        }

        public bool Validate()
        {
            var packageCountOk = PackageCount == MainPackages.Count;

            // Autoplay samples (channel 9-22) are ignored so the count is going to be 0
            var eventCountsOk = MainPackages.TrueForAll(
                p => (p.Channel >= 9 && p.EventPackages.Count == 0) || p.EventCount == p.EventPackages.Count
            );

            var noteCountOk = GetActualPlayableNoteCounts().Sum() == PlayableNoteCount;

            return packageCountOk && eventCountsOk && noteCountOk;
        }

    }
}
