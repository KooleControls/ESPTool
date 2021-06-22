namespace ESPTool
{
    public enum Errors : byte
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

    public static class EnumErrorHelper
    {
        public static Errors ToGlobalError(this SoftLoaderErrors err)
        {
            switch (err)
            {
                case SoftLoaderErrors.ESP_OK: return Errors.NoError;
                case SoftLoaderErrors.ESP_BAD_DATA_LEN: return Errors.Invalid;
                case SoftLoaderErrors.ESP_BAD_DATA_CHECKSUM: return Errors.InvalidCRC;
                case SoftLoaderErrors.ESP_BAD_BLOCKSIZE: return Errors.BadBlockSize;
                case SoftLoaderErrors.ESP_INVALID_COMMAND: return Errors.Failed;
                case SoftLoaderErrors.ESP_FAILED_SPI_OP: return Errors.FailedSPIOP;
                case SoftLoaderErrors.ESP_FAILED_SPI_UNLOCK: return Errors.FailedSPIUnlock;
                case SoftLoaderErrors.ESP_NOT_IN_FLASH_MODE: return Errors.NotInFlashMode;
                case SoftLoaderErrors.ESP_INFLATE_ERROR: return Errors.InflateError;
                case SoftLoaderErrors.ESP_NOT_ENOUGH_DATA: return Errors.NotEnoughData;
                case SoftLoaderErrors.ESP_TOO_MUCH_DATA: return Errors.TooMuchData;
                case SoftLoaderErrors.ESP_CMD_NOT_IMPLEMENTED: return Errors.Failed;
                default: return Errors.Unknown;
            }
        }

        public static Errors ToGlobalError(this RomLoaderErrors err)
        {
            switch (err)
            {
                case RomLoaderErrors.Invalid: return Errors.Invalid;
                case RomLoaderErrors.Failed: return Errors.Failed;
                case RomLoaderErrors.InvalidCRC: return Errors.InvalidCRC;
                case RomLoaderErrors.WriteError: return Errors.WriteError;
                case RomLoaderErrors.ReadError: return Errors.ReadError;
                case RomLoaderErrors.ReadLenthError: return Errors.ReadLenthError;
                case RomLoaderErrors.DeflateError: return Errors.DeflateError;
                default: return Errors.Unknown;
            }
        }


    }

    public enum RomLoaderErrors
    {
        Invalid = 0x05,
        Failed = 0x06,
        InvalidCRC = 0x07,
        WriteError = 0x08,
        ReadError = 0x09,
        ReadLenthError = 0x0a,
        DeflateError = 0x0b,
    }

    //https://github.com/espressif/esptool/blob/master/flasher_stub/include/stub_flasher.h#L95
    public enum SoftLoaderErrors
    {
        ESP_OK = 0,
        ESP_BAD_DATA_LEN = 0xC0,
        ESP_BAD_DATA_CHECKSUM = 0xC1,
        ESP_BAD_BLOCKSIZE = 0xC2,
        ESP_INVALID_COMMAND = 0xC3,
        ESP_FAILED_SPI_OP = 0xC4,
        ESP_FAILED_SPI_UNLOCK = 0xC5,
        ESP_NOT_IN_FLASH_MODE = 0xC6,
        ESP_INFLATE_ERROR = 0xC7,
        ESP_NOT_ENOUGH_DATA = 0xC8,
        ESP_TOO_MUCH_DATA = 0xC9,
        ESP_CMD_NOT_IMPLEMENTED = 0xFF,
    }
}
