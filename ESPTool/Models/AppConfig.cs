namespace ESPTool.Models
{
    public class AppConfig
    {
        public List<DeviceConfig> Devices { get; set; } = [];
        public List<Firmware> SoftLoaders { get; set; } = [];
        public PinSequence? BootloaderSequence { get; set; }
        public PinSequence? ResetSequence { get; set; }
    }
}