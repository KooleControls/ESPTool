namespace EspDotNet.Config
{
    public class ESPToolConfig
    {
        public List<DeviceConfig> Devices { get; set; } =
        [
            new DeviceConfig{ ChipType = ChipTypes.ESP32,   FlashBlockSize = 1024, RamBlockSize = 6144 },
            new DeviceConfig{ ChipType = ChipTypes.ESP32S2, FlashBlockSize = 2048, RamBlockSize = 6144 }
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
