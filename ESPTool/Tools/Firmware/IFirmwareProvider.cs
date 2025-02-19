using System.Text.Json.Serialization;

namespace EspDotNet.Tools.Firmware
{
    public interface IFirmwareProvider
    {
        uint EntryPoint { get; }
        IReadOnlyList<IFirmwareSegmentProvider> Segments { get; }
    }
}

