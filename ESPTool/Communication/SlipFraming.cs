using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EspDotNet.Communication
{
    /*  In slipframing, the following rules apply:
     *  0xC0 => 0xDB 0xDC
     *  0xDC => 0xDB 0xDD
     */

    public class SlipFraming
    {
        private const byte FrameDelimiter = 0xC0;
        private const byte EscapeByte = 0xDB;
        private const byte EscapeFrameDelimiter = 0xDC; 
        private const byte EscapeEscapeByte = 0xDD;
        private readonly SerialPort _serialPort;

        public SlipFraming(SerialPort serialPort)
        {
            _serialPort = serialPort;
        }

        public async Task WriteFrameAsync(Frame frame, CancellationToken token)
        {
            byte[] escapedFrame = EscapeFrame(frame);
            _serialPort.BaseStream.WriteByte(FrameDelimiter); // Start of frame
            await _serialPort.BaseStream.WriteAsync(escapedFrame, 0, escapedFrame.Length, token);
            _serialPort.BaseStream.WriteByte(FrameDelimiter); // end of frame
            await _serialPort.BaseStream.FlushAsync(token); // Ensure all data is sent
        }

        public async Task<Frame?> ReadFrameAsync(CancellationToken token)
        {
            List<byte> escapedFrameBuffer = new List<byte>();

            // In slipframing, all delimiters are replaced, so we can record everything between delimeters and decode it later
            while (true)
            {
                byte currentByte = await ReadByte(token);

                if (currentByte == FrameDelimiter)
                {
                    // If we havent recieved any data yet, this is the SOF
                    if (escapedFrameBuffer.Count > 0)
                        return Unescape(escapedFrameBuffer.ToArray());
                }
                else
                {
                    escapedFrameBuffer.Add(currentByte);
                }
            }
        }

        private async Task<byte> ReadByte(CancellationToken token)
        {
            // Wait for data
            while (_serialPort.BytesToRead == 0)
            {
                // Prevent busy waiting
                await Task.Delay(10, token);
            }

            return (byte)_serialPort.ReadByte();
        }

        private byte[] EscapeFrame(Frame frame)
        {
            List<byte> buffer = new();

            foreach (byte b in frame.Data)
            {
                if (b == FrameDelimiter)
                {
                    buffer.Add(EscapeByte);
                    buffer.Add(EscapeFrameDelimiter);
                }
                else if (b == EscapeByte)
                {
                    buffer.Add(EscapeByte);
                    buffer.Add(EscapeEscapeByte);
                }
                else
                {
                    buffer.Add(b);
                }
            }
            return buffer.ToArray();
        }

        private Frame? Unescape(byte[] data)
        {
            List<byte> buffer = new List<byte>();

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == EscapeByte)
                {
                    i++;
                    if (i >= data.Length) break;

                    if (data[i] == EscapeFrameDelimiter)
                    {
                        buffer.Add(FrameDelimiter);
                    }
                    else if (data[i] == EscapeEscapeByte)
                    {
                        buffer.Add(EscapeByte);
                    }
                }
                else
                {
                    buffer.Add(data[i]);
                }
            }

            return new Frame(buffer.ToArray());
        }
    }
}
