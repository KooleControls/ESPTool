using System;

namespace ESPTool.Firmware
{
    public class Segment
    {
        public UInt32 Offset { get; set; }
        public byte[] Data { get; set; }
    }
}
