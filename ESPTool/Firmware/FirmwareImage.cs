using System;
using System.Collections.Generic;
using System.IO;

namespace ESPTool.Firmware
{

    public class FirmwareImage
    {
        public SPIInterfaces SPIInterface { get; set; }
        public byte SPIFrequency { get; set; }
        public UInt32 EntryPoint { get; set; }
        public List<Segment> Segments { get; set; } = new List<Segment>();

        public static FirmwareImage OpenESP32BIN(string binFile)
        {
            FirmwareImage image = null;
            FileInfo bin = new FileInfo(binFile);
            if (bin.Exists)
            {
                using (Stream stream = bin.OpenRead())
                {
                    byte[] header = new byte[32];
                    stream.Read(header, 0, header.Length);

                    /*
                    int check = stream.ReadByte();

                    if (check == 0xE9)
                    {
                        image = new FirmwareImage();
                        image.Segments = new List<Segment>();
                        int noSegments = (byte)stream.ReadByte();
                        image.SPIInterface = (SPIInterfaces)stream.ReadByte();
                        image.SPIFrequency = (byte)stream.ReadByte();
                        image.EntryPoint = stream.ReadUInt32();
                        for(int i = 0; i<noSegments; i++)
                        {
                            Segment segment = new Segment();
                            segment.Offset = stream.ReadUInt32();
                            UInt32 size = stream.ReadUInt32();
                            byte[] data = new byte[size];
                            stream.Read(data, 0, data.Length);
                            segment.Data = data;
                            image.Segments.Add(segment);
                        }
                    }
                    */
                }
            }
            return image;
        }
    }
}
