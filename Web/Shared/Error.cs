using System.Text.Json.Serialization;

namespace Web.Shared
{
    public class Error : IEquatable<Error>
    {
        private Error(string code, string description, ErrorType type)
        {
            Code = code;
            Description = description;
            Type = type;
        }

        public string Code { get; }

        public string Description { get; }

        [JsonIgnore]
        public ErrorType Type { get; }

        public static readonly Error Generic = new($"{nameof(Error)}.{nameof(Generic)}", "An error occurred.", ErrorType.Failure);

        public static readonly Error GenericValidation = new($"{nameof(Error)}.Validation", "A validation error occurred.", ErrorType.Validation);

        public static Error Failure(string code, string description)
        {
            return new(code, description, ErrorType.Failure);
        }

        public static Error Validation(string code, string description)
        {
            return new(code, description, ErrorType.Validation);
        }

        public static Error NotFound(string code, string description)
        {
            return new(code, description, ErrorType.NotFound);
        }

        public static Error Conflict(string code, string description)
        {
            return new(code, description, ErrorType.Conflict);
        }

        public static Error Unauthorized(
            string code,
            string description)
        {
            return new(code, description, ErrorType.Unauthorized);
        }

        public static readonly Error Forbidden = new(nameof(Forbidden), "The request has not been applied because it lacks sufficient authentication credentials for the target resource.", ErrorType.Forbidden);

        public virtual bool Equals(Error? other)
        {
            if (other is null)
            {
                return false;
            }

            return Code == other.Code && Description == other.Description;
        }

        public override bool Equals(object? obj)
        {
            return obj is Error error && Equals(error);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Code, Description);
        }
    }

    public enum ErrorType
    {
        Failure,
        Validation,
        NotFound,
        Conflict,
        Unauthorized,
        Forbidden
    }
}
