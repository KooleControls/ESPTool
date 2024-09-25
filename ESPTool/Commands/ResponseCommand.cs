using System;
using System.Collections.Generic;
using System.Text;
using ESPTool.Commands;

namespace ESPTool.CMD
{
    public partial class ResponseCommand
    {
        public byte Direction { get; set; }
        public byte Command { get; set; }
        public virtual UInt16 Size { get; set; }
        public UInt32 Value { get; set; }
        public virtual byte[] Payload { get; set; } = new byte[0];
        public bool Success { get; set; }
        public ResponseErrors Error { get; set; }
    }
}
