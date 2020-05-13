using Quaver.API.Maps.Parsers.O2Jam.EventPackages;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

        public int GetActualMeasureCount() => MainPackages.Select(mainPackage => mainPackage.Measure).Max();

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

        public void Dump(bool printMeasurementEvents = false, bool printBpmEvents = false, bool printNoteEvents = false)
        {

            var debugString = new StringBuilder();

            foreach (var mainPackage in MainPackages)
            {
                debugString.Append($"M {mainPackage.Measure}, C {mainPackage.Channel}\n");
                foreach (var eventPackage in mainPackage.EventPackages)
                {
                    switch (eventPackage)
                    {
                        case O2JamMeasurementEventPackage measurementEvent:
                            if (printMeasurementEvents)
                                debugString.Append($"    MEA {measurementEvent.Measurement}\n");
                            break;
                        case O2JamBpmEventPackage bpmEvent:
                            if (printBpmEvents)
                                debugString.Append($"    BPM {bpmEvent.Bpm}\n");
                            break;
                        case O2JamNoteEventPackage noteEvent:
                            if (printNoteEvents)
                                debugString.Append($"    INDEX {noteEvent.IndexIndicator}, PAN {noteEvent.PanSound}, VOL {noteEvent.VolumeNote}, TYPE {noteEvent.NoteType}\n");
                            break;
                        default:
                            break;
                    }
                }
            }

            System.IO.File.WriteAllText(@"C:\temp.txt", debugString.ToString());

        }
    }
}
