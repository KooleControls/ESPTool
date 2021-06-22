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





        public async Task<Result> EnterBootloader(CancellationToken ct = default(CancellationToken))
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

            return cancelled ? Result.TaskCanceled : Result.OK;
        }

        public async Task<Result> Reset(CancellationToken ct = default(CancellationToken))
        {
            //Reset
            uart.DtrEnable = false;
            uart.RtsEnable = true;

            bool cancelled = ct.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(500));

            //Release reset pin
            uart.DtrEnable = false;
            uart.RtsEnable = false;

            return cancelled ? Result.TaskCanceled : Result.OK;
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
                int d = 0;
                while ((d = uart.ReadByte()) != -1)
                    framing.Decode((byte)d);
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
