namespace Quaver.API.Maps.Parsers.O2Jam.EventPackages
{
    public class O2JamNoteEventPackage : O2JamEventPackage
    {
        // W### or M###
        public short IndexIndicator;

        // 1~7 = left -> center, 0 or 8 = center, 9~15 = center -> right
        // uses half a byte
        public byte PanSound;

        // The volume to play the sample from 1~15, and 0 is the max volume.
        // uses half a byte
        public byte VolumeNote;

        public O2JamNoteType NoteType;

        public O2JamNoteEventPackage(short indexIndicator, byte panSound, byte volumeNote, byte noteType)
        {
            IndexIndicator = indexIndicator;
            PanSound = panSound;
            VolumeNote = volumeNote;
            NoteType = (O2JamNoteType)noteType;
        }
    }
}
