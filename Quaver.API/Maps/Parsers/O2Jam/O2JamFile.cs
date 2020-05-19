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
        /// <summary>
        ///     Note file, contains metadata, note data (difficulties) and background image
        /// </summary>
        public OjnParser OjnParser;

        /// <summary>
        ///     Music file, contains the samples of the map (BGM, keysounds)
        /// </summary>
        public OjmParser OjmParser;

        public bool IsValid { get; set; }

        public O2JamFile(string ojnFilePath)
        {
            OjnParser = new OjnParser(ojnFilePath);
            OjnParser.Parse();

            OjmParser = new OjmParser(OjnParser);
            OjmParser.Parse();

            // OjmParser.SaveAudioTo(ojnFilePath);

            IsValid = true;
            foreach (O2JamDifficulty difficulty in Enum.GetValues(typeof(O2JamDifficulty)))
                IsValid &= OjnParser.GetDifficulty(difficulty).IsValid();

        }

        /// <summary>
        ///     Converts all difficulties to Qua objects
        /// </summary>
        /// <returns>List of Qua objects, one for each difficulty</returns>
        /// See <see cref="ToQua(O2JamDifficulty)"/> to convert a single difficulty.
        public List<Qua> ToQua()
        {
            var quaList = new List<Qua>();
            foreach (O2JamDifficulty difficulty in Enum.GetValues(typeof(O2JamDifficulty)))
                ToQua(difficulty);
            return quaList;
        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="difficulty"></param>
        /// <returns>Qua object</returns>
        public Qua ToQua(O2JamDifficulty difficulty)
        {
            var noteChart = OjnParser.GetDifficulty(difficulty);

            // No song preview time
            // No banner
            var qua = new Qua
            {
                Mode = GameMode.Keys7,
                Source = "O2Jam",
                HasScratchKey = false,

                Artist = OjnParser.SongArtist,
                Title = OjnParser.SongTitle,
                Creator = OjnParser.NoteCharter,
                DifficultyName = $"{Enum.GetName(typeof(O2JamDifficulty), difficulty)}, {noteChart.LevelToString()}",
                Tags = Enum.GetName(typeof(O2JamGenre), OjnParser.SongGenre),
                Genre = Enum.GetName(typeof(O2JamGenre), OjnParser.SongGenre),
                Description = $"This is a Quaver converted version of {OjnParser.NoteCharter}'s map."
            };

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
