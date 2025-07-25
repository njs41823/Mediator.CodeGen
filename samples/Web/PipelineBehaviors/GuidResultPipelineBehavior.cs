using Mediator.CodeGen.Contracts;
using Web.Shared;

namespace Web.PipelineBehaviors
{
    internal sealed class GuidResultPipelineBehavior<TRequest> : IPipelineBehavior<TRequest, Result<Guid>>
        where TRequest : IRequest<Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(TRequest request, PipelineBehaviorNextDelegate<Result<Guid>> next, CancellationToken cancellationToken)
        {
            _ = await next();

            return Result<Guid>.Success(Guid.NewGuid());
        }
    }
}
