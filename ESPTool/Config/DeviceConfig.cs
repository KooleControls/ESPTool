namespace EspDotNet.Config
{
    public class DeviceConfig
    {
        public ChipTypes ChipType { get; set; }
        public int RamBlockSize { get; set; }
        public int FlashBlockSize { get; set; }
        public int MagicRegisterValue { get; set; }
        public Dictionary<EFlagKey, EFuseMapping> EFlags { get; set; } = new Dictionary<EFlagKey, EFuseMapping>();

    }

}
