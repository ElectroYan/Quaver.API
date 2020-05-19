namespace Quaver.API.Maps.Parsers.O2Jam
{
    public abstract class O2JamEventPackage
    {
        /// <summary>
        ///     Used to check, if an event is ignored or not.
        /// </summary>
        /// <returns></returns>
        public abstract bool IsNonZero();
    }
}
