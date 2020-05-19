using Quaver.API.Maps.Parsers.O2Jam.EventPackages;
using Quaver.API.Maps.Structures;
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

        public List<O2JamNote> Notes;
        public List<O2JamBpm> Bpms;
        public const int MAX_SNAP_DIVISOR = 192;

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

            // Autoplay samples (channel 9-22) are ignored, so the count is going to be 0
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

        public void ConvertHitObjectsAndBpmsToHelperStructs()
        {
            Notes = new List<O2JamNote>();
            Bpms = new List<O2JamBpm>();
            foreach (var mainPackage in MainPackages)
            {
                var eventPackageNumber = 0;
                foreach (var eventPackage in mainPackage.EventPackages)
                {
                    if (eventPackage.IsNonZero())
                    {
                        var snapPosition = mainPackage.Measure * MAX_SNAP_DIVISOR + eventPackageNumber * MAX_SNAP_DIVISOR / mainPackage.EventPackages.Count();
                        switch (eventPackage)
                        {
                            case O2JamBpmEventPackage bpmEvent:
                                Bpms.Add(new O2JamBpm
                                {
                                    SnapPosition = snapPosition,
                                    BpmValue = bpmEvent.Bpm
                                });
                                break;
                            case O2JamNoteEventPackage noteEvent:
                                Notes.Add(new O2JamNote
                                {
                                    SnapPosition = snapPosition,
                                    Lane = mainPackage.Channel - 1,
                                    NoteType = noteEvent.NoteType,
                                    IndexIndicator = noteEvent.IndexIndicator
                                });
                                break;
                            default:
                                break;
                        }
                    }

                    eventPackageNumber++;
                }
            }

            Notes.OrderBy(x => x.SnapPosition);
            Bpms.OrderBy(x => x.SnapPosition);
        }

        public void CalculateTimeOffsets()
        {
            var referenceTime = TimeSpan.Zero;
            var bpmIndex = 0;

            for (var i = 0; i < Notes.Count(); i++)
            {
                var note = Notes[i];
                while (bpmIndex + 1 < Bpms.Count() && note.SnapPosition > Bpms[bpmIndex + 1].SnapPosition)
                {
                    referenceTime += TimeSpan.FromSeconds((Bpms[bpmIndex + 1].SnapPosition - Bpms[bpmIndex].SnapPosition) / (Bpms[bpmIndex].BpmValue * 0.8));
                    bpmIndex++;
                    var bpm = Bpms[bpmIndex];
                    bpm.Time = referenceTime;
                    Bpms[bpmIndex] = bpm;
                }

                note.Time = TimeSpan.FromSeconds((note.SnapPosition - Bpms[bpmIndex].SnapPosition) / (Bpms[bpmIndex].BpmValue * 0.8)) + referenceTime;
                Notes[i] = note;
            }
        }

        public void AddHitObjectsAndTimingPointsToQua(Qua qua)
        {
            foreach (var bpm in Bpms)
            {
                qua.TimingPoints.Add(new TimingPointInfo()
                {
                    Bpm = bpm.BpmValue,
                    StartTime = (float)bpm.Time.TotalMilliseconds
                });
            }

            foreach (var note in Notes)
            {
                var time = (int)Math.Round(note.Time.TotalMilliseconds, MidpointRounding.AwayFromZero);
                switch (note.NoteType)
                {
                    case O2JamNoteType.NormalNote:
                    case O2JamNoteType.StartLongNote:
                        qua.HitObjects.Add(new HitObjectInfo
                        {
                            StartTime = time,
                            Lane = note.Lane
                        });
                        break;
                    case O2JamNoteType.EndLongNote:
                        qua.HitObjects.FindLast(x => x.Lane == note.Lane).EndTime = time;
                        break;
                    case O2JamNoteType.BgmNote:
                        break;
                    default:
                        break;
                }
            }
        }

        public struct O2JamNote
        {
            public int SnapPosition; // in a 1/192 grid
            public int Lane;
            public O2JamNoteType NoteType;
            public TimeSpan Time;
            public int IndexIndicator;
        }

        public struct O2JamBpm
        {
            public int SnapPosition; // in a 1/192 grid
            public float BpmValue;
            public TimeSpan Time;
        }
    }
}
