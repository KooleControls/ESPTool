namespace EspDotNet.Config
{
    public class PinSequenceStep
    {
        public bool Dtr { get; set; }

        public bool Rts { get; set; }

        public TimeSpan Delay { get; set; }
    }
}
