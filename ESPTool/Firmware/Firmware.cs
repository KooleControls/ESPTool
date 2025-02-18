namespace ESPTool.Firmware
{
    public class Firmware
    {
        public uint EntryPoint { get; set; }
        public List<FirmwareSegment> Segments { get; set; } = [];

    }
}
