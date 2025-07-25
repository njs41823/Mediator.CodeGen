using Cli.Entities;
using Mediator.CodeGen.Contracts;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Cli.PipelineBehaviors
{
    internal sealed class UserResponsePipelineBehavior<TRequest>(
        JsonSerializerOptions jsonSerializerOptions,
        ILogger<UserResponsePipelineBehavior<TRequest>> logger)
        : IPipelineBehavior<TRequest, User?>
        where TRequest : IRequest<User?>
    {
        private readonly JsonSerializerOptions jsonSerializerOptions = jsonSerializerOptions;

        private readonly ILogger<UserResponsePipelineBehavior<TRequest>> logger = logger;

        public async Task<User?> Handle(TRequest request, PipelineBehaviorNextDelegate<User?> next, CancellationToken cancellationToken)
        {
            var result = await next();

            if (result is null)
            {
                return null;
            }

            this.logger.LogInformation($"""
                Responding with user:
                {JsonSerializer.Serialize(result, this.jsonSerializerOptions)}
                """);

            return result;
        }
    }
}
