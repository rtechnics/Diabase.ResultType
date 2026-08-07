namespace Diabase.ResultType
{
    public record Result<TValue, TMeta> : Result
    {
        public Result(TValue? value, TMeta? meta)
        {
            this.value = value;
            hasValue = value is not null;
            this.meta = meta;
        }

        public Result(TMeta? meta)
        {
            this.meta = meta;
        }

        public Result()
        {
            // do nothing
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

        private readonly TMeta? meta;
        public bool HasMeta => meta is not null;
        public TMeta Meta
        {
            get
            {
                if (meta is null) throw new ResultException($"Result type has no meta for {typeof(TMeta).Name}.");
                return meta;
            }
        }
        public TMeta? MetaOrNull => meta;

        public static string? ExtractResultMessage<T>(T? obj)
        {
            if (obj is IResultMessageGetter messageGetter) return messageGetter.Message;
            return null;
        }

        public static implicit operator Result<TValue, TMeta>(TValue? value) { return new Result<TValue, TMeta>(value, default); }
        public static implicit operator Result<TValue, TMeta>((TValue? value, TMeta? meta) tuple) { return new Result<TValue, TMeta>(tuple.value, tuple.meta); }
        public static implicit operator Result<TValue, TMeta>(TMeta? value) { return new NoResult<TValue, TMeta>(value, ExtractResultMessage(value)); }
        public static implicit operator TValue(Result<TValue, TMeta> result) { return result.Value; }
        public static implicit operator Result<TValue, TMeta>(NoResult _) { return new NoResult<TValue, TMeta>(); }
        public static implicit operator Result<TValue, TMeta>(FailureResult value) { return new FailureResult<TValue, TMeta>(value.FailureMessage); }
        public static implicit operator Result<TValue, TMeta>(FailureResultWithMeta<TMeta> value) { return new FailureResult<TValue, TMeta>(value.FailureMessage, value.Meta); }
        public static implicit operator Result<TValue, TMeta>(Exception exception) { return new ExceptionResult<TValue, TMeta>(exception); }
    }

    public record NoResult<TValue, TMeta> : Result<TValue, TMeta>
    {
        public NoResult(TMeta? meta = default, string? message = null) : base(meta)
        {
            this.message = message;
        }

        private readonly string? message;
        public override string? Message => message;
    }

    public record FailureResult<TValue, TMeta> : Result<TValue, TMeta>
    {
        public FailureResult(string message) : base(default, default)
        {
            this.message = message;
        }

        public FailureResult(string message, TMeta? meta) : base(default, meta)
        {
            this.message = message;
        }

        private readonly string message;
        public override string? Message => message;
        public string FailureMessage => message;
        public override TValue Value => throw new ResultException(message);

        public override bool Succeeded => false;
        public override bool Failed => true;

        public override void ThrowIfFailed()
        {
            throw new ResultException(message);
        }
    }

    public record ExceptionResult<TValue, TMeta> : FailureResult<TValue, TMeta>
    {
        public ExceptionResult(Exception exception) : base(exception.Message, default)
        {
            this.exception = exception;
        }
        private readonly Exception exception;
        public override Exception? Exception => exception;
        public override TValue Value => throw exception;

        public override void ThrowIfFailed()
        {
            throw exception;
        }
    }
}
