using Quaver.API.Maps.Structures;
using System;
using System.Collections.Generic;

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

            OjmParser = new OjmParser(OjnParser);

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
            var qua = new Qua
            {
                MapId = -1,
                MapSetId = -1,
                Mode = Enums.GameMode.Keys7,
                Source = "O2Jam",
                HasScratchKey = false,

                Artist = OjnParser.Artist,
                Title = OjnParser.Title,
                Creator = OjnParser.NoteCharter,
                DifficultyName = Enum.GetName(typeof(O2JamDifficulty), difficulty),
                Tags = Enum.GetName(typeof(O2JamGenre), OjnParser.GenreOfSong),
                Genre = Enum.GetName(typeof(O2JamGenre), OjnParser.GenreOfSong),
                Description = $"This is a Quaver converted version of {OjnParser.NoteCharter}'s map."
            };

            // No song preview time
            // No Banner

            // No SVs, only BPM controls SV
            var timingPoints = new List<TimingPointInfo>();
            var hitObjects = new List<HitObjectInfo>();

            // TODO: AudioFile
            var audioFileName = "audio.mp3";
            // TODO: BackgroundFile
            var backgroundFileName = "bg.jpg";

            // TODO: Keysounds
            // List<CustomAudioSampleInfo> CustomAudioSamples;
            // List<SoundEffectInfo> SoundEffects;

            return qua;
        }
    }

}
