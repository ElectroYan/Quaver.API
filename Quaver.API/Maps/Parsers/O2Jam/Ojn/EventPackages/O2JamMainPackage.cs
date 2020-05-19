using System.Collections.Generic;

namespace Quaver.API.Maps.Parsers.O2Jam.EventPackages
{
    /// <summary>
    ///     Represents events for one measure for one BPM Change / Measurement Change / Lane
    /// </summary>
    /// <example>
    ///     If a chart contains one note in each 7 lanes in measure 3,
    ///     then there would be a total of 7 main packages with measure 3 with channels 2-8
    /// </example>
    public struct O2JamMainPackage
    {
        /// <summary>
        ///     0-based measure this main package is located at.
        /// </summary>
        public int Measure;

        /// <summary>
        ///     Represents the type of event
        ///         0: Measurement Event, see <see cref="O2JamMeasurementEventPackage"/>
        ///         1: BPM Event, see <see cref="O2JamBpmEventPackage"/>
        ///       2-9: Note Event, see <see cref="O2JamNoteEventPackage"/>, the channel controls the lane
        ///      9-22: Autoplay samples
        /// </summary>
        public short Channel;

        /// <summary>
        ///     Number of event packages.
        ///
        ///     The number of event packages also controls the snap/divisor
        ///     the events are snapped to. If no event falls on a particular snap, then a value of 0 is used for
        ///     <see cref="O2JamBpmEventPackage.Bpm"/>, <see cref="O2JamMeasurementEventPackage.Measurement"/>
        ///     and <see cref="O2JamNoteEventPackage.SampleIndex"/> to signify, that an event should be ignored
        ///     (as a kind of buffer).
        /// </summary>
        public short EventCount;

        /// <summary>
        ///     All event packages.
        /// </summary>
        public List<O2JamEventPackage> EventPackages;
    }
}
