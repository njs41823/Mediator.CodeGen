using Cli.UseCases.GetUserById;
using Cocona;
using Mediator.CodeGen.Contracts;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Cli
{
    public sealed class Commands(
        ISender sender,
        JsonSerializerOptions jsonSerializerOptions,
        ILogger<Commands> logger)
    {
        private readonly ISender sender = sender;

        private readonly JsonSerializerOptions jsonSerializerOptions = jsonSerializerOptions;

        private readonly ILogger<Commands> logger = logger;

        public async Task GetUserById([Argument] Guid userId)
        {
            var getUserByIdRequest = new GetUserByIdRequest(new(userId));

            var getUserByIdResponse = await this.sender.Send(getUserByIdRequest, CancellationToken.None);

            if (getUserByIdResponse is not null)
            {
                var userString = JsonSerializer.Serialize(getUserByIdResponse, jsonSerializerOptions);

                this.logger.LogInformation("""
                    User found:
                    {userString}
                    """, userString);
            }
            else
            {
                this.logger.LogInformation("No user with id {userId} found.", userId.ToString("D"));
            }
        }

        public async Task FlipCoin()
        {
            await Task.CompletedTask;

            if (Random.Shared.NextDouble() < 0.5)
            {
                this.logger.LogInformation("HEADS");
            }
            else
            {
                this.logger.LogInformation("TAILS");
            }
        }
    }
}
