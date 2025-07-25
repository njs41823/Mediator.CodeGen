using Web.Shared;

namespace Web.UseCases.FailAtRandom
{
    internal sealed class FailAtRandomCommandHandler : ICommandHandler<FailAtRandomCommand>
    {
        public async Task<Result> Handle(FailAtRandomCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            var random = new Random();

            var sample = random.NextDouble();

            return sample >= (request.FailureProbability ?? 0.5)
                ? Result.Success
                : throw new InvalidOperationException();
        }
    }
}
