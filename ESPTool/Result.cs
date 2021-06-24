using ESPTool.CMD;

namespace ESPTool
{
    public class Result<T> : Result
    {
        public T Value { get; set; }

        public override string ToString()
        {
            return Success ? "Success: " + Value.ToString() : "Error: " + Error.ToString();
        }
    }

    public class Result
    {
        public bool Success { get; set; }
        public Errors Error { get; set; }

        public static Result OK { get => new Result { Error = Errors.NoError, Success = true }; }
        public static Result TaskCanceled { get => new Result { Error = Errors.TaskCancelled, Success = false }; }
        public static Result UnsupportedByDevice { get => new Result { Error = Errors.UnsupportedByDevice, Success = false }; }
        public static Result UnsupportedByLoader { get => new Result { Error = Errors.UnsupportedByLoader, Success = false }; }
        public static Result WrongChip { get => new Result { Error = Errors.WrongChip, Success = false }; }
        public static Result UnknownError { get => new Result { Error = Errors.Unknown, Success = true }; }

        public override string ToString()
        {
            return Success ? "Success" : "Error: " + Error.ToString();
        }
    };
}

