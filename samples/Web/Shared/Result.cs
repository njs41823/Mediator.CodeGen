using System.Text.Json;
using System.Text.Json.Serialization;

namespace Web.Shared
{
    public class Result
    {
        private readonly Error? error;

        protected Result(bool isSuccess, Error? error)
        {
            if (isSuccess && error is not null)
            {
                throw new InvalidOperationException($"A successful {nameof(Result)} cannot have an {nameof(Error)}.");
            }

            if (!isSuccess && error is null)
            {
                throw new InvalidOperationException($"An unsuccessful {nameof(Result)} must have an {nameof(Error)}.");
            }

            IsSuccess = isSuccess;
            this.error = error;
        }

        public bool IsSuccess { get; }

        public Error Error => !IsSuccess
            ? error!
            : throw new InvalidOperationException($"The {nameof(Error)} of a successful {nameof(Result)} can not be accessed.");

        public static readonly Result Success = new(true, null);

        public static readonly Result GenericFailure = new(false, Error.Generic);

        public static Result Failure(Error error)
        {
            return new(false, error);
        }
    }

    public class Result<TValue> : Result
        where TValue : IEquatable<TValue>
    {
        private readonly TValue? value;

        internal Result(bool isSuccess, Error? error, TValue? value)
            : base(isSuccess, error)
        {
            if (isSuccess && value is null)
            {
                throw new InvalidOperationException($"A successful {nameof(Result<TValue>)} must have a {nameof(Value)}.");
            }

            if (!isSuccess && !EqualityComparer<TValue>.Default.Equals(value, default))
            {
                throw new InvalidOperationException($"An unsuccessful {nameof(Result<TValue>)} cannot have a {nameof(Value)}.");
            }

            this.value = value;
        }

        public TValue Value => IsSuccess
            ? value!
            : throw new InvalidOperationException($"The {nameof(Value)} of a failure {nameof(Result)} can not be accessed.");

        public new static Result<TValue> Success(TValue value)
        {
            return new(true, null, value);
        }

        public new static readonly Result<TValue> GenericFailure = Failure(Error.Generic);

        public new static Result<TValue> Failure(Error error)
        {
            return new(false, error, default);
        }
    }

    public class ResultJsonConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (typeToConvert == typeof(Result))
            {
                return true;
            }

            return
                !typeToConvert.IsGenericTypeDefinition &&
                typeToConvert.IsGenericType &&
                typeToConvert.GetGenericTypeDefinition() == typeof(Result<>);
        }

        public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
        {
            if (!type.IsGenericType)
            {
                return new ResultJsonConverter();
            }

            var converterType = typeof(ResultJsonConverter<>).MakeGenericType(type.GetGenericArguments());

            return (JsonConverter)Activator.CreateInstance(converterType)!;
        }
    }

    public class ResultJsonConverter : JsonConverter<Result>
    {
        public override Result? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<Result>(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, Result value, JsonSerializerOptions options)
        {
            var proxy = new
            {
                value.IsSuccess,
                Error = value.IsSuccess
                    ? null
                    : value.Error
            };

            JsonSerializer.Serialize(writer, proxy, options);
        }
    }

    public class ResultJsonConverter<TValue> : JsonConverter<Result<TValue>>
        where TValue : IEquatable<TValue>
    {
        public override Result<TValue>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<Result<TValue>>(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, Result<TValue> value, JsonSerializerOptions options)
        {
            var proxy = new
            {
                value.IsSuccess,
                Error = value.IsSuccess
                    ? null
                    : value.Error,
                Value = value.IsSuccess
                    ? value.Value
                    : default
            };

            JsonSerializer.Serialize(writer, proxy, options);
        }
    }
}
