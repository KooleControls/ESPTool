namespace ESPTool.CMD
{
    public enum Errors : byte
    {
        /// <summary>
        /// "Received message is invalid" (parameters or length field is invalid)
        /// </summary>
        Invalid = 0x05,
        /// <summary>
        /// "Failed to act on received message"
        /// </summary>
        Failed = 0x06,
        /// <summary>
        /// "Invalid CRC in message"
        /// </summary>
        InvalidCRC = 0x07,
        /// <summary>
        /// "flash write error" - after writing a block of data to flash, the ROM loader reads the value back and the 8-bit CRC is compared to the data read from flash. If they don't match, this error is returned.
        /// </summary>
        WriteError = 0x08,
        /// <summary>
        /// "flash read error" - SPI read failed
        /// </summary>
        ReadError = 0x09,
        /// <summary>
        /// "flash read length error" - SPI read request length is too long
        /// </summary>
        ReadLenthError = 0x0a,
        /// <summary>
        /// "Deflate error" (ESP32 compressed uploads only)
        /// </summary>
        DeflateError = 0x0b,

        UnknownError = 0xFF
    }
}
