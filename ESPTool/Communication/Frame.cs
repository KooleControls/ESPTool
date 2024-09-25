namespace ESPTool.Communication
{
    public class Frame
    {
        public byte[] Data { get; set; }

        public Frame()
        {

        }
        public Frame(byte[] data)
        {
            Data = data;
        }
    }
}
