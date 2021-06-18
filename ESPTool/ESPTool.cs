using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ESPTool
{
    public class ESPTool
    {


        public ESPTool()
        {
            //programmer.OpenSerial("COM30", 115200);
        }






        /*
        public async void FlashFirmware()
        {
            bool suc = true;

            if (suc) suc &= await device.EnterBootloader();
            if (suc) suc &= await device.Sync();

            ChipType type = await device.GetChipType();

            switch(type)
            {
                case ChipType.ESP32:
                    device = new ESP32(programmer);
                    break;
                default:
                    throw new NotImplementedException();
            }

            if (suc) suc &= await device.StartStubloader();
            if (suc) suc &= await device.ChangeBaud(921600);

        }
        */
    }
}
