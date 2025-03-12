using EspDotNet.Config;
using EspDotNet.Loaders;

namespace EspDotNet.Tools
{
    public class ChipTypeDetectTool
    {
        private readonly ILoader _loader;
        private readonly ESPToolConfig _config;

        public ChipTypeDetectTool(ILoader loader, ESPToolConfig config)
        {
            _loader = loader;
            _config = config;
        }
        public async Task<ChipTypes> DetectChipTypeAsync(CancellationToken token)
        {
            // https://esp32.com/viewtopic.php?t=26626
            uint CHIP_DETECT_MAGIC_REG_ADDR = 0x40001000; 
            uint id = await _loader.ReadRegisterAsync(CHIP_DETECT_MAGIC_REG_ADDR, token);
            var deviceConfig = _config.Devices.FirstOrDefault(device => device.MagicRegisterValue == id);
            return deviceConfig == null ? ChipTypes.Unknown : deviceConfig.ChipType;
        }
    }
}
