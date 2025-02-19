using EspDotNet.Loaders;

namespace EspDotNet.Tools
{
    public class ChipTypeDetectTool
    {
        private readonly ILoader _loader;
        public ChipTypeDetectTool(ILoader loader)
        {
            _loader = loader;
        }
        public async Task<ChipTypes> DetectChipTypeAsync(CancellationToken token)
        {
            // https://esp32.com/viewtopic.php?t=26626
            uint CHIP_DETECT_MAGIC_REG_ADDR = 0x40001000; 
            uint id = await _loader.ReadRegisterAsync(CHIP_DETECT_MAGIC_REG_ADDR, token);
            return id switch
            {
                0xfff0c101 => ChipTypes.ESP8266,
                0x00f01d83 => ChipTypes.ESP32,
                0x000007c6 => ChipTypes.ESP32S2,
                0x00000009 => ChipTypes.ESP32S3,
                0xeb004136 => ChipTypes.ESP32S3BETA2,
                0x6921506f => ChipTypes.ESP32C3ECO12,
                0x1b31506f => ChipTypes.ESP32C3ECO3,
                0xca26cc22 => ChipTypes.ESP32H2,
                0x0da1806f => ChipTypes.ESP32C6BETAROM,
                _ => ChipTypes.Unknown
            };
        }
    }
}
