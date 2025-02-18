using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPTool.Models
{

    public class FirmwareSegment
    {
        public uint Offset { get; set; } = 0;
        public byte[] Data { get; set; } = [];

        public FirmwareSegment(uint offset, byte[] data)
        {
            Offset = offset;
            Data = data;
        }
    }

}
