namespace Quaver.API.Maps.Parsers.O2Jam.EventPackages
{
    public class O2JamBpmEventPackage : O2JamEventPackage
    {
        public float Bpm;
        public O2JamBpmEventPackage(float bpm) => Bpm = bpm;

        public override float GetValue() => Bpm;

        public override bool IsNonZero() => Bpm != 0.0f;
    }
}
