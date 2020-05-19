namespace Quaver.API.Maps.Parsers.O2Jam.EventPackages
{
    public class O2JamMeasurementEventPackage : O2JamEventPackage
    {
        /// <summary>
        ///     Controls the size of the measures after a specific point, a value of 0.50 would refer
        ///     to half the size of the normal measure size
        ///
        ///     0 means the event is ignored
        /// </summary>
        public float Measurement;
        public O2JamMeasurementEventPackage(float measurement) => Measurement = measurement;
        public override bool IsNonZero() => Measurement != 0;
    }
}
