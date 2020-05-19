using Quaver.API.Enums;
using Quaver.API.Maps.Parsers.O2Jam.EventPackages;
using Quaver.API.Maps.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
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

            noteChart.ConvertHitObjectsAndBpmsToHelperStructs();
            noteChart.CalculateTimeOffsets();
            noteChart.AddHitObjectsAndTimingPointsToQua(qua);


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
