namespace Quaver.API.Maps.Parsers.O2Jam.EventPackages
{
    public class O2JamMeasurementEventPackage : O2JamEventPackage
    {
        public float Measurement; // A value of 0.50 would refer to half the size of the normal measure size
        public O2JamMeasurementEventPackage(float measurement) => Measurement = measurement;
        public override bool IsNonZero() => Measurement != 0;
        public override float GetValue() => Measurement;
    }
}
