namespace EspDotNet.Config
{
    public class ESPToolConfig
    {
        public List<DeviceConfig> Devices { get; set; } =
        [
            new DeviceConfig{ ChipType = ChipTypes.ESP32c6,        FlashBlockSize = 0x400, RamBlockSize = 0x1800 },
            new DeviceConfig{ ChipType = ChipTypes.ESP32c3,        FlashBlockSize = 0x400, RamBlockSize = 0x1800 },
            new DeviceConfig{ ChipType = ChipTypes.ESP32c2,        FlashBlockSize = 0x400, RamBlockSize = 0x1800 },
            new DeviceConfig{ ChipType = ChipTypes.ESP32,          FlashBlockSize = 0x400, RamBlockSize = 0x1800 },
            new DeviceConfig{ ChipType = ChipTypes.ESP8266,        FlashBlockSize = 0x400, RamBlockSize = 0x1800 },
            new DeviceConfig{ ChipType = ChipTypes.ESP32s3beta2,   FlashBlockSize = 0x400, RamBlockSize = 0x1800 },
            new DeviceConfig{ ChipType = ChipTypes.ESP32s3,        FlashBlockSize = 0x400, RamBlockSize = 0x1800 },
            new DeviceConfig{ ChipType = ChipTypes.ESP32s2,        FlashBlockSize = 0x400, RamBlockSize = 0x1800 },
            new DeviceConfig{ ChipType = ChipTypes.ESP32p4,        FlashBlockSize = 0x400, RamBlockSize = 0x1800 },
            new DeviceConfig{ ChipType = ChipTypes.ESP32h2beta2,   FlashBlockSize = 0x400, RamBlockSize = 0x1800 },
            new DeviceConfig{ ChipType = ChipTypes.ESP32h2beta1,   FlashBlockSize = 0x400, RamBlockSize = 0x1800 },
            new DeviceConfig{ ChipType = ChipTypes.ESP32h2,        FlashBlockSize = 0x400, RamBlockSize = 0x1800 },
            new DeviceConfig{ ChipType = ChipTypes.ESP32c6beta,    FlashBlockSize = 0x400, RamBlockSize = 0x1800 },
        ];

        public PinSequence? BootloaderSequence { get; set; } = new PinSequence
        {
            Steps =
            [
                new PinSequenceStep {  Dtr = false, Rts = true, Delay = TimeSpan.FromMilliseconds(100) },
                new PinSequenceStep {  Dtr = true,  Rts = false, Delay = TimeSpan.FromMilliseconds(500) },
                new PinSequenceStep {  Dtr = false, Rts = false, Delay = TimeSpan.FromMilliseconds(100) },
            ]
        };

        public PinSequence? ResetSequence { get; set; } = new PinSequence
        {
            Steps =
            [
                new PinSequenceStep {  Dtr = false, Rts = true, Delay = TimeSpan.FromMilliseconds(500) },
                new PinSequenceStep {  Dtr = false,  Rts = false, Delay = TimeSpan.FromMilliseconds(0) },
            ]
        };
    }
}
