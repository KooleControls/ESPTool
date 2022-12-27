using System;
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
        }

        public int GetBaud()
        {
            return uart.BaudRate;
        }





        public Task<Result> EnterBootloader(CancellationToken ct = default)
        {
            //Reset
            uart.DtrEnable = false;
            uart.RtsEnable = true;

            //Hold boot pin
            uart.DtrEnable = true;
            uart.RtsEnable = false;

            bool cancelled = ct.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(500)); //Determained by trial and error. 250ms didnt work. We could wait for the uart to say "waiting for download"

            //Release boot pin
            uart.DtrEnable = false;
            uart.RtsEnable = false;

            return Task.FromResult(cancelled ? Result.TaskCanceled : Result.OK);
        }

        public Task<Result> Reset(CancellationToken ct = default)
        {
            //Reset
            uart.DtrEnable = false;
            uart.RtsEnable = true;

            bool cancelled = ct.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(500));

            //Release reset pin
            uart.DtrEnable = false;
            uart.RtsEnable = false;

            return Task.FromResult(cancelled ? Result.TaskCanceled : Result.OK);
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
