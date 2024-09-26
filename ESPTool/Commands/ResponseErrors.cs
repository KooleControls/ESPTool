namespace ESPTool.Commands
{
    public enum ResponseErrors : byte
    {
        NoError,
        Unknown,
        Invalid,            //"Received message is invalid" (parameters or length field is invalid)
        Failed,             //"Failed to act on received message"
        InvalidCRC,         //"Invalid CRC in message"
        WriteError,         //"flash write error" - after writing a block of data to flash, the ROM loader reads the value back and the 8-bit CRC is compared to the data read from flash. If they don't match, this error is returned.
        ReadError,          //"flash read error" - SPI read failed
        ReadLenthError,     //"flash read length error" - SPI read request length is too long
        DeflateError,       //"Deflate error" (ESP32 compressed uploads only)
        BadBlockSize,
        FailedSPIOP,
        FailedSPIUnlock,
        NotInFlashMode,
        InflateError,
        NotEnoughData,
        TooMuchData,
        TaskCancelled,
        UnsupportedByLoader,
        UnsupportedByDevice,
        MD5Mismatch,
        WrongChip,
    }
}
