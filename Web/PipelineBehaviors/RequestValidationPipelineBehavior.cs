using FluentValidation;
using Mediator.CodeGen.Contracts;
using Microsoft.Extensions.Caching.Memory;
using Web.Shared;

namespace Web.PipelineBehaviors
{
    [PipelineBehaviorPriority(2)]
    internal sealed class RequestValidationPipelineBehavior<TRequest, TResponse>(
        IEnumerable<IValidator<TRequest>>? validators,
        IMemoryCache memoryCache) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : Result
    {
        private readonly IValidator<TRequest>[] validators = [.. validators ?? []];

        private readonly IMemoryCache memoryCache = memoryCache;

        public async Task<TResponse> Handle(TRequest request, PipelineBehaviorNextDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (validators.Length == 0)
            {
                return await next();
            }

            var error = validators
                .Select(validator => validator.Validate(request))
                .SelectMany(validationResult => validationResult.Errors)
                .Where(validationFailure => validationFailure is not null)
                .Select(failure => Error.Validation($"Validation.Invalid{failure.PropertyName}", failure.ErrorMessage))
                .Distinct()
                .FirstOrDefault();

            if (error is not null)
            {
                return CreateValidationResult(error);
            }

            return await next();
        }

        private TResponse CreateValidationResult(Error error)
        {
            if (typeof(TResponse) == typeof(Result))
            {
                return (Result.Failure(error) as TResponse)!;
            }

            var failureResult = this.memoryCache.GetOrCreate(
                $"{typeof(ExceptionHandlingPipelineBehavior<,>).FullName}_{typeof(TResponse).FullName}_{error.Code}_{error.Description}",
                (cacheEntry) =>
                {
                    cacheEntry.AbsoluteExpiration = DateTimeOffset.MaxValue;

                    return (TResponse)typeof(Result<>)
                        .MakeGenericType(typeof(TResponse).GenericTypeArguments)
                        .GetMethod(nameof(Result.Failure))!
                        .Invoke(null, [error])!;
                })!;

            return failureResult;
        }
    }
}
