using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace ESPTool.Com
{
    /// <summary>
    /// Is responsible for the low level RS232 communication and the SLIP framing.
    /// </summary>
    public class Communicator
    {
        SerialPort uart = new SerialPort();
        SlipFraming framing = new SlipFraming();

        public event EventHandler<Frame> FrameRecieved;

        public Communicator()
        {
            uart.DataReceived += Uart_DataReceived;
            framing.FrameRecieved += Framing_FrameRecieved;
        }


        ~Communicator()
        {
            uart.Close();
        }

        public void ClearBuffer()
        {
            if(uart.IsOpen)
                uart.DiscardInBuffer();
            framing.ClearBuffer();
        }

        public void OpenSerial(string name, int baud)
        {
            if (uart.IsOpen && (uart.PortName != name || uart.BaudRate != baud))
            {
                uart.Close();
            }

            if (!uart.IsOpen)
            {
                uart.PortName = name;
                uart.BaudRate = baud;
                uart.Open();
                framing.ClearBuffer();
            }
        }

        public void CloseSerial()
        {
            uart.Close();
        }

        public void ChangeBaud(int baudrate)
        {
            uart.Close();
            uart.BaudRate = baudrate;
            uart.Open();
            framing.ClearBuffer();
        }

        public int GetBaud()
        {
            return uart.BaudRate;
        }



        public async Task<Result> EnterBootloader(CancellationToken ct = default)
        {
            // Taken from: https://github.com/espressif/esptool/blob/master/esptool/reset.py line 92 (function ClassicReset)

            // Ensure we start in the origional state
            uart.DtrEnable = false;
            uart.RtsEnable = false;
            ct.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(50));

            // Execute sequence
            uart.RtsEnable = true;
            ct.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(100));
            uart.DtrEnable = true;
            uart.RtsEnable = false;
            ct.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(50));
            uart.DtrEnable = false;


            return await Task.FromResult(ct.IsCancellationRequested ? Result.TaskCanceled : Result.OK);
        }

        public async Task<Result> Reset(CancellationToken ct = default)
        {
            //Reset
            uart.DtrEnable = false;
            uart.RtsEnable = true;

            bool cancelled = ct.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(500));

            //Release reset pin
            uart.DtrEnable = false;
            uart.RtsEnable = false;

            return await Task.FromResult(cancelled ? Result.TaskCanceled : Result.OK);
        }

        public void SendFrame(Frame frame)
        {
            byte[] data = framing.Encode(frame);
            uart.Write(data, 0, data.Length);
        }

        private void Uart_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //@TODO, when changing the baudrate, closing and opening the port. This throws an error. Should handle this better.
            try
            {
                while(uart.BytesToRead > 0)
                {
                    byte[] rx = new byte[uart.BytesToRead];
                    uart.Read(rx, 0, rx.Length);
                    framing.Decode(rx);
                }  
            }
            catch
            {

            }
        }

        private void Framing_FrameRecieved(object sender, Frame e)
        {
            FrameRecieved?.Invoke(this, e);
        }
    }
}
