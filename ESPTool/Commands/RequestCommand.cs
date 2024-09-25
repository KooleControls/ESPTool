using System;
using System.Collections.Generic;
using System.Text;

namespace ESPTool.CMD
{
    public class RequestCommand
    {
        public byte Direction { get; set; } = 0;
        public ushort Size { get; set; }
        public uint Checksum { get; set; }
        public byte Command { get; set; }
        public bool ChecksumRequired { get; set; }
        public byte[] Payload { get; set; } = new byte[0];
    }
    

}
