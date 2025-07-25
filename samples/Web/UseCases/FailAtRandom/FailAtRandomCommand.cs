namespace Web.UseCases.FailAtRandom
{
    internal sealed record FailAtRandomCommand(double? FailureProbability = 0.5) : ICommand;
}
