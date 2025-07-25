using Mediator.CodeGen.Contracts;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using Web.Shared;

namespace Web.PipelineBehaviors
{
    [PipelineBehaviorPriority(0)]
    internal sealed class ExceptionHandlingPipelineBehavior<TRequest, TResponse>(
        IMemoryCache memoryCache,
        JsonSerializerOptions jsonSerializerOptions,
        ILogger<ExceptionHandlingPipelineBehavior<TRequest, TResponse>> logger)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : Result
    {
        private readonly IMemoryCache memoryCache = memoryCache;

        private readonly ILogger<ExceptionHandlingPipelineBehavior<TRequest, TResponse>> logger = logger;

        private readonly JsonSerializerOptions jsonSerializerOptions = jsonSerializerOptions;

        public async Task<TResponse> Handle(TRequest request, PipelineBehaviorNextDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            try
            {
                return await next();
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    ex,
                    """
                    An unhandled exception was thrown during execution.
                    Request Type: {requestType}
                    Request: {request}
                    """,
                    request.GetType().Name,
                    JsonSerializer.Serialize(request, jsonSerializerOptions));

                return this.CreateValidationResult();
            }
        }

        private TResponse CreateValidationResult()
        {
            if (typeof(TResponse) == typeof(Result))
            {
                return (Result.GenericFailure as TResponse)!;
            }

            var failureResult = this.memoryCache.GetOrCreate(
                $"{typeof(ExceptionHandlingPipelineBehavior<,>).FullName}_{typeof(TResponse).FullName}",
                (cacheEntry) =>
                {
                    cacheEntry.AbsoluteExpiration = DateTimeOffset.MaxValue;

                    return (TResponse)typeof(Result<>)
                        .MakeGenericType(typeof(TResponse).GenericTypeArguments)
                        .GetMethod(nameof(Result.Failure))!
                        .Invoke(null, [Error.Generic])!;
                })!;

            return failureResult;
        }
    }
}
