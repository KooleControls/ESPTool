﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EspDotNet.Commands
{
    public partial class ResponseCommand
    {
        public byte Direction { get; set; }
        public byte Command { get; set; }
        public virtual ushort Size { get; set; }
        public uint Value { get; set; }
        public virtual byte[] Payload { get; set; } = new byte[0];
        public bool Success { get; set; }
        public ResponseCommandStatus Error { get; set; }
    }
}
