using ESPTool.Com;
using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace ESPTool.Com
{
    public class Communicator : IDisposable
    {
        private readonly SerialPort _serialPort;
        private readonly SlipFraming _slipFraming;

        public Communicator()
        {
            _serialPort = new SerialPort();
            _slipFraming = new SlipFraming(_serialPort.BaseStream);
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
            {
                _serialPort.Close();
                _serialPort.BaudRate = baudRate;
                _serialPort.Open();
            }
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

        /// <summary>
        /// Executes the bootloader entry sequence.
        /// </summary>
        /// <param name="token">Cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via the cancellation token.</exception>
        /// <exception cref="InvalidOperationException">Thrown if an error occurs while entering the bootloader.</exception>
        public async Task EnterBootloaderAsync(CancellationToken token)
        {
            try
            {
                // Ensure we start in the original state
                _serialPort.DtrEnable = false;
                _serialPort.RtsEnable = false;
                await Task.Delay(50, token);  // Replaces WaitOne

                // Execute sequence
                _serialPort.RtsEnable = true;
                await Task.Delay(100, token);  // Replaces WaitOne
                _serialPort.DtrEnable = true;
                _serialPort.RtsEnable = false;
                await Task.Delay(50, token);  // Replaces WaitOne
                _serialPort.DtrEnable = false;

                token.ThrowIfCancellationRequested();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to enter bootloader", ex);
            }
        }

        /// <summary>
        /// Resets the device by toggling the RTS and DTR pins.
        /// </summary>
        /// <param name="token">Cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via the cancellation token.</exception>
        /// <exception cref="InvalidOperationException">Thrown if an error occurs while resetting the device.</exception>
        public async Task ResetDeviceAsync(CancellationToken token)
        {
            try
            {
                // Reset
                _serialPort.DtrEnable = false;
                _serialPort.RtsEnable = true;

                await Task.Delay(500, token);  // Replaces WaitOne

                // Release reset pin
                _serialPort.DtrEnable = false;
                _serialPort.RtsEnable = false;

                token.ThrowIfCancellationRequested();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to reset the device", ex);
            }
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
    }
}
