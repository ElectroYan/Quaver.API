using Quaver.API.Enums;
using Quaver.API.Maps.Parsers.O2Jam.EventPackages;
using Quaver.API.Maps.Structures;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quaver.API.Maps.Parsers.O2Jam
{
    public class O2JamFile
    {
        public OjnParser OjnParser; // Note file, contains metadata, note data (difficulties) and background image
        public OjmParser OjmParser; // Music file, contains the samples of the map (BGM, keysounds)
        public bool IsValid { get; set; }

        public const int MAX_SNAP_DIVISOR = 192;

        public O2JamFile(string ojnFilePath)
        {
            OjnParser = new OjnParser(ojnFilePath);
            OjnParser.Parse();

            OjmParser = new OjmParser(OjnParser);
            OjmParser.Parse();

            // OjmParser.SaveAudioTo(ojnFilePath);

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

            // No song preview time
            // No banner
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

            var currentBpm = noteChart.GetNthEventPackageOfType<O2JamBpmEventPackage>(1).Bpm;

            var currentOffset = 0.0f;
            var currentMeasurementFactor = 1.0f;

            for (var measure = 0; measure < noteChart.GetActualMeasureCount(); measure++)
            {
                for (var snap = 0; snap < MAX_SNAP_DIVISOR; snap++)
                {
                    var eventPackages = noteChart.GetEventPackagesOfTypeInMeasure<O2JamEventPackage>(measure, false, snap, MAX_SNAP_DIVISOR);
                    foreach (var eventPackage in eventPackages)
                    {
                        if (!eventPackage.IsNonZero())
                            continue;

                        switch (eventPackage)
                        {
                            case O2JamMeasurementEventPackage measurementEvent:
                                currentMeasurementFactor = measurementEvent.Measurement;
                                break;

                            case O2JamBpmEventPackage bpmEvent:
                                currentBpm = bpmEvent.Bpm;
                                qua.TimingPoints.Add(new TimingPointInfo()
                                {
                                    StartTime = currentOffset,
                                    Bpm = bpmEvent.Bpm
                                });
                                break;

                            case O2JamNoteEventPackage noteEvent:
                                var roundedOffset = (int)Math.Round(currentOffset, MidpointRounding.AwayFromZero);
                                var lane = noteEvent.Channel - 1;
                                switch (noteEvent.NoteType)
                                {
                                    case O2JamNoteType.NormalNote:
                                    case O2JamNoteType.StartLongNote:
                                        qua.HitObjects.Add(new HitObjectInfo()
                                        {
                                            StartTime = roundedOffset,
                                            Lane = lane
                                        });
                                        break;
                                    case O2JamNoteType.EndLongNote:
                                        qua.HitObjects.FindLast(x => x.Lane == lane).EndTime = roundedOffset;
                                        break;
                                    case O2JamNoteType.BgmNote:
                                        break;
                                    default:
                                        break;
                                }
                                break;

                            default:
                                break;
                        }
                    }

                    var msPerBeat = 60000 * 4 / (currentBpm * MAX_SNAP_DIVISOR);
                    currentOffset += msPerBeat * currentMeasurementFactor;
                }
            }

            //// TODO: AudioFile
            //var audioFileName = "audio.mp3";
            //// TODO: BackgroundFile
            //var backgroundFileName = "bg.jpg";

            // TODO: Keysounds
            // List<CustomAudioSampleInfo> CustomAudioSamples;
            // List<SoundEffectInfo> SoundEffects;

            return qua;
        }
    }
}
