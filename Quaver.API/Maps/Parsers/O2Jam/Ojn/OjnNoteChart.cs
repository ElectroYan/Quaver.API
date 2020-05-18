using Quaver.API.Maps.Parsers.O2Jam.EventPackages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public bool Validate()
        {
            var packageCountOk = PackageCount == MainPackages.Count;

            // Autoplay samples (channel 9-22) are ignored so the count is going to be 0
            var eventCountsOk = MainPackages.TrueForAll(
                p =>
                    p.EventCount == p.EventPackages.Count
                    || (p.Channel >= 9 && p.EventPackages.Count == 0)
            );

            var noteCountOk = GetActualPlayableNoteCountPerLane().Sum() == PlayableNoteCount;

            return packageCountOk && eventCountsOk && noteCountOk;
        }

        /// <summary>
        ///     The note count in the provided file could be edited or incorrect,
        ///     so this returns the true note count, regardless of what's provided in the .ojn file
        /// </summary>
        /// <returns>An array of all note counts in their respective lanes (0 = lane 1, 1 = lane 2...)</returns>
        public int[] GetActualPlayableNoteCountPerLane()
        {
            var noteCountArray = new int[7];

            foreach (var mainPackage in MainPackages)
                if (mainPackage.Channel >= 2 && mainPackage.Channel <= 8) // is lane
                    foreach (var eventPackage in mainPackage.EventPackages) // foreach note
                        if (eventPackage.IsNonZero())
                            noteCountArray[mainPackage.Channel - 2]++;

            return noteCountArray;
        }

        /// <summary>
        ///     The measure count in the provided file could be edited or incorrect,
        ///     so this returns the true measure count, regardless of what's provided in the .ojn file
        /// </summary>
        /// <returns>The maximum measure of the provided .ojn file</returns>
        public int GetActualMeasureCount() => MainPackages.Select(mainPackage => mainPackage.Measure).Max();

        public T GetNthEventPackageOfType<T>(int n, bool mustBeNonZero = true)
            where T : O2JamEventPackage
        {
            var count = 0;
            foreach (var mainPackage in MainPackages)
            {
                foreach (var eventPackage in mainPackage.EventPackages)
                {
                    if (eventPackage is T && (!mustBeNonZero || eventPackage.IsNonZero()))
                        count++;
                    if (count == n)
                        return (T)eventPackage;
                }
            }

            return null;
        }

        /// <summary>
        ///     Gets all event packages in a particular measure.
        /// </summary>
        /// <typeparam name="T">Should be an O2JamEventPackage</typeparam>
        /// <param name="measure">Measure to look in</param>
        /// <param name="mustBeNonZero">Include all non-zero (buffer) elements?</param>
        /// <param name="snapNumerator">Optional to match packages at an exact snap, otherwise returns all packages in a measure</param>
        /// <param name="snapDenominator">Optional to match packages at an exact snap, otherwise returns all packages in a measure</param>
        /// <returns>List of all valid event packages, null if none are found</returns>
        public List<T> GetEventPackagesOfTypeInMeasure<T>(int measure, bool mustBeNonZero = true, int snapNumerator = -1, int snapDenominator = -1)
            where T : O2JamEventPackage
        {
            var snapNeeded = !(snapNumerator == -1 && snapDenominator == -1);
            var currentSnapRatio = snapNumerator / snapDenominator;
            var validEventPackages = new List<T>();
            foreach (var mainPackage in MainPackages.Where(x => x.Measure == measure))
            {
                var eventPackageNumber = 0.0f;
                var count = (float)mainPackage.EventPackages.Count();
                foreach (var eventPackage in mainPackage.EventPackages)
                {
                    var isCorrectType = eventPackage is T;
                    var nonZero = !mustBeNonZero || eventPackage.IsNonZero();

                    var eventSnapRatio = eventPackageNumber / count;
                    var snapIsCorrect = !snapNeeded || (currentSnapRatio == eventSnapRatio);

                    if (isCorrectType && nonZero && snapIsCorrect)
                        validEventPackages.Add((T)eventPackage);

                    eventPackageNumber++;
                }
            }
            return validEventPackages;
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
