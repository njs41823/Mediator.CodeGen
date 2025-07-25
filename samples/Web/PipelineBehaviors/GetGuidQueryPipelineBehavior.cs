using Mediator.CodeGen.Contracts;
using Web.Shared;
using Web.UseCases.GetGuid;

namespace Web.PipelineBehaviors
{
    internal sealed class GetGuidQueryPipelineBehavior : IPipelineBehavior<GetGuidQuery, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(GetGuidQuery request, PipelineBehaviorNextDelegate<Result<Guid>> next, CancellationToken cancellationToken)
        {
            var result = await next();
            
            if (result.IsSuccess && result.Value == Guid.NewGuid())
            {
                throw new Exception("Whoa");
            }

            return result;
        }
    }
}
