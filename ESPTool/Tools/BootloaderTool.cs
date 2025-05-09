using EspDotNet.Communication;
using EspDotNet.Config;
using EspDotNet.Loaders.ESP32BootLoader;
using System.Text.RegularExpressions;
using System.Text;

namespace EspDotNet.Tools
{
    public class BootloaderTool
    {
        private readonly Communicator _communicator;
        private readonly List<PinSequenceStep> _bootloaderSequence;

        public BootloaderTool(Communicator communicator, List<PinSequenceStep> bootloaderSequence)
        {
            _communicator = communicator;
            _bootloaderSequence = bootloaderSequence;
        }

        public async Task<ESP32BootLoader> StartAsync(CancellationToken token = default)
        {
            // Start bootloader
            await _communicator.ExecutePinSequence(_bootloaderSequence, token);

            // Check bootloader message
            if (!await TryReadBootStartup(token))
                throw new Exception("Booloader message not verified");

            // Instantiate loader and synchronize
            var bootloader = new ESP32BootLoader(_communicator);

            if (!await Synchronize(bootloader, token))
                throw new Exception("Failed to synchronize with bootloader");

            return bootloader;
        }

        private async Task<bool> TryReadBootStartup(CancellationToken token)
        {
            var buffer = new byte[4096];
            var read = await _communicator.ReadRawAsync(buffer, token);
            if (read > 0)
            {
                var data = new byte[read];
                Array.Copy(buffer, data, read);
                Regex regex = new Regex("boot:(0x[0-9a-fA-F]+)(.*waiting for download)?");
                var result = regex.Match(Encoding.ASCII.GetString(data));
                return result.Success;
            }
            return false;
        }

        private async Task<bool> Synchronize(ESP32BootLoader loader, CancellationToken token)
        {
            for (int tryNo = 0; tryNo < 100; tryNo++)
            {
                token.ThrowIfCancellationRequested();

                // Try to sync for 100ms.
                using CancellationTokenSource cts = new CancellationTokenSource();

                // Register the token and store the registration to dispose of it later
                using CancellationTokenRegistration ctr = token.Register(() => cts.Cancel());

                cts.CancelAfter(100); // Cancel after 100ms

                try
                {
                    if(await loader.SynchronizeAsync(cts.Token))
                        return true;
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    ctr.Unregister();
                }
            }

            return false;
        }
    }
}