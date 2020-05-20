using Quaver.API.Maps.Parsers.O2Jam.EventPackages;
using System;
using System.Collections.Generic;
using System.IO;

namespace Quaver.API.Maps.Parsers.O2Jam
{
    /// <summary>
    ///     Note file, contains metadata, note data (difficulties) and background image
    /// </summary>
    /// <remarks>OJn -> O2Jam Note</remarks>
    public class OjnParser
    {
        private ByteDecoder decoder;
        private readonly FileStream stream;

        public OjnParser(string filePath)
        {
            OriginalDirectory = Path.GetDirectoryName(filePath);
            stream = new FileStream(filePath, FileMode.Open);
        }

        private readonly int numberOfDifficulties = Enum.GetNames(typeof(O2JamDifficulty)).Length; // Easy, Normal, Hard

        public string OriginalDirectory { get; set; }

        /// <summary>
        ///     Song ID, usually the same ID in the name of the file "o2ma###.ojn"
        /// </summary>
        public int SongID { get; set; }

        /// <summary>
        ///     File identifier, content should always be "ojn\0"
        /// </summary>
        public string FileSignature { get; set; }

        /// <summary>
        ///     OJN encoder version, usually 2.9f
        /// </summary>
        public byte[] OjnEncoderVersion { get; set; }

        /// <summary>
        ///     Song genre
        /// </summary>
        public O2JamGenre SongGenre { get; set; }

        /// <summary>
        ///     Songs general BPM. Doesn't actually affect the file itself when changed.
        /// </summary>
        public byte[] SongBPM { get; set; }

        /// <summary>
        ///     Unused field used to pad the binary file for alignment.
        /// </summary>
        public short EmptyField { get; set; }

        /// <summary>
        ///     Unused version number
        /// </summary>
        public short OldEncodeVersion { get; set; }

        /// <summary>
        ///     Unused song ID
        /// </summary>
        public short OldSongID { get; set; }

        /// <summary>
        ///     Unused genre
        /// </summary>
        public string OldGenre { get; set; }

        /// <summary>
        ///     Bitmap image size. The bitmap serves as a small 8x8 thumbnail image.
        /// </summary>
        public int SizeOfBMPImage { get; set; }

        /// <summary>
        ///     Unused file version.
        /// </summary>
        public int OldFileVersion { get; set; }

        /// <summary>
        ///     Song title
        /// </summary>
        public string SongTitle { get; set; }

        /// <summary>
        ///     Song artist
        /// </summary>
        public string SongArtist { get; set; }

        /// <summary>
        ///     Charter/Mapper/Creator
        /// </summary>
        public string NoteCharter { get; set; }

        /// <summary>
        ///     The path to the .ojm file corresponding to the .ojn file, usually "o2ma###.ojm".
        /// </summary>
        public string OjmFilePath { get; set; }

        /// <summary>
        ///     Size of the background file
        /// </summary>
        public int SizeOfJPGFile { get; set; }

        /// <summary>
        ///     Byte offset of the background image.
        /// </summary>
        public int ImageByteOffset { get; set; }

        /// <summary>
        ///     List of all difficulties (usually Easy, Normal, Hard)
        /// </summary>
        public List<OjnNoteChart> Difficulties { get; set; }

