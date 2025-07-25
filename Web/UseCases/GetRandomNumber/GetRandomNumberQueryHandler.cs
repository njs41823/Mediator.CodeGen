using Web.Shared;

namespace Web.UseCases.GetRandomNumber
{
    internal sealed class GetRandomNumberQueryHandler : IQueryHandler<GetRandomNumberQuery, long>
    {
        public async Task<Result<long>> Handle(GetRandomNumberQuery request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            return Result<long>.Success(Random.Shared.NextInt64(request.MinValue, request.MaxValue));
        }
    }
}
