using Quaver.API.Maps.Parsers.O2Jam.EventPackages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Quaver.API.Maps.Parsers.O2Jam
{
    public class OjnParser
    {
        private readonly FileStream stream;
        public OjnParser(string filePath) => stream = new FileStream(filePath, FileMode.Open);

        private ByteDecoder decoder;

        public const int NUMBER_OF_DIFFICULTIES = 3; // Easy, Normal, Hard

        public int IDSong { get; set; }
        public string FileSignature { get; set; }
        public byte[] EncodeOJNVersion { get; set; }
        public O2JamGenre GenreOfSong { get; set; }
        public byte[] BpmSong { get; set; }
        public short EmptyField { get; set; }
        public short OldEncodeVersion { get; set; }
        public short OldSongID { get; set; }
        public string OldGenre { get; set; }
        public int SizeOfBMPImage { get; set; }
        public int OldFileVersion { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Notecharter { get; set; }
        public string OjmFile { get; set; }
        public int SizeOfJPGFile { get; set; }
        public int ImageOffset { get; set; }
        public List<OjnNoteChart> Difficulties { get; set; }

        public void Parse()
        {
            decoder = new ByteDecoder(stream);

            IDSong = decoder.ReadInt();
            FileSignature = decoder.ReadString(4);
            EncodeOJNVersion = decoder.ReadBytes(4);
            GenreOfSong = (O2JamGenre)decoder.ReadInt();
            BpmSong = decoder.ReadBytes(4);

            var levels = new short[NUMBER_OF_DIFFICULTIES];
            for (var i = 0; i < NUMBER_OF_DIFFICULTIES; i++)
                levels[i] = decoder.ReadShort();

            EmptyField = decoder.ReadShort();

            var noteCounts = new int[NUMBER_OF_DIFFICULTIES];
            for (var i = 0; i < NUMBER_OF_DIFFICULTIES; i++)
                noteCounts[i] = decoder.ReadInt();

            var playableNoteCounts = new int[NUMBER_OF_DIFFICULTIES];
            for (var i = 0; i < NUMBER_OF_DIFFICULTIES; i++)
                playableNoteCounts[i] = decoder.ReadInt();

            var measureCounts = new int[NUMBER_OF_DIFFICULTIES];
            for (var i = 0; i < NUMBER_OF_DIFFICULTIES; i++)
                measureCounts[i] = decoder.ReadInt();

            var packageCounts = new int[NUMBER_OF_DIFFICULTIES];
            for (var i = 0; i < NUMBER_OF_DIFFICULTIES; i++)
                packageCounts[i] = decoder.ReadInt();

            OldEncodeVersion = decoder.ReadShort();
            OldSongID = decoder.ReadShort();
            OldGenre = decoder.ReadString(20);
            SizeOfBMPImage = decoder.ReadInt();
            OldFileVersion = decoder.ReadInt();
            Title = decoder.ReadString(64);
            Artist = decoder.ReadString(32);
            Notecharter = decoder.ReadString(32);
            OjmFile = decoder.ReadString(32);
            SizeOfJPGFile = decoder.ReadInt();

            var durations = new int[NUMBER_OF_DIFFICULTIES];
            for (var i = 0; i < NUMBER_OF_DIFFICULTIES; i++)
                durations[i] = decoder.ReadInt();

            var startingNoteOffsets = new int[NUMBER_OF_DIFFICULTIES];
            for (var i = 0; i < NUMBER_OF_DIFFICULTIES; i++)
                startingNoteOffsets[i] = decoder.ReadInt();

            ImageOffset = decoder.ReadInt();

            Difficulties = new List<OjnNoteChart>();

            for (var difficulty = 0; difficulty < NUMBER_OF_DIFFICULTIES; difficulty++)
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

            Console.WriteLine("Hello World!");
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

                // Package
                for (var eventPackage = 0; eventPackage < eventCount; eventPackage++)
                {
                    switch (channel)
                    {
                        case 0:
                            var measurement = decoder.ReadSingle();
                            eventPackages.Add(new O2JamMeasurementEventPackage(measurement));
                            break;
                        case 1:
                            var bpm = decoder.ReadSingle();
                            eventPackages.Add(new O2JamBpmEventPackage(bpm));
                            break;
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                            var indexIndicator = decoder.ReadShort();
                            var panSoundAndVolumeNote = decoder.ReadBytes(1)[0]; // each takes half a byte
                            var panSound = (byte)(panSoundAndVolumeNote >> 4); // take the first four bits
                            var volumeNote = (byte)(panSoundAndVolumeNote & 0b00001111); // take the last four bits
                            var noteType = decoder.ReadBytes(1)[0];
                            eventPackages.Add(new O2JamNoteEventPackage(indexIndicator, panSound, volumeNote, noteType));
                            break;
                        default: // 9-22 are some kind of "auto-play sample notes", which I will ignore
                            decoder.ReadBytes(4);
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

        public OjnNoteChart GetDifficulty(O2JamDifficulty difficulty) => Difficulties[(int)difficulty];

    }
}