        public void Parse()
        {
            decoder = new ByteDecoder(stream);

            SongID = decoder.ReadInt();
            FileSignature = decoder.ReadString(4);
            OjnEncoderVersion = decoder.ReadBytes(4);
            SongGenre = (O2JamGenre)decoder.ReadInt();
            SongBPM = decoder.ReadBytes(4);
            var levels = decoder.ReadArray(decoder.ReadShort, numberOfDifficulties);
            EmptyField = decoder.ReadShort();
            var noteCounts = decoder.ReadArray(decoder.ReadInt, numberOfDifficulties);
            var playableNoteCounts = decoder.ReadArray(decoder.ReadInt, numberOfDifficulties);
            var measureCounts = decoder.ReadArray(decoder.ReadInt, numberOfDifficulties);
            var packageCounts = decoder.ReadArray(decoder.ReadInt, numberOfDifficulties);
            OldEncodeVersion = decoder.ReadShort();
            OldSongID = decoder.ReadShort();
            OldGenre = decoder.ReadString(20);
            SizeOfBMPImage = decoder.ReadInt();
            OldFileVersion = decoder.ReadInt();
            SongTitle = decoder.ReadString(64);
            SongArtist = decoder.ReadString(32);
            NoteCharter = decoder.ReadString(32);
            OjmFilePath = decoder.ReadString(32);
            SizeOfJPGFile = decoder.ReadInt();
            var durations = decoder.ReadArray(decoder.ReadInt, numberOfDifficulties);
            var startingNoteOffsets = decoder.ReadArray(decoder.ReadInt, numberOfDifficulties);
            ImageByteOffset = decoder.ReadInt();

            Difficulties = new List<OjnNoteChart>();

            for (var difficulty = 0; difficulty < numberOfDifficulties; difficulty++)
                Difficulties.Add(ParseDifficulty(
                    (O2JamDifficulty)difficulty,
                    levels[difficulty],
                    noteCounts[difficulty],
                    playableNoteCounts[difficulty],
                    measureCounts[difficulty],
                    packageCounts[difficulty],
                    durations[difficulty],
                    startingNoteOffsets[difficulty]
                ));

            stream.Close();
        }

        private OjnNoteChart ParseDifficulty(O2JamDifficulty difficulty, int level, int noteCount, int playableNoteCount, int measureCount, int packageCount, int duration, int startingNoteOffset)
        {
            stream.Position = startingNoteOffset; // It should technically already be at that position but you can't be sure enough
            var mainPackages = new List<O2JamMainPackage>();

            var counter = packageCount;

            while (counter > 0)
            {
                var measure = decoder.ReadInt();
                var channel = decoder.ReadShort();
                var eventCount = decoder.ReadShort();
                var eventPackages = new List<O2JamEventPackage>();

                for (var eventPackage = 0; eventPackage < eventCount; eventPackage++)
                {
                    // Every kind of event package has a total size of 4 bytes.
                    switch (channel)
                    {
                        // Measurement Event
                        case 0:
                            var measurement = decoder.ReadSingle(); // 4 bytes
                            eventPackages.Add(new O2JamMeasurementEventPackage(measurement));
                            break;

                        // BPM Event
                        case 1:
                            var bpm = decoder.ReadSingle(); // 4 bytes
                            eventPackages.Add(new O2JamBpmEventPackage(bpm));
                            break;

                        // Note event (channel number specifies the lane)
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                            var sampleIndex = decoder.ReadShort();                          // 2 bytes
                            var soundPanAndSoundVolume = decoder.ReadBytes(1)[0];
                            var soundPan = (byte)(soundPanAndSoundVolume >> 4);             // half a byte
                            var soundVolume = (byte)(soundPanAndSoundVolume & 0b00001111);  // half a byte
                            var noteType = decoder.ReadBytes(1)[0];                         // 1 byte
                            eventPackages.Add(new O2JamNoteEventPackage(sampleIndex, soundPan, soundVolume, noteType));
                            break;

                        // 9-22 are some kind of "auto-play sample notes", which I will ignore
                        default:
                            decoder.ReadBytes(4); // 4 bytes
                            break;
                    }
                }

                mainPackages.Add(new O2JamMainPackage
                {
                    Measure = measure,
                    Channel = channel,
                    EventCount = eventCount,
                    EventPackages = eventPackages
                });

                counter--;
            }

            return new OjnNoteChart(
                difficulty,
                level,
                noteCount,
                playableNoteCount,
                measureCount,
                packageCount,
                duration,
                startingNoteOffset,
                mainPackages
            ); ;
        }

        /// <summary>
        /// </summary>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        public OjnNoteChart GetDifficulty(O2JamDifficulty difficulty) => Difficulties[(int)difficulty];
    }
}
