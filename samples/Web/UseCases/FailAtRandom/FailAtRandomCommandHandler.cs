using Web.Shared;

namespace Web.UseCases.FailAtRandom
{
    internal sealed class FailAtRandomCommandHandler : ICommandHandler<FailAtRandomCommand>
    {
        public async Task<Result> Handle(FailAtRandomCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            var failureProbability = request.FailureProbability ?? 0.5;

            if (failureProbability < 0)
            {
                return Result.Failure(Error.Validation(
                    $"Invalid{nameof(request.FailureProbability)}",
                    $"{nameof(request.FailureProbability)} must be greater than or equal to 0."));
            }

            if (failureProbability > 1)
            {
                return Result.Failure(Error.Validation(
                    $"Invalid{nameof(request.FailureProbability)}",
                    $"{nameof(request.FailureProbability)} must be less than or equal to 1."));
            }

            var random = new Random();

            var sample = random.NextDouble();

            return sample >= failureProbability
                ? Result.Success
                : throw new InvalidOperationException();
        }
    }
}
