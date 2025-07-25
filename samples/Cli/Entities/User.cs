using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cli.Entities
{
    internal sealed class User
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public User()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        { }

        private User(
            UserId id,
            string emailAddress,
            string firstName,
            string lastName)
        {
            Id = id;
            EmailAddress = emailAddress;
            FirstName = firstName;
            LastName = lastName;
        }

        public UserId Id { get; private set; }

        public string EmailAddress { get; private set; }

        public string FirstName { get; private set; }

        public string LastName { get; private set; }

        public static User Create(
            string emailAddress,
            string firstName,
            string lastName,
            UserId? userId = null)
        {
            return new(
                id: userId ?? new(Guid.NewGuid()),
                emailAddress: emailAddress,
                firstName: firstName,
                lastName: lastName);
        }
    }

    internal sealed record UserId(Guid Value);

    internal sealed class UserIdJsonConverter : JsonConverter<UserId>
    {
        public override UserId? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var userGuid = JsonSerializer.Deserialize<Guid?>(ref reader, options);

            return userGuid.HasValue
                ? new UserId(userGuid.Value)
                : null;
        }

        public override void Write(Utf8JsonWriter writer, UserId value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value.Value, options);
        }
    }
}
