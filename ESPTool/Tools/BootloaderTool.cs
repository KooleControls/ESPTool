using EspDotNet.Communication;
using EspDotNet.Config;
using EspDotNet.Loaders.ESP32BootLoader;

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

            // Instantiate loader and synchronize
            var bootloader = new ESP32BootLoader(_communicator);

            if (!await Synchronize(bootloader, token))
                throw new Exception("Failed to synchronize with bootloader");

            return bootloader;
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