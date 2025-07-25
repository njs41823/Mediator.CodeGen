using Web.Shared;

namespace Web.UseCases.GetGuid
{
    internal sealed class GetGuidQueryHandler : IQueryHandler<GetGuidQuery, Guid>
    {
        public async Task<Result<Guid>> Handle(GetGuidQuery request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            return Result<Guid>.Success(Guid.NewGuid());
        }
    }
}
