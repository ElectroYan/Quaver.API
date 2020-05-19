namespace Quaver.API.Maps.Parsers.O2Jam
{
    public enum O2JamNoteType
    {
        NormalNote = 0, // If hitsounded, then usually a WAV sample
        // 1 is currently unused
        StartLongNote = 2,
        EndLongNote = 3,
        BgmNote = 4 // OGG sample
    }
}
