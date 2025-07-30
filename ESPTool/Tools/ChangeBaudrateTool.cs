using EspDotNet.Loaders;

namespace EspDotNet.Tools
{
    public class ChangeBaudRateTool
    {
        private readonly ILoader _loader;
        public ChangeBaudRateTool(ILoader loader)
        {
            _loader = loader;
        }

        /// <summary>
        /// Instructs the device to change baud rate via the loader. Caller must reconfigure the serial port after this.
        /// </summary>
        public async Task ChangeBaudAsync(int newBaud, int currentBaud, CancellationToken token)
        {
            if (newBaud == currentBaud)
                return;

            await _loader.ChangeBaudAsync(newBaud, currentBaud, token);
        }
    }

}
