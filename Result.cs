namespace Diabase.ResultType
{
    public record Result
    {
        private const string noErrorMessage = "(no error message)";
        public virtual void ThrowIfFailed()
        {
            // do nothing
        }

        public virtual bool Succeeded => false;
        public virtual bool Failed => false;
        public virtual string? Message => null;
        public virtual Exception? Exception => null;

        public static SuccessResult Success { get; } = new SuccessResult();
        public static Result<T> Value<T>(T value) => new(value);
        public static Result<TValue, TMeta> Value<TValue, TMeta>(TValue value, TMeta meta) => new(value, meta);
        public static NoResult NoResult(string? message = null) => new(message);
        public static FailureResult Failure(string? message) => new(message ?? noErrorMessage);
        public static FailureResultWithMeta<TMeta> Failure<TMeta>(string message, TMeta meta) => new(message, meta);

        public static implicit operator Result(Exception exception) => new ExceptionResult(exception);
    }

    public record SuccessResult : Result
    {
        public override bool Succeeded => true;
        public override bool Failed => false;
    }

    public record NoResult : Result
    {
        public NoResult(string? message = null)
        {
            this.message = message;
        }

        private readonly string? message;
        public override string? Message => message;
    }

    public record FailureResult : Result
    {
        private readonly string message;
        public FailureResult(string message)
        {
            this.message = message;
        }

        public override void ThrowIfFailed()
        {
            throw new ResultException(message);
        }

        public override string? Message => message;
        public string FailureMessage => message;
        public override Exception? Exception => new ResultException(message);
        public override bool Failed => true;
    }

    public record FailureResultWithMeta<TMeta> : Result
    {
        private readonly string message;
        public TMeta? Meta { get; init; }
        public FailureResultWithMeta(string message)
        {
            this.message = message;
        }
        public FailureResultWithMeta(string message, TMeta? meta)
        {
            this.message = message;
            Meta = meta;
        }
        public override string? Message => message;
        public string FailureMessage => message;
        public override Exception? Exception => new ResultException(message);
    }

    public record ExceptionResult : FailureResult
    {
        private readonly Exception exception;
        public override Exception? Exception => exception;
        public ExceptionResult(Exception exception) : base(exception.Message)
        {
            this.exception = exception;
        }
        public override void ThrowIfFailed()
        {
            throw exception;
        }
    }

    public class ResultException(string message) : Exception(message)
    {
    }
   
}
