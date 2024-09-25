namespace ESPTool.Commands
{
    public class RequestCommandBuilder
    {
        private readonly RequestCommand _request;

        public RequestCommandBuilder()
        {
            _request = new RequestCommand();
        }

        /// <summary>
        /// Sets the command.
        /// </summary>
        /// <param name="cmd">The command byte.</param>
        public RequestCommandBuilder WithCommand(byte cmd)
        {
            _request.Command = cmd;
            return this;
        }

        /// <summary>
        /// Appends a byte array to the payload.
        /// </summary>
        public RequestCommandBuilder AppendPayload(byte[] payloadPart)
        {
            _request.Payload = _request.Payload.Concat(payloadPart).ToArray();
            _request.Size = (ushort)_request.Payload.Length;
            return this;
        }

        /// <summary>
        /// Sets whether a checksum is required.
        /// </summary>
        /// <param name="checksumRequired">True if a checksum is required, false otherwise.</param>
        public RequestCommandBuilder RequiresChecksum(bool checksumRequired = true)
        {
            _request.ChecksumRequired = checksumRequired;
            return this;
        }

        /// <summary>
        /// Sets the direction byte.
        /// </summary>
        /// <param name="direction">The direction byte.</param>
        public RequestCommandBuilder WithDirection(byte direction)
        {
            _request.Direction = direction;
            return this;
        }

        /// <summary>
        /// Builds and returns the final RequestCMD object.
        /// </summary>
        public RequestCommand Build()
        {
            if (_request.ChecksumRequired)
            {
                CalculateChecksum();
            }
            return _request;
        }

        /// <summary>
        /// Calculates the checksum for the payload if required.
        /// </summary>
        private void CalculateChecksum()
        {
            _request.Checksum = 0xEF;

            for (int i = 16; i < _request.Payload.Length; i++)
            {
                _request.Checksum ^= _request.Payload[i];
            }
        }
    }
}
