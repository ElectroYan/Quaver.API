namespace Quaver.API.Maps.Parsers.O2Jam.EventPackages
{
    public class O2JamBpmEventPackage : O2JamEventPackage
    {
        /// <summary>
        ///     BPM of the current event
        ///     0 means event is ignored
        /// </summary>
        public float Bpm;
        public O2JamBpmEventPackage(float bpm) => Bpm = bpm;
        public override bool IsNonZero() => Bpm != 0.0f;
    }
}
