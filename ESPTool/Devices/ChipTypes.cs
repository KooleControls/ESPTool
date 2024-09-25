using System;

namespace ESPTool.Devices
{
    public enum ChipTypes : uint
    {
        Unknown = 0,
        ESP8266 = 0xfff0c101,
        ESP32 = 0x00f01d83,
        ESP32S2 = 0x000007c6,
        ESP32S3 = 0x9,
        ESP32S3BETA2 = 0xeb004136,
        ESP32C3ECO12 = 0x6921506f,
        ESP32C3ECO3 = 0x1b31506f,
        ESP32H2 = 0xca26cc22,
        ESP32C6BETAROM = 0x0da1806f,
    }

}
