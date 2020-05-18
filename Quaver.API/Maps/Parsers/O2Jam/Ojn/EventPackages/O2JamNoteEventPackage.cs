using System;

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

        // This value isn't actually in the note event package, but it helps with convertions
        // The value can originally be found in the main package, containing the note event
        public int Channel;

        public O2JamNoteEventPackage(short indexIndicator, byte panSound, byte volumeNote, byte noteType, int channel)
        {
            IndexIndicator = indexIndicator;
            PanSound = panSound;
            VolumeNote = volumeNote;
            NoteType = (O2JamNoteType)Enum.ToObject(typeof(O2JamNoteType), noteType);
            Channel = channel;
        }

        public override bool IsNonZero() => IndexIndicator != 0;
        public override float GetValue() => IndexIndicator;
    }
}
