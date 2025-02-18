using System.Text.Json.Serialization;

namespace ESPTool.Models
{
    public class Firmware
    {
        public uint EntryPoint { get; set; }
        public ChipTypes ChipType { get; set; }
        public List<FirmwareSegment> Segments { get; set; } = [];

    }



}
