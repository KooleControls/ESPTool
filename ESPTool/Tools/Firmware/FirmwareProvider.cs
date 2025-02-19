namespace EspDotNet.Tools.Firmware
{
    public class FirmwareProvider : IFirmwareProvider
    {
        public uint EntryPoint { get; }
        public IReadOnlyList<IFirmwareSegmentProvider> Segments { get; }

        public FirmwareProvider(uint entryPoint, IEnumerable<IFirmwareSegmentProvider> segments)
        {
            EntryPoint = entryPoint;
            Segments = segments.ToList().AsReadOnly();
        }
    }


}
