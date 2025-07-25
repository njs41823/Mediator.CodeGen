using Mediator.CodeGen.Contracts;
using System.Text.Json;

namespace Web.PipelineBehaviors
{
    [PipelineBehaviorPriority(1)]
    internal sealed class RequestLoggingPipelineBehavior<TRequest, TResponse>(
        JsonSerializerOptions jsonSerializerOptions,
        ILogger<RequestLoggingPipelineBehavior<TRequest, TResponse>> logger)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<RequestLoggingPipelineBehavior<TRequest, TResponse>> logger = logger;

        private readonly JsonSerializerOptions jsonSerializerOptions = jsonSerializerOptions;

        public async Task<TResponse> Handle(TRequest request, PipelineBehaviorNextDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var response = await next();

            try
            {
                this.logger.LogDebug("""
                    Request Type: {requestType}
                    Request: {request}
                    Response: {response}
                    """,
                    request.GetType().Name,
                    JsonSerializer.Serialize(request, jsonSerializerOptions),
                    JsonSerializer.Serialize(response, jsonSerializerOptions));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to log request/response. Request Type: {request}", request.GetType());
            }

            return response;
        }
    }
}
