using System;
using System.Collections.Generic;

namespace ESPTool.Com
{

    public class SlipFraming
    {
        public event EventHandler<Frame> FrameRecieved;
        List<byte> frameBuffer = new List<byte>();

        bool startFound = false;
        bool esc = false;

        public void Decode(byte data)
        {
            //0xC0 => 0xDB 0xDC
            //0xDB => 0xDB 0xDD
            if (startFound)
            {
                if (esc)
                {
                    if (data == 0xDC)
                        frameBuffer.Add(0xC0);
                    if (data == 0xDD)
                        frameBuffer.Add(0xDB);
                    esc = false;
                }
                else if (data == 0xC0)
                {
                    if (frameBuffer.Count > 0)
                    {
                        startFound = false;
                        FrameRecieved?.Invoke(this, new Frame(frameBuffer.ToArray()));
                        frameBuffer.Clear();
                    }
                }
                else if (data == 0xDB)
                    esc = true;
                else
                    frameBuffer.Add(data);
            }
            else
            {
                if (data == 0xC0)
                    startFound = true;
            }
        }


        public byte[] Encode(Frame frame)
        {
            List<byte> raw = new List<byte>();
            raw.Add(0xC0);

            //0xC0 => 0xDB 0xDC
            //0xDB => 0xDB 0xDD

            foreach (byte b in frame.Data)
            {
                if (b == 0xC0)
                {
                    raw.Add(0xDB);
                    raw.Add(0xDC);
                }
                else if (b == 0xDB)
                {
                    raw.Add(0xDB);
                    raw.Add(0xDD);
                }
                else
                {
                    raw.Add(b);
                }
            }
            raw.Add(0xC0);
            return raw.ToArray();
        }
    }

}
