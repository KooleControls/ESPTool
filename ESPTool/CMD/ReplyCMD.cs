using System;
using System.Collections.Generic;
using System.Text;

namespace ESPTool.CMD
{
    public partial class ReplyCMD
    {
        public byte Direction { get; set; }
        public byte Command { get; set; }
        public virtual UInt16 Size { get; set; }
        public UInt32 Value { get; set; }
        public virtual byte[] Payload { get; set; } = new byte[0];
        public bool Success { get; set; }
        public Errors Error { get; set; }

        /*
        public static ReplyFrame FromRAW(byte[] data, bool usesSoftwareloader)
        {
            ReplyFrame frame = new ReplyFrame();
            frame.Direction = data[0];
            frame.Command = data[1];
            frame.Size = BitConverter.ToUInt16(data, 2);
            frame.Value = BitConverter.ToUInt32(data, 4);
            frame.Payload = data.SubArray(8);

            if (usesSoftwareloader)
            {
                frame.Success = frame.Payload[frame.Size - 2] == 0;
                frame.Error = (FrameErrors)frame.Payload[frame.Size - 1];
            }
            else
            {
                frame.Success = frame.Payload[frame.Size - 4] == 0;
                frame.Error = (FrameErrors)frame.Payload[frame.Size - 3];
            }
            return frame;
        }
        */
    }
}
