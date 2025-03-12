namespace EspDotNet.Config
{
    public class DeviceConfig
    {
        public ChipTypes ChipType { get; set; }
        public int RamBlockSize { get; set; }
        public int FlashBlockSize { get; set; }
        public UInt32 MagicRegisterValue { get; set; }
    }
}
