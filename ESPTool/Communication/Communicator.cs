﻿using EspDotNet.Config;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EspDotNet.Communication
{
    public class Communicator : IDisposable
    {
        private readonly SerialPort _serialPort;
        private readonly SlipFraming _slipFraming;

        public Communicator()
        {
            _serialPort = new SerialPort();
            _slipFraming = new SlipFraming(_serialPort);
        }

        /// <summary>
        /// Opens the serial port with the specified port name and baud rate.
        /// </summary>
        /// <param name="portName">The name of the serial port to open.</param>
        /// <param name="baudRate">The baud rate for the communication.</param>
        public void OpenSerial(string portName, int baudRate)
        {
            if (_serialPort.IsOpen && (_serialPort.PortName != portName || _serialPort.BaudRate != baudRate))
            {
                _serialPort.Close();
            }

            if (!_serialPort.IsOpen)
            {
                _serialPort.PortName = portName;
                _serialPort.BaudRate = baudRate;
                _serialPort.Open();
            }
        }

        /// <summary>
        /// Closes the serial port.
        /// </summary>
        public void CloseSerial()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }

        /// <summary>
        /// Gets the current baud rate of the serial port.
        /// </summary>
        /// <returns>The baud rate of the serial port.</returns>
        public int GetBaudRate() => _serialPort.BaudRate;

        /// <summary>
        /// Changes the baud rate of the serial port.
        /// </summary>
        /// <param name="baudRate">The new baud rate for the communication.</param>
        public void ChangeBaudRate(int baudRate)
        {
            if (_serialPort.IsOpen)
                _serialPort.Close();

            _serialPort.BaudRate = baudRate;
            _serialPort.Open();
            
        }

        /// <summary>
        /// Clears the input buffer of the serial port.
        /// </summary>
        public void ClearBuffer()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.DiscardInBuffer();
            }
        }

        /// <summary>
        /// Writes a SLIP-encoded frame asynchronously to the stream.
        /// </summary>
        /// <param name="frame">The frame to send.</param>
        /// <param name="token">Cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via the cancellation token.</exception>
        public async Task WriteFrameAsync(Frame frame, CancellationToken token)
        {
            await _slipFraming.WriteFrameAsync(frame, token);
        }

        /// <summary>
        /// Reads and decodes a SLIP-encoded frame asynchronously from the stream.
        /// Reads until the end of the frame is received.
        /// </summary>
        /// <param name="token">Cancellation token to cancel the operation.</param>
        /// <returns>The frame received, or null if no frame is available.</returns>
        /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via the cancellation token.</exception>
        public async Task<Frame?> ReadFrameAsync(CancellationToken token)
        {
            return await _slipFraming.ReadFrameAsync(token);
        }

        public async Task ExecutePinSequence(List<PinSequenceStep> sequence, CancellationToken token)
        {
            foreach (var step in sequence)
            {
                if (step.Dtr != null)
                    _serialPort.DtrEnable = step.Dtr.Value;

                if (step.Rts != null)
                {
                    _serialPort.RtsEnable = step.Rts.Value;
                    if (OperatingSystem.IsWindows())
                        _serialPort.DtrEnable = _serialPort.DtrEnable;
                }

                await Task.Delay(step.Delay, token);
            }
        }

        public async Task<int> ReadRawAsync(byte[] buffer, CancellationToken token)
        {
            return await _serialPort.BaseStream.ReadAsync(buffer, 0, buffer.Length, token);
        }

        /// <summary>
        /// Writes raw data asynchronously to the serial port.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <param name="token">Cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via the cancellation token.</exception>
        public async Task WriteAsync(byte[] data, CancellationToken token)
        {
            await _serialPort.BaseStream.WriteAsync(data, 0, data.Length, token);
        }


        /// <summary>
        /// Disposes the serial port and associated resources.
        /// </summary>
        public void Dispose()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
            _serialPort.Dispose();
        }

        internal async Task FlushAsync(CancellationToken token)
        {
            await _serialPort.BaseStream.FlushAsync(token);
        }
    }
}
