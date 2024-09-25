namespace ESPTool.Commands
{
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
