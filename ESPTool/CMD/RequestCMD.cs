using System;
using System.Collections.Generic;
using System.Text;

namespace ESPTool.CMD
{
    public class RequestCMD
    {
        public byte Direction { get; set; } = 0;
        public UInt16 Size { get; set; }
        public UInt32 Checksum { get; set; }
        public byte Command { get; set; }
        public bool ChecksumRequired { get; set; }
        public byte[] Payload { get; set; } = new byte[0];

        public RequestCMD()
        {

        }

        public RequestCMD(byte cmd, bool checksumRequired, byte[] payload)
        {
            ChecksumRequired = checksumRequired;
            Direction = 0x00;
            Command = cmd;
            Payload = payload;
            Size = (UInt16)Payload.Length;
            if (ChecksumRequired)
                CalculateChecksum();
        }

        /*
        public static RequestFrame FromRAW(byte[] data)
        {
            RequestFrame frame = new RequestFrame();
            frame.Direction = data[0];
            frame.Command = data[1];
            frame.Size = BitConverter.ToUInt16(data, 2);
            frame.Checksum = BitConverter.ToUInt32(data, 4);
            frame.Payload = data.SubArray(8);
            return frame;
        }

        public byte[] ToRAW()
        {
            List<byte> raw = new List<byte>();
            raw.Add(Direction);
            raw.Add(Command);
            raw.AddRange(BitConverter.GetBytes(Size));
            raw.AddRange(BitConverter.GetBytes(Checksum));
            raw.AddRange(Payload);
            return raw.ToArray();
        }
        */
        private void CalculateChecksum()
        {
            Checksum = 0xEF;

            for (int i = 16; i < Payload.Length; i++)
            {
                Checksum ^= Payload[i];
            }
        }

    }
}
