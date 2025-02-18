using ESPTool.Loaders.SoftLoader;

namespace ESPTool.Models
{
    public class PinSequenceStep
    {
        public bool Dtr { get; set; }

        public bool Rts { get; set; }

        public int DelayMs { get; set; }
    }
}
