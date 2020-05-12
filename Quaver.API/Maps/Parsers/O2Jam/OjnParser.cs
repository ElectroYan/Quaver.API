using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Quaver.API.Maps.Parsers.O2Jam
{
    public class OjnParser
    {
        private readonly FileStream stream;
        public OjnParser(string filePath) => stream = new FileStream(filePath, FileMode.Open);

        public int IDSong { get; set; }
        public string FileSignature { get; set; }
        public byte[] EncodeOJNVersion { get; set; }
        public int GenreOfSong { get; set; }
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
        public OjnNoteFile NoteFileEx { get; set; }
        public OjnNoteFile NoteFileNx { get; set; }
        public OjnNoteFile NoteFileHx { get; set; }

        public void Parse()
        {
            var enc = new UTF8Encoding(true);
            IDSong = BitConverter.ToInt32(ReadBytes(4), 0);
            FileSignature = enc.GetString(ReadBytes(4));
            EncodeOJNVersion = ReadBytes(4);
            GenreOfSong = BitConverter.ToInt32(ReadBytes(4), 0);
            BpmSong = ReadBytes(4);
            var levelForEx = BitConverter.ToInt16(ReadBytes(2), 0);
            var levelForNx = BitConverter.ToInt16(ReadBytes(2), 0);
            var levelForHx = BitConverter.ToInt16(ReadBytes(2), 0);
            EmptyField = BitConverter.ToInt16(ReadBytes(2), 0);
            var noteCountForEx = BitConverter.ToInt32(ReadBytes(4), 0);
            var noteCountForNx = BitConverter.ToInt32(ReadBytes(4), 0);
            var noteCountForHx = BitConverter.ToInt32(ReadBytes(4), 0);
            var playableNoteCountForEx = BitConverter.ToInt32(ReadBytes(4), 0);
            var playableNoteCountForNx = BitConverter.ToInt32(ReadBytes(4), 0);
            var playableNoteCountForHx = BitConverter.ToInt32(ReadBytes(4), 0);
            var measureCountForEx = BitConverter.ToInt32(ReadBytes(4), 0);
            var measureCountForNx = BitConverter.ToInt32(ReadBytes(4), 0);
            var measureCountForHx = BitConverter.ToInt32(ReadBytes(4), 0);
            var packagesCountForEx = BitConverter.ToInt32(ReadBytes(4), 0);
            var packagesCountForNx = BitConverter.ToInt32(ReadBytes(4), 0);
            var packagesCountForHx = BitConverter.ToInt32(ReadBytes(4), 0);
            OldEncodeVersion = BitConverter.ToInt16(ReadBytes(2), 0);
            OldEncodeVersion = BitConverter.ToInt16(ReadBytes(2), 0);
            OldSongID = BitConverter.ToInt16(ReadBytes(2), 0);
            OldGenre = enc.GetString(ReadBytes(20));
            SizeOfBMPImage = BitConverter.ToInt32(ReadBytes(4), 0);
            OldFileVersion = BitConverter.ToInt32(ReadBytes(4), 0);
            Title = enc.GetString(ReadBytes(64));
            Artist = enc.GetString(ReadBytes(32));
            Notecharter = enc.GetString(ReadBytes(32));
            OjmFile = enc.GetString(ReadBytes(32));
            SizeOfJPGFile = BitConverter.ToInt32(ReadBytes(4), 0);
            var durationForEx = BitConverter.ToInt32(ReadBytes(4), 0);
            var durationForNx = BitConverter.ToInt32(ReadBytes(4), 0);
            var durationForHx = BitConverter.ToInt32(ReadBytes(4), 0);
            var startingNoteOffsetForEx = BitConverter.ToInt32(ReadBytes(4), 0);
            var startingNoteOffsetForNx = BitConverter.ToInt32(ReadBytes(4), 0);
            var startingNoteOffsetForHx = BitConverter.ToInt32(ReadBytes(4), 0);
            ImageOffset = BitConverter.ToInt32(ReadBytes(4), 0);

            NoteFileEx = ParseDifficulty(levelForEx, noteCountForEx,playableNoteCountForEx,measureCountForEx,packagesCountForEx,durationForEx,startingNoteOffsetForEx);
            NoteFileNx = ParseDifficulty(levelForNx, noteCountForNx,playableNoteCountForNx,measureCountForNx,packagesCountForNx,durationForNx,startingNoteOffsetForNx);
            NoteFileHx = ParseDifficulty(levelForHx, noteCountForHx,playableNoteCountForHx,measureCountForHx,packagesCountForHx,durationForHx,startingNoteOffsetForHx);
        }

        private OjnNoteFile ParseDifficulty(int level, int noteCount, int playableNoteCount, int measureCount, int packagesCount, int duration, int startingNoteOffset)
        {
            var noteFile = new OjnNoteFile
            {
                Level = level
            };
            stream.Position = startingNoteOffset;


            return noteFile;
        } 

        private byte[] ReadBytes(int count)
        {
            var bytes = new byte[count];
            stream.Read(bytes, 0, count);
            return bytes;
        }
    }
}
