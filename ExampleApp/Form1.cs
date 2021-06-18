using ESPTool.Com;
using ESPTool.Devices;
using ESPTool.Loaders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExampleApp
{
    public partial class Form1 : Form
    {
        Device dev = new Device();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            AddButton("Open com", async () => 
            {
                richTextBox1.AppendText("Opening com\r\n");
                dev.OpenSerial("COM30", 115200);
            });

            AddButton("Enter Bootloader", async () =>
            {
                richTextBox1.AppendText("Entering bootloader ");
                bool suc = await dev.EnterBootloader();
                richTextBox1.AppendText($"{(suc?"OKE":"FAIL")}\r\n");
            });

            AddButton("Sync", async () =>
            {
                richTextBox1.AppendText("Syncing ");
                bool suc = await dev.Sync();
                richTextBox1.AppendText($"{(suc ? "OKE" : "FAIL")}\r\n");
            });

            AddButton("Detect chiptype", async () =>
            {
                richTextBox1.AppendText("GetChipType ");
                ChipTypes type = await dev.DetectChipType();
                richTextBox1.AppendText($"{type.ToString()}\r\n");
            });
        }
        

        void AddButton(string text, Func<Task> act)
        {
            Button btn = new Button();
            btn.Width = flowLayoutPanel1.Width - flowLayoutPanel1.Margin.Left - flowLayoutPanel1.Margin.Right;
            btn.Text = text;
            btn.Click += async (sender, e) => 
            {
                flowLayoutPanel1.Enabled = false;
                await act();
                flowLayoutPanel1.Enabled = true;
            };
            flowLayoutPanel1.Controls.Add(btn);
        }

        void AddButton(string text, Action act)
        {
            AddButton(text, new Task(act).Start);
        }

    }
}
