using ESPTool.Devices;
using ESPTool.Firmware;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
                await dev.OpenSerial("COM56", 115200);
            });

            AddButton("Enter Bootloader", async () =>
            {
                richTextBox1.AppendText("Entering bootloader ");
                var suc = await dev.EnterBootloader();
                richTextBox1.AppendText($"{(suc.Success ? "OKE" : "FAIL")}\r\n");
            });

            AddButton("Sync", async () =>
            {
                richTextBox1.AppendText("Syncing ");
                var suc = await dev.Sync();
                richTextBox1.AppendText($"{(suc.Success ? "OKE" : "FAIL")}\r\n");
            });

            AddButton("Detect chiptype", async () =>
            {
                richTextBox1.AppendText("GetChipType ");
                var type = await dev.DetectChipType();
                richTextBox1.AppendText($"{type.ToString()}\r\n");

                if (type.Value == ChipTypes.ESP32)
                {
                    dev = new ESP32(dev);
                    richTextBox1.AppendText($"Changing device to ESP32\r\n");
                }
            });

            AddButton("Upload stubloader", async () =>
            {
                if (dev is ESP32 esp)
                {
                    richTextBox1.AppendText("Uploading stubloader ");
                    var suc = await esp.StartStubloader();
                    richTextBox1.AppendText($"{(suc.Success ? "OKE, stubloader is running" : "FAIL")}\r\n");
                }
                else
                    richTextBox1.AppendText($"Device type isn't ESP32. Please detect first. \r\n");
            });

            AddButton("Change baudrate", async () =>
            {
                richTextBox1.AppendText("Changing baud to '921600'");
                var suc = await dev.ChangeBaud(921600);
                richTextBox1.AppendText($"{(suc.Success ? "OKE" : "FAIL")}\r\n");
            });

            AddButton("Upload firmware", async () =>
            {
                if (dev is ESP32 esp)
                {
                    richTextBox1.AppendText("Uploading firmware\r\n");



                    FirmwareImage image = new FirmwareImage();
                    image.Segments = new List<Segment>();

                    foreach (string file in Directory.GetFiles("Firmware", "*.bin"))
                    {
                        FileInfo bin = new FileInfo(file);

                        UInt32 offset = 0;

                        switch (Path.GetFileNameWithoutExtension(file))
                        {

                            case "bootloader":
                                offset = 0x1000;
                                break;

                            case "Firmware":
                                offset = 0x10000;
                                break;
                            case "storage":
                                offset = 0x110000;
                                break;
                            case "partition-table":
                                offset = 0x8000;
                                break;

                        }

                        using (Stream stream = bin.OpenRead())
                        {
                            byte[] data = new byte[stream.Length];
                            stream.Read(data, 0, data.Length);


                            Segment seg = new Segment();
                            seg.Offset = offset;
                            seg.Data = data;

                            image.Segments.Add(seg);
                        }
                    }

                    var progress = new Progress<float>(prog =>
                    {
                        progressBar1.InvokeIfRequired(() => { progressBar1.Value = (int)(prog * 100); });
                    });

                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    var suc = await esp.UploadToFLASHDeflated(image, false, default, progress);
                    sw.Stop();
                    richTextBox1.AppendText($"Uploading {(suc.Success ? "OKE" : "FAILED")}. It took {sw.ElapsedMilliseconds}ms \r\n");

                }
                else
                    richTextBox1.AppendText($"Device type isn't ESP32. Please detect first. \r\n");

            });
        }

        void Progress(double prog)
        {
            progressBar1.InvokeIfRequired(() => { progressBar1.Value = (int)(prog * 100); });
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
