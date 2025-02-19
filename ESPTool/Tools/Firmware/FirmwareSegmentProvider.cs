namespace EspDotNet.Tools.Firmware
{
    public class FirmwareSegmentProvider : IFirmwareSegmentProvider
    {
        public uint Offset { get; }
        public uint Size => (uint)_data.Length;
        private readonly byte[] _data;

        public FirmwareSegmentProvider(uint offset, byte[] data)
        {
            Offset = offset;
            _data = data;
        }

        public Task<Stream> GetStreamAsync(CancellationToken token = default)
        {
            return Task.FromResult<Stream>(new MemoryStream(_data, writable: false));
        }
    }


}
