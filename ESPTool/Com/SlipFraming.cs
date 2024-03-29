﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ESPTool.Com
{

    public class SlipFraming
    {
        public event EventHandler<Frame> FrameRecieved;
        List<byte> frameBuffer = new List<byte>();


        public void ClearBuffer()
        {
            lock(frameBuffer)
                frameBuffer.Clear();
        }


        public void Decode(byte[] data)
        {
            //0xC0 => 0xDB 0xDC
            //0xDB => 0xDB 0xDD

            lock (frameBuffer)
            {
                frameBuffer.AddRange(data);

                bool ok = true;


                while (ok)
                {
                    int start = frameBuffer.IndexOf(0xC0);
                    int end = frameBuffer.IndexOf(0xC0, start + 1);
                    ok = start != -1 && end != -1;

                    if (ok)
                    {
                        List<byte> buf = new List<byte>(); ;

                        for (int i = start + 1; i < end; i++)
                        {
                            byte a0 = frameBuffer[i];
                            byte a1 = frameBuffer[i + 1];

                            if (a0 == 0xDB)
                            {
                                if (a1 == 0xDC)
                                {
                                    buf.Add(0xC0);
                                }
                                else if (a1 == 0xDD)
                                    buf.Add(0xDB);
                                i++;
                            }
                            else
                                buf.Add(a0);

                        }

                        frameBuffer.RemoveRange(0, end + 1);
                        FrameRecieved?.Invoke(this, new Frame(buf.ToArray()));
                    }
                }

            }


        }


        /*
        public void Decode(byte[] data)
        {
            //0xC0 => 0xDB 0xDC
            //0xDB => 0xDB 0xDD

            int i;
            for (i = 0; i < data.Length; i++)
            {
                byte bt = data[i];

                if (startFound)
                {
                    if (esc)
                    {
                        if (bt == 0xDC)
                            frameBuffer.Add(0xC0);
                        if (bt == 0xDD)
                            frameBuffer.Add(0xDB);
                        esc = false;
                    }
                    else if (bt == 0xC0)
                    {
                        if (frameBuffer.Count > 0)
                        {
                            startFound = false;
                            FrameRecieved?.Invoke(this, new Frame(frameBuffer.ToArray()));
                            frameBuffer.Clear();
                        }
                    }
                    else if (bt == 0xDB)
                        esc = true;
                    else
                        frameBuffer.Add(bt);
                }
                else
                {
                    if (bt == 0xC0)
                        startFound = true;
                }
            }
        }
        */

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
