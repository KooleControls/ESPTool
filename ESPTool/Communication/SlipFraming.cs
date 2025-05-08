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

        /// <summary>
        /// Writes a SLIP-encoded frame asynchronously to the stream.
        /// </summary>
        /// <param name="frame">The frame to be written to the stream.</param>
        /// <param name="token">Cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via the cancellation token.</exception>
        /// <exception cref="IOException">Thrown if an I/O error occurs during writing.</exception>
        public async Task WriteFrameAsync(Frame frame, CancellationToken token)
        {
            byte[] encodedFrame = Encode(frame);
            //Debug.Write($"(TX) ");
            //foreach(var b in encodedFrame)
            //    Debug.Write($"{b:X2} ");
            //Debug.WriteLine($"");
            await _serialPort.BaseStream.WriteAsync(encodedFrame, 0, encodedFrame.Length, token);
            await _serialPort.BaseStream.FlushAsync(token); // Ensure all data is sent
        }

        /// <summary>
        /// Reads and decodes a SLIP-encoded frame asynchronously from the stream.
        /// Reads until the end of the frame is received.
        /// </summary>
        /// <param name="token">Cancellation token to cancel the operation.</param>
        /// <returns>The decoded frame, or null if the end of the stream is reached.</returns>
        /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via the cancellation token.</exception>
        /// <exception cref="IOException">Thrown if an I/O error occurs during reading.</exception>
        public async Task<Frame?> ReadFrameAsync(CancellationToken token)
        {
            List<byte> buffer = new List<byte>();
            bool startFound = false;

            while (true)
            {
                // Wait for data
                while (_serialPort.BytesToRead == 0)
                {
                    // Prevent busy waiting
                    await Task.Delay(10, token);
                }

                byte currentByte = (byte)_serialPort.ReadByte();

                if (currentByte == FrameDelimiter)
                {
                    if (startFound)
                    {
                        // End of the frame
                        if (buffer.Count == 0)
                            continue;

                        return Decode(buffer.ToArray());
                    }
                    else
                    {
                        // Start of the frame
                        startFound = true;
                        buffer.Clear();
                        //Debug.Write($"(RX) SOF ");
                    }
                }
                else if (startFound)
                {
                    // Add byte to the buffer
                    buffer.Add(currentByte);
                    //Debug.Write($"{currentByte:X2} ");
                }
            }
        }

        /// <summary>
        /// Encodes the frame into SLIP format.
        /// </summary>
        /// <param name="frame">The frame to encode.</param>
        /// <returns>A byte array containing the SLIP-encoded frame.</returns>
        private byte[] Encode(Frame frame)
        {
            List<byte> encoded = new List<byte> { FrameDelimiter };

            foreach (byte b in frame.Data)
            {
                if (b == FrameDelimiter)
                {
                    encoded.Add(EscapeByte);
                    encoded.Add(EscapeFrameDelimiter);
                }
                else if (b == EscapeByte)
                {
                    encoded.Add(EscapeByte);
                    encoded.Add(EscapeEscapeByte);
                }
                else
                {
                    encoded.Add(b);
                }
            }

            encoded.Add(FrameDelimiter);
            return encoded.ToArray();
        }

        /// <summary>
        /// Decodes the SLIP-encoded data into a Frame.
        /// </summary>
        /// <param name="data">The SLIP-encoded byte array.</param>
        /// <returns>The decoded frame.</returns>
        private Frame? Decode(byte[] data)
        {
            List<byte> decoded = new List<byte>();

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == EscapeByte)
                {
                    i++;
                    if (i >= data.Length) break;

                    if (data[i] == EscapeFrameDelimiter)
                    {
                        decoded.Add(FrameDelimiter);
                    }
                    else if (data[i] == EscapeEscapeByte)
                    {
                        decoded.Add(EscapeByte);
                    }
                }
                else
                {
                    decoded.Add(data[i]);
                }
            }

            return new Frame(decoded.ToArray());
        }
    }
}
