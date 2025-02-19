namespace EspDotNet.Tools.Firmware
{
    public interface IFirmwareSegmentProvider
    {
        uint Offset { get; }
        uint Size { get; }
        Task<Stream> GetStreamAsync(CancellationToken token = default);
    }


}
