namespace Diabase.ResultType
{
    public record Result<TValue> : Result
    {
        public Result(TValue? value)
        {
            this.value = value;
            hasValue = value is not null;
        }

        public Result()
        {
        }

        private readonly bool hasValue;
        private readonly TValue? value;
        public bool HasValue => hasValue && value is not null;
        public override bool Succeeded => hasValue;
        public override bool Failed => !hasValue;
        public virtual TValue Value
        {
            get
            {
                if (!HasValue) throw new ResultException($"Result type has no value for {typeof(TValue).Name}.");
                return value!;
            }
        }

        public static implicit operator Result<TValue>(TValue? value) { return new Result<TValue>(value); }
        public static implicit operator TValue(Result<TValue> result) { return result.Value; }
        public static implicit operator Result<TValue>(NoResult value) { return new NoResult<TValue>(value.Message); }
        public static implicit operator Result<TValue>(FailureResult value) { return new FailureResult<TValue>(value.FailureMessage); }
        public static implicit operator Result<TValue>(Exception exception) { return new ExceptionResult<TValue>(exception); }
    }

    public record NoResult<T> : Result<T>
    {
        public NoResult(string? message = null) : base()
        {
            this.message = message;
        }

        private readonly string? message;
        public override string? Message => message;
    }

    public record FailureResult<T> : Result<T>
    {
        public FailureResult(string message) : base(default(T?))
        {
            this.message = message;
        }

        private readonly string message;
        public override string? Message => message;

        public override bool Succeeded => false;
        public override bool Failed => true;

        public override T Value => throw new ResultException(message);

        public override void ThrowIfFailed()
        {
            throw new ResultException(message);
        }

        public override Exception? Exception => new ResultException(message);
    }

    public record ExceptionResult<T> : FailureResult<T>
    {
        public ExceptionResult(Exception exception) : base(exception.Message)
        {
            this.exception = exception;
        }
        private readonly Exception exception;
        public override Exception? Exception => exception;
        public override T Value => throw exception;

        public override void ThrowIfFailed()
        {
            throw exception;
        }
    }
}
