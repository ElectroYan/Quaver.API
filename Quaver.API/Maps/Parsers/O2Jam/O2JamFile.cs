using Quaver.API.Enums;
using Quaver.API.Maps.Parsers.O2Jam.EventPackages;
using Quaver.API.Maps.Structures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quaver.API.Maps.Parsers.O2Jam
{
    public class O2JamFile
    {
        public OjnParser OjnParser; // Note file, contains metadata, note data (difficulties) and background image
        public OjmParser OjmParser; // Music file, contains the samples of the map (BGM, keysounds)
        public bool IsValid { get; set; }

        public O2JamFile(string ojnFilePath)
        {
            OjnParser = new OjnParser(ojnFilePath);
            OjnParser.Parse();

            //OjmParser = new OjmParser(OjnParser);

            IsValid = true;
            foreach (O2JamDifficulty difficulty in Enum.GetValues(typeof(O2JamDifficulty)))
                IsValid &= OjnParser.GetDifficulty(difficulty).Validate();

        }

        public List<Qua> ToQua()
        {
            var quaList = new List<Qua>();
            foreach (O2JamDifficulty difficulty in Enum.GetValues(typeof(O2JamDifficulty)))
                ToQua(difficulty);
            return quaList;
        }

        public Qua ToQua(O2JamDifficulty difficulty)
        {
            var noteChart = OjnParser.GetDifficulty(difficulty);
            noteChart.Dump(true, true, true);

            // No song preview time
            // No Banner
            var qua = new Qua
            {
                MapId = -1,
                MapSetId = -1,
                Mode = GameMode.Keys7,
                Source = "O2Jam",
                HasScratchKey = false,

                Artist = OjnParser.Artist,
                Title = OjnParser.Title,
                Creator = OjnParser.NoteCharter,
                DifficultyName = $"{Enum.GetName(typeof(O2JamDifficulty), difficulty)}, Lv. {noteChart.Level}",
                Tags = Enum.GetName(typeof(O2JamGenre), OjnParser.GenreOfSong),
                Genre = Enum.GetName(typeof(O2JamGenre), OjnParser.GenreOfSong),
                Description = $"This is a Quaver converted version of {OjnParser.NoteCharter}'s map."
            };

            var bpmChanges = new List<BpmMeasure>();
            var noteChanges = new List<NoteMeasure>();
            var measurementChanges = new List<MeasurementMeasure>();

            ConvertMainPackages(noteChart, bpmChanges, noteChanges, measurementChanges);

            //// TODO: AudioFile
            //var audioFileName = "audio.mp3";
            //// TODO: BackgroundFile
            //var backgroundFileName = "bg.jpg";

            // TODO: Keysounds
            // List<CustomAudioSampleInfo> CustomAudioSamples;
            // List<SoundEffectInfo> SoundEffects;

            return qua;
        }

        private static void ConvertMainPackages(OjnNoteChart noteChart, List<BpmMeasure> bpmChanges, List<NoteMeasure> noteChanges, List<MeasurementMeasure> measurementChanges)
        {
            foreach (var mainPackage in noteChart.MainPackages)
            {
                if (mainPackage.EventPackages.Count == 0)
                    continue;

                var snapDivisor = mainPackage.EventPackages.Count;

                for (var i = 0; i < snapDivisor; i++)
                {
                    switch (mainPackage.EventPackages[0])
                    {
                        case O2JamMeasurementEventPackage _:
                            var measurementEvent = (O2JamMeasurementEventPackage)mainPackage.EventPackages[i];
                            if (measurementEvent.Measurement == 0)
                                continue;
                            measurementChanges.Add(new MeasurementMeasure
                            {
                                MeasurementFactor = measurementEvent.Measurement,
                                Measure = mainPackage.Measure,
                                SnapNumerator = i,
                                SnapDenominator = snapDivisor
                            });
                            break;
                        case O2JamBpmEventPackage _:
                            var bpmEvent = (O2JamBpmEventPackage)mainPackage.EventPackages[i];
                            if (bpmEvent.Bpm == 0)
                                continue;
                            bpmChanges.Add(new BpmMeasure
                            {
                                Bpm = bpmEvent.Bpm,
                                Measure = mainPackage.Measure,
                                SnapNumerator = i,
                                SnapDenominator = snapDivisor
                            });
                            break;
                        case O2JamNoteEventPackage _:
                            var noteEvent = (O2JamNoteEventPackage)mainPackage.EventPackages[i];
                            if (noteEvent.IndexIndicator == 0)
                                continue;
                            noteChanges.Add(new NoteMeasure
                            {
                                Lane = mainPackage.Channel - 1,
                                SampleIndex = noteEvent.IndexIndicator,
                                NoteType = noteEvent.NoteType,
                                Measure = mainPackage.Measure,
                                SnapNumerator = i,
                                SnapDenominator = snapDivisor
                            });
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private struct BpmMeasure
        {
            public float Bpm;
            public int Measure;
            public int SnapNumerator;
            public int SnapDenominator;
        }

        private struct NoteMeasure
        {
            public int Lane;
            public short SampleIndex;
            public O2JamNoteType NoteType;
            public int Measure;
            public int SnapNumerator;
            public int SnapDenominator;
        }

        private struct MeasurementMeasure
        {
            public float MeasurementFactor;
            public int Measure;
            public int SnapNumerator;
            public int SnapDenominator;
        }
    }
}
