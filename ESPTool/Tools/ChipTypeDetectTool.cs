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
                0x000007c6 => ChipTypes.ESP32s2,
                0x00000009 => ChipTypes.ESP32s3,
                0xca26cc22 => ChipTypes.ESP32h2,
                0x1B31506F => ChipTypes.ESP32c3,
                _ => ChipTypes.Unknown
            };
        }
    }
}
