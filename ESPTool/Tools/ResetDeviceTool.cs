using EspDotNet.Communication;
using EspDotNet.Config;

namespace EspDotNet.Tools
{
    public class ResetDeviceTool
    {
        private readonly Communicator _communicator;
        private readonly List<PinSequenceStep> _resetDeviceSequence;

        public ResetDeviceTool(Communicator communicator, List<PinSequenceStep> resetDeviceSequence)
        {
            _communicator = communicator;
            _resetDeviceSequence = resetDeviceSequence;
        }

        public async Task ResetAsync(CancellationToken token = default)
        {
            await _communicator.ExecutePinSequence(_resetDeviceSequence, token);
        }
    }

}