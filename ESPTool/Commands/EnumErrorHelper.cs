namespace ESPTool.Commands
{
    public static class EnumErrorHelper
    {
        public static ResponseErrors ToResponseError(this SoftLoaderErrors err)
        {
            switch (err)
            {
                case SoftLoaderErrors.ESP_OK: return ResponseErrors.NoError;
                case SoftLoaderErrors.ESP_BAD_DATA_LEN: return ResponseErrors.Invalid;
                case SoftLoaderErrors.ESP_BAD_DATA_CHECKSUM: return ResponseErrors.InvalidCRC;
                case SoftLoaderErrors.ESP_BAD_BLOCKSIZE: return ResponseErrors.BadBlockSize;
                case SoftLoaderErrors.ESP_INVALID_COMMAND: return ResponseErrors.Failed;
                case SoftLoaderErrors.ESP_FAILED_SPI_OP: return ResponseErrors.FailedSPIOP;
                case SoftLoaderErrors.ESP_FAILED_SPI_UNLOCK: return ResponseErrors.FailedSPIUnlock;
                case SoftLoaderErrors.ESP_NOT_IN_FLASH_MODE: return ResponseErrors.NotInFlashMode;
                case SoftLoaderErrors.ESP_INFLATE_ERROR: return ResponseErrors.InflateError;
                case SoftLoaderErrors.ESP_NOT_ENOUGH_DATA: return ResponseErrors.NotEnoughData;
                case SoftLoaderErrors.ESP_TOO_MUCH_DATA: return ResponseErrors.TooMuchData;
                case SoftLoaderErrors.ESP_CMD_NOT_IMPLEMENTED: return ResponseErrors.Failed;
                default: return ResponseErrors.Unknown;
            }
        }

        public static ResponseErrors ToResponseError(this RomLoaderErrors err)
        {
            switch (err)
            {
                case RomLoaderErrors.Invalid: return ResponseErrors.Invalid;
                case RomLoaderErrors.Failed: return ResponseErrors.Failed;
                case RomLoaderErrors.InvalidCRC: return ResponseErrors.InvalidCRC;
                case RomLoaderErrors.WriteError: return ResponseErrors.WriteError;
                case RomLoaderErrors.ReadError: return ResponseErrors.ReadError;
                case RomLoaderErrors.ReadLenthError: return ResponseErrors.ReadLenthError;
                case RomLoaderErrors.DeflateError: return ResponseErrors.DeflateError;
                default: return ResponseErrors.Unknown;
            }
        }


    }
}
