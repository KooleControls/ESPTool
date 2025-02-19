namespace EspDotNet.Loaders.ESP32BootLoader
{
    public enum BootLoaderResponseStatus
    {
        Invalid = 0x05,
        Failed = 0x06,
        InvalidCRC = 0x07,
        WriteError = 0x08,
        ReadError = 0x09,
        ReadLenthError = 0x0a,
        DeflateError = 0x0b,
    }
}
