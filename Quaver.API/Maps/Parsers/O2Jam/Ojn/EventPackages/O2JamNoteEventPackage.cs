using System;

namespace Quaver.API.Maps.Parsers.O2Jam.EventPackages
{
    /// <summary>
    ///     Represents notes for one measure for one lane
    /// </summary>
    public class O2JamNoteEventPackage : O2JamEventPackage
    {
        /// <summary>
        ///     Reference to the sound sample index in the .ojm file.
        ///     0 means no note (event is ignored)
        /// </summary>
        public short SampleIndex;

        /// <summary>
        ///     Panning of the sound in a stereo environment
        ///     1~7 = left - center
        ///     0 or 8 = center
        ///     9~15 = center - right 
        /// </summary>
        /// <remarks>Uses half a byte</remarks>
        public byte SoundPan;

        /// <summary>
        ///     Volume to play the sample from 0-15, 0 is the max volume.
        /// </summary>
        /// <remarks>Uses half a byte</remarks>
        public byte SoundVolume;

        /// <summary>
        ///     Note type, see <see cref="O2JamNoteType"/>
        /// </summary>
        public O2JamNoteType NoteType;

        public O2JamNoteEventPackage(short sampleIndex, byte soundPan, byte soundVolume, byte noteType)
        {
            SampleIndex = sampleIndex;
            SoundPan = soundPan;
            SoundVolume = soundVolume;
            NoteType = (O2JamNoteType)noteType;
        }

        public override bool IsNonZero() => SampleIndex != 0;
    }
}
